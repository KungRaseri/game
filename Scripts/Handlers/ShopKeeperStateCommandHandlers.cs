#nullable enable

using Game.Core.CQS;
using Game.Scripts.Commands;
using Game.Scripts.Systems;
using Game.Core.Utils;

namespace Game.Scripts.Handlers;

/// <summary>
/// Handler for starting herb gathering activities.
/// </summary>
public class StartGatheringHerbsCommandHandler : ICommandHandler<StartGatheringHerbsCommand>
{
    private readonly ShopKeeperStateSystem _stateSystem;

    public StartGatheringHerbsCommandHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task HandleAsync(StartGatheringHerbsCommand command, CancellationToken cancellationToken = default)
    {
        var result = _stateSystem.StartGatheringHerbs(command.DurationMinutes, command.EfficiencyMultiplier);
        
        if (!result)
        {
            throw new InvalidOperationException($"Cannot start gathering herbs. Current state: {_stateSystem.GetCurrentState().CurrentState}");
        }

        GameLogger.Info($"Started gathering herbs for {command.DurationMinutes} minutes with {command.EfficiencyMultiplier}x efficiency");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for starting potion crafting activities.
/// </summary>
public class StartCraftingPotionsCommandHandler : ICommandHandler<StartCraftingPotionsCommand>
{
    private readonly ShopKeeperStateSystem _stateSystem;

    public StartCraftingPotionsCommandHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task HandleAsync(StartCraftingPotionsCommand command, CancellationToken cancellationToken = default)
    {
        var result = _stateSystem.StartCraftingPotions(command.RecipeId, command.EfficiencyMultiplier);
        
        if (!result)
        {
            var currentState = _stateSystem.GetCurrentState();
            var (herbs, potions) = _stateSystem.GetResourceCounts();
            throw new InvalidOperationException($"Cannot start crafting potions. Current state: {currentState.CurrentState}, Available herbs: {herbs}");
        }

        GameLogger.Info($"Started crafting potions using recipe {command.RecipeId} with {command.EfficiencyMultiplier}x efficiency");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for starting shop operations.
/// </summary>
public class StartRunningShopCommandHandler : ICommandHandler<StartRunningShopCommand>
{
    private readonly ShopKeeperStateSystem _stateSystem;

    public StartRunningShopCommandHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task HandleAsync(StartRunningShopCommand command, CancellationToken cancellationToken = default)
    {
        var result = _stateSystem.StartRunningShop(command.DurationMinutes, command.PriceMultiplier);
        
        if (!result)
        {
            var currentState = _stateSystem.GetCurrentState();
            var (herbs, potions) = _stateSystem.GetResourceCounts();
            throw new InvalidOperationException($"Cannot start running shop. Current state: {currentState.CurrentState}, Available potions: {potions}");
        }

        var durationText = command.DurationMinutes > 0 ? $" for {command.DurationMinutes} minutes" : " until stock depleted";
        GameLogger.Info($"Started running shop{durationText} with {command.PriceMultiplier}x price multiplier");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for stopping current activities.
/// </summary>
public class StopCurrentActivityCommandHandler : ICommandHandler<StopCurrentActivityCommand>
{
    private readonly ShopKeeperStateSystem _stateSystem;

    public StopCurrentActivityCommandHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task HandleAsync(StopCurrentActivityCommand command, CancellationToken cancellationToken = default)
    {
        var currentState = _stateSystem.GetCurrentState();
        _stateSystem.StopCurrentActivity(command.Reason);
        
        GameLogger.Info($"Stopped {currentState.CurrentState} activity: {command.Reason}");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for forced state transitions (debugging/testing).
/// </summary>
public class ForceStateTransitionCommandHandler : ICommandHandler<ForceStateTransitionCommand>
{
    private readonly ShopKeeperStateSystem _stateSystem;

    public ForceStateTransitionCommandHandler(ShopKeeperStateSystem stateSystem)
    {
        _stateSystem = stateSystem ?? throw new ArgumentNullException(nameof(stateSystem));
    }

    public async Task HandleAsync(ForceStateTransitionCommand command, CancellationToken cancellationToken = default)
    {
        // First stop current activity
        _stateSystem.StopCurrentActivity($"Forced transition: {command.Reason}");

        // Attempt to start the target activity based on state
        switch (command.TargetState)
        {
            case Models.ShopKeeperState.GatheringHerbs:
                _stateSystem.StartGatheringHerbs();
                break;
            
            case Models.ShopKeeperState.CraftingPotions:
                _stateSystem.StartCraftingPotions();
                break;
            
            case Models.ShopKeeperState.RunningShop:
                _stateSystem.StartRunningShop();
                break;
            
            case Models.ShopKeeperState.Idle:
                // Already handled by StopCurrentActivity
                break;
        }

        GameLogger.Warning($"Forced state transition to {command.TargetState}: {command.Reason}");
        await Task.CompletedTask;
    }
}
