#nullable enable

using Game.Core.CQS;
using Game.Scripts.Queries;
using Game.Scripts.Systems;
using Game.Scripts.Models;
using Game.Core.Utils;

namespace Game.Scripts.Handlers;

/// <summary>
/// Handler for getting current ShopKeeper state information.
/// </summary>
public class GetShopKeeperStateQueryHandler : IQueryHandler<GetShopKeeperStateQuery, ShopKeeperStateInfo>
{
    private readonly ShopKeeperStateSystem _stateSystem;

    public GetShopKeeperStateQueryHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task<ShopKeeperStateInfo> HandleAsync(GetShopKeeperStateQuery query, CancellationToken cancellationToken = default)
    {
        var state = _stateSystem.GetCurrentState();
        
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
    private readonly ShopKeeperStateSystem _stateSystem;

    public CanTransitionToStateQueryHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task<bool> HandleAsync(CanTransitionToStateQuery query, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_stateSystem.CanTransitionToState(query.TargetState, query.IgnoreResourceRequirements));
    }
}

/// <summary>
/// Handler for getting available activities.
/// </summary>
public class GetAvailableActivitiesQueryHandler : IQueryHandler<GetAvailableActivitiesQuery, AvailableActivitiesResult>
{
    private readonly ShopKeeperStateSystem _stateSystem;

    public GetAvailableActivitiesQueryHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task<AvailableActivitiesResult> HandleAsync(GetAvailableActivitiesQuery query, CancellationToken cancellationToken = default)
    {
        var currentState = _stateSystem.GetCurrentState();
        var (availableHerbs, availablePotions) = _stateSystem.GetResourceCounts();
        var unavailabilityReasons = new Dictionary<ShopKeeperState, string>();

        // Check if player is currently busy
        bool isIdle = currentState.CurrentState == ShopKeeperState.Idle;
        if (!isIdle)
        {
            unavailabilityReasons[ShopKeeperState.GatheringHerbs] = $"Currently {currentState.CurrentState}";
            unavailabilityReasons[ShopKeeperState.CraftingPotions] = $"Currently {currentState.CurrentState}";
            unavailabilityReasons[ShopKeeperState.RunningShop] = $"Currently {currentState.CurrentState}";
        }

        // Determine what activities are available
        bool canGatherHerbs = isIdle; // Can always gather herbs if idle
        bool canCraftPotions = isIdle && availableHerbs > 0;
        bool canRunShop = isIdle && availablePotions > 0;

        // Add resource-based unavailability reasons
        if (isIdle)
        {
            if (availableHerbs <= 0)
            {
                unavailabilityReasons[ShopKeeperState.CraftingPotions] = "No herbs available";
            }
            if (availablePotions <= 0)
            {
                unavailabilityReasons[ShopKeeperState.RunningShop] = "No potions available";
            }
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
    private readonly ShopKeeperStateSystem _stateSystem;

    public GetActivityStatisticsQueryHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task<ActivityStatisticsResult> HandleAsync(GetActivityStatisticsQuery query, CancellationToken cancellationToken = default)
    {
        // For now, return mock statistics
        // In a full implementation, this would query from a statistics tracking system
        var currentState = _stateSystem.GetCurrentState();
        
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
