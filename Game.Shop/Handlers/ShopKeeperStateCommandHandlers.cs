#nullable enable

using Game.Core.CQS;
using Game.Shop.Commands;
using Game.Shop.Systems;
using Game.Core.Utils;

namespace Game.Shop.Handlers;

/// <summary>
/// Handler for starting herb gathering activities.
/// </summary>
public class StartGatheringHerbsCommandHandler : ICommandHandler<StartGatheringHerbsCommand>
{
    private readonly ShopKeeperStateManager _stateManager;

    public StartGatheringHerbsCommandHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task HandleAsync(StartGatheringHerbsCommand command, CancellationToken cancellationToken = default)
    {
        var result = _stateManager.StartGatheringHerbs(command.DurationMinutes, command.EfficiencyMultiplier);
        
        if (!result)
        {
            throw new InvalidOperationException($"Cannot start gathering herbs. Current state: {_stateManager.GetCurrentState().CurrentState}");
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
    private readonly ShopKeeperStateManager _stateManager;

    public StartCraftingPotionsCommandHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task HandleAsync(StartCraftingPotionsCommand command, CancellationToken cancellationToken = default)
    {
        var result = _stateManager.StartCraftingPotions(command.RecipeId, command.EfficiencyMultiplier);
        
        if (!result)
        {
            var currentState = _stateManager.GetCurrentState();
            var (herbs, potions) = _stateManager.GetResourceCounts();
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
    private readonly ShopKeeperStateManager _stateManager;

    public StartRunningShopCommandHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task HandleAsync(StartRunningShopCommand command, CancellationToken cancellationToken = default)
    {
        var result = _stateManager.StartRunningShop(command.DurationMinutes, command.PriceMultiplier);
        
        if (!result)
        {
            var currentState = _stateManager.GetCurrentState();
            var (herbs, potions) = _stateManager.GetResourceCounts();
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
    private readonly ShopKeeperStateManager _stateManager;

    public StopCurrentActivityCommandHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task HandleAsync(StopCurrentActivityCommand command, CancellationToken cancellationToken = default)
    {
        var currentState = _stateManager.GetCurrentState();
        _stateManager.StopCurrentActivity(command.Reason);
        
        GameLogger.Info($"Stopped {currentState.CurrentState} activity: {command.Reason}");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Handler for forced state transitions (debugging/testing).
/// </summary>
public class ForceStateTransitionCommandHandler : ICommandHandler<ForceStateTransitionCommand>
{
    private readonly ShopKeeperStateManager _stateManager;

    public ForceStateTransitionCommandHandler(ShopKeeperStateManager stateManager)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }

    public async Task HandleAsync(ForceStateTransitionCommand command, CancellationToken cancellationToken = default)
    {
        // First stop current activity
        _stateManager.StopCurrentActivity($"Forced transition: {command.Reason}");

        // Attempt to start the target activity based on state
        switch (command.TargetState)
        {
            case Models.ShopKeeperState.GatheringHerbs:
                _stateManager.StartGatheringHerbs();
                break;
            
            case Models.ShopKeeperState.CraftingPotions:
                _stateManager.StartCraftingPotions();
                break;
            
            case Models.ShopKeeperState.RunningShop:
                _stateManager.StartRunningShop();
                break;
            
            case Models.ShopKeeperState.Idle:
                // Already handled by StopCurrentActivity
                break;
        }

        GameLogger.Warning($"Forced state transition to {command.TargetState}: {command.Reason}");
        await Task.CompletedTask;
    }
}
