#nullable enable

using Game.Core.CQS;
using Game.Core.Commands;
using Game.Core.Systems;
using Game.Core.Utils;

namespace Game.Core.Handlers;

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
        var result = await _stateSystem.StartGatheringHerbsAsync(command.DurationMinutes, command.EfficiencyMultiplier);
        
        if (!result)
        {
            throw new InvalidOperationException($"Cannot start gathering herbs. Current state: {_stateSystem.GetCurrentState().CurrentState}");
        }

        GameLogger.Info($"Started gathering herbs for {command.DurationMinutes} minutes with {command.EfficiencyMultiplier}x efficiency");
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
        var result = await _stateSystem.StartCraftingPotionsAsync(command.RecipeId, command.EfficiencyMultiplier, command.MaxPotionsToCraft);
        
        if (!result)
        {
            var currentState = _stateSystem.GetCurrentState();
            throw new InvalidOperationException($"Cannot start crafting potions. Current state: {currentState.CurrentState}, Available resources: {currentState.AvailableResources}");
        }

        GameLogger.Info($"Started crafting potions using recipe {command.RecipeId} with {command.EfficiencyMultiplier}x efficiency");
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
        var result = await _stateSystem.StartRunningShopAsync(command.DurationMinutes, command.PriceMultiplier, command.AutoCloseWhenEmpty);
        
        if (!result)
        {
            var currentState = _stateSystem.GetCurrentState();
            throw new InvalidOperationException($"Cannot start running shop. Current state: {currentState.CurrentState}, Available resources: {currentState.AvailableResources}");
        }

        var durationText = command.DurationMinutes > 0 ? $" for {command.DurationMinutes} minutes" : " until stock depleted";
        GameLogger.Info($"Started running shop{durationText} with {command.PriceMultiplier}x price multiplier");
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
        await _stateSystem.StopCurrentActivityAsync(command.Reason);
        
        GameLogger.Info($"Stopped {currentState.CurrentState} activity: {command.Reason}");
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
        await _stateSystem.StopCurrentActivityAsync($"Forced transition: {command.Reason}");

        // Attempt to start the target activity based on state
        switch (command.TargetState)
        {
            case Models.ShopKeeperState.GatheringHerbs:
                await _stateSystem.StartGatheringHerbsAsync();
                break;
            
            case Models.ShopKeeperState.CraftingPotions:
                await _stateSystem.StartCraftingPotionsAsync();
                break;
            
            case Models.ShopKeeperState.RunningShop:
                await _stateSystem.StartRunningShopAsync();
                break;
            
            case Models.ShopKeeperState.Idle:
                // Already handled by StopCurrentActivityAsync
                break;
        }

        GameLogger.Warning($"Forced state transition to {command.TargetState}: {command.Reason}");
    }
}
