#nullable enable

using Game.Core.CQS;
using Game.Shop.Queries;
using Game.Shop.Systems;
using Game.Shop.Models;
using Game.Core.Utils;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for getting current ShopKeeper state information.
/// </summary>
public class GetShopKeeperStateQueryHandler : IQueryHandler<GetShopKeeperStateQuery, ShopKeeperStateInfo>
{
    private readonly ShopKeeperStateManager _stateManager;

    public GetShopKeeperStateQueryHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task<ShopKeeperStateInfo> HandleAsync(GetShopKeeperStateQuery query, CancellationToken cancellationToken = default)
    {
        var state = _stateManager.GetCurrentState();
        
        if (query.IncludeTimeEstimates && state.ActivityStartTime.HasValue)
        {
            // Update time estimates in real-time
            if (state.ActivityDuration.HasValue)
            {
                var elapsed = DateTime.UtcNow - state.ActivityStartTime.Value;
                var remaining = state.ActivityDuration.Value - elapsed;
                state.EstimatedTimeRemaining = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }

        return await Task.FromResult(state);
    }
}

/// <summary>
/// Handler for checking if state transitions are allowed.
/// </summary>
public class CanTransitionToStateQueryHandler : IQueryHandler<CanTransitionToStateQuery, bool>
{
    private readonly ShopKeeperStateManager _stateManager;

    public CanTransitionToStateQueryHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task<bool> HandleAsync(CanTransitionToStateQuery query, CancellationToken cancellationToken = default)
    {
        var trigger = query.TargetState switch
        {
            ShopKeeperState.GatheringHerbs => ShopKeeperStateManager.ShopKeeperTrigger.StartGathering,
            ShopKeeperState.CraftingPotions => ShopKeeperStateManager.ShopKeeperTrigger.StartCrafting,
            ShopKeeperState.RunningShop => ShopKeeperStateManager.ShopKeeperTrigger.StartRunningShop,
            ShopKeeperState.Idle => ShopKeeperStateManager.ShopKeeperTrigger.StopActivity,
            _ => throw new ArgumentOutOfRangeException(nameof(query.TargetState))
        };

        var canFire = _stateManager.CanFire(trigger);
        
        // If ignoring resource requirements and the basic transition is not allowed,
        // check if it's only due to resource constraints
        if (!canFire && query.IgnoreResourceRequirements)
        {
            var currentState = _stateManager.GetCurrentState();
            canFire = currentState.CurrentState == ShopKeeperState.Idle;
        }

        return await Task.FromResult(canFire);
    }
}

/// <summary>
/// Handler for getting available activities.
/// </summary>
public class GetAvailableActivitiesQueryHandler : IQueryHandler<GetAvailableActivitiesQuery, AvailableActivitiesResult>
{
    private readonly ShopKeeperStateManager _stateManager;

    public GetAvailableActivitiesQueryHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task<AvailableActivitiesResult> HandleAsync(GetAvailableActivitiesQuery query, CancellationToken cancellationToken = default)
    {
        var currentState = _stateManager.GetCurrentState();
        var (availableHerbs, availablePotions) = _stateManager.GetResourceCounts();
        var unavailabilityReasons = new Dictionary<ShopKeeperState, string>();

        // Check what triggers can be fired
        var permittedTriggers = (await _stateManager.GetPermittedTriggersAsync()).ToList();
        
        // Determine what activities are available
        bool canGatherHerbs = permittedTriggers.Contains(ShopKeeperStateManager.ShopKeeperTrigger.StartGathering);
        bool canCraftPotions = permittedTriggers.Contains(ShopKeeperStateManager.ShopKeeperTrigger.StartCrafting);
        bool canRunShop = permittedTriggers.Contains(ShopKeeperStateManager.ShopKeeperTrigger.StartRunningShop);

        // Add unavailability reasons
        if (!canGatherHerbs)
        {
            unavailabilityReasons[ShopKeeperState.GatheringHerbs] = 
                currentState.CurrentState != ShopKeeperState.Idle 
                    ? $"Currently {currentState.CurrentState}" 
                    : "Cannot gather herbs in current state";
        }

        if (!canCraftPotions)
        {
            unavailabilityReasons[ShopKeeperState.CraftingPotions] = 
                currentState.CurrentState != ShopKeeperState.Idle 
                    ? $"Currently {currentState.CurrentState}"
                    : availableHerbs <= 0 
                        ? "No herbs available" 
                        : "Cannot craft potions in current state";
        }

        if (!canRunShop)
        {
            unavailabilityReasons[ShopKeeperState.RunningShop] = 
                currentState.CurrentState != ShopKeeperState.Idle 
                    ? $"Currently {currentState.CurrentState}"
                    : availablePotions <= 0 
                        ? "No potions available" 
                        : "Cannot run shop in current state";
        }

        return await Task.FromResult(new AvailableActivitiesResult
        {
            CanGatherHerbs = canGatherHerbs,
            CanCraftPotions = canCraftPotions,
            CanRunShop = canRunShop,
            AvailableHerbs = availableHerbs,
            AvailablePotions = availablePotions,
            UnavailabilityReasons = unavailabilityReasons,
            CurrentState = currentState.CurrentState
        });
    }
}

/// <summary>
/// Handler for getting activity statistics.
/// </summary>
public class GetActivityStatisticsQueryHandler : IQueryHandler<GetActivityStatisticsQuery, ActivityStatisticsResult>
{
    private readonly ShopKeeperStateManager _stateManager;

    public GetActivityStatisticsQueryHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task<ActivityStatisticsResult> HandleAsync(GetActivityStatisticsQuery query, CancellationToken cancellationToken = default)
    {
        // For now, return mock statistics based on current session
        // In a full implementation, this would query from a statistics tracking system
        var currentState = _stateManager.GetCurrentState();
        
        return await Task.FromResult(new ActivityStatisticsResult
        {
            TotalHerbsGathered = currentState.CurrentState == ShopKeeperState.GatheringHerbs ? currentState.TotalProcessedInSession : 0,
            TotalPotionsCrafted = currentState.CurrentState == ShopKeeperState.CraftingPotions ? currentState.TotalProcessedInSession : 0,
            TotalPotionsSold = currentState.CurrentState == ShopKeeperState.RunningShop ? currentState.TotalProcessedInSession : 0,
            TotalGatheringTimeMinutes = 0, // Would be tracked by statistics system
            TotalCraftingTimeMinutes = 0,  // Would be tracked by statistics system
            TotalShopTimeMinutes = 0,      // Would be tracked by statistics system
            HerbsPerMinute = currentState.CurrentProductionRate * 60, // Convert per-second to per-minute
            PotionsPerMinute = currentState.CurrentProductionRate * 60,
            SalesPerMinute = currentState.CurrentProductionRate * 60,
            MostEfficientActivity = ShopKeeperState.GatheringHerbs // Mock data
        });
    }
}
