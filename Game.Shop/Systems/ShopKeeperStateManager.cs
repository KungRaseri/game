#nullable enable

using Game.Core.Utils;
using Game.Shop.Models;
using Stateless;

namespace Game.Shop.Systems;

/// <summary>
/// Advanced ShopKeeper state management system using Stateless state machine.
/// Enforces single-activity constraints with robust state transitions and validation.
/// </summary>
public class ShopKeeperStateManager : IDisposable
{
    private readonly StateMachine<ShopKeeperState, ShopKeeperTrigger> _stateMachine;
    private readonly System.Threading.Timer _updateTimer;
    private ShopKeeperStateInfo _stateInfo;
    private readonly object _stateLock = new();

    // Mock resource counts for demonstration
    private int _herbCount = 0;
    private int _potionCount = 0;

    // Production rates (per second)
    private const float BASE_HERB_PRODUCTION_RATE = 0.5f;      // 30 herbs per minute
    private const float BASE_POTION_CRAFTING_RATE = 0.25f;    // 15 potions per minute
    private const float BASE_POTION_SALE_RATE = 0.33f;        // 20 potions per minute

    /// <summary>
    /// Triggers that can cause state transitions.
    /// </summary>
    public enum ShopKeeperTrigger
    {
        StartGathering,
        StartCrafting,
        StartRunningShop,
        StopActivity,
        CompleteDuration,
        ResourcesDepleted,
        ForceTransition
    }

    /// <summary>
    /// Event fired when state changes occur.
    /// </summary>
    public event Action<ShopKeeperState, ShopKeeperState, ShopKeeperTrigger>? StateChanged;

    /// <summary>
    /// Event fired when activity progress updates.
    /// </summary>
    public event Action<ShopKeeperStateInfo>? ProgressUpdated;

    public ShopKeeperStateManager()
    {
        _stateInfo = new ShopKeeperStateInfo();
        
        // Configure the state machine
        _stateMachine = new StateMachine<ShopKeeperState, ShopKeeperTrigger>(
            () => _stateInfo.CurrentState, 
            state => _stateInfo.CurrentState = state);

        ConfigureStateMachine();
        
        // Update state every second
        _updateTimer = new System.Threading.Timer(UpdateState, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        
        GameLogger.Info("ShopKeeperStateManager initialized with Stateless state machine");
    }

    /// <summary>
    /// Configures the state machine transitions and behaviors.
    /// </summary>
    private void ConfigureStateMachine()
    {
        // Configure Idle state
        _stateMachine.Configure(ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.StartGathering, ShopKeeperState.GatheringHerbs)
            .PermitIf(ShopKeeperTrigger.StartCrafting, ShopKeeperState.CraftingPotions, () => _herbCount > 0)
            .PermitIf(ShopKeeperTrigger.StartRunningShop, ShopKeeperState.RunningShop, () => _potionCount > 0)
            .OnEntry(OnIdleEntry);

        // Configure GatheringHerbs state
        _stateMachine.Configure(ShopKeeperState.GatheringHerbs)
            .Permit(ShopKeeperTrigger.StopActivity, ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.CompleteDuration, ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.ForceTransition, ShopKeeperState.Idle)
            .OnEntry(OnGatheringEntry)
            .OnExit(OnGatheringExit);

        // Configure CraftingPotions state
        _stateMachine.Configure(ShopKeeperState.CraftingPotions)
            .Permit(ShopKeeperTrigger.StopActivity, ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.ResourcesDepleted, ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.ForceTransition, ShopKeeperState.Idle)
            .OnEntry(OnCraftingEntry)
            .OnExit(OnCraftingExit);

        // Configure RunningShop state
        _stateMachine.Configure(ShopKeeperState.RunningShop)
            .Permit(ShopKeeperTrigger.StopActivity, ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.ResourcesDepleted, ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.CompleteDuration, ShopKeeperState.Idle)
            .Permit(ShopKeeperTrigger.ForceTransition, ShopKeeperState.Idle)
            .OnEntry(OnShopEntry)
            .OnExit(OnShopExit);

        // Global transition event
        _stateMachine.OnTransitioned(transition => 
        {
            GameLogger.Info($"State transition: {transition.Source} -> {transition.Destination} (Trigger: {transition.Trigger})");
            StateChanged?.Invoke(transition.Source, transition.Destination, transition.Trigger);
        });
    }

    /// <summary>
    /// Gets the current state information.
    /// </summary>
    public ShopKeeperStateInfo GetCurrentState()
    {
        lock (_stateLock)
        {
            return _stateInfo;
        }
    }

    /// <summary>
    /// Gets current resource counts.
    /// </summary>
    public (int herbs, int potions) GetResourceCounts()
    {
        return (_herbCount, _potionCount);
    }

    /// <summary>
    /// Starts gathering herbs if valid transition.
    /// </summary>
    public bool StartGatheringHerbs(int durationMinutes = 5, float efficiencyMultiplier = 1.0f)
    {
        try
        {
            lock (_stateLock)
            {
                if (!_stateMachine.CanFire(ShopKeeperTrigger.StartGathering))
                {
                    GameLogger.Warning($"Cannot start gathering herbs from state {_stateInfo.CurrentState}");
                    return false;
                }

                // Set up activity parameters before firing trigger
                _stateInfo.ActivityDuration = TimeSpan.FromMinutes(durationMinutes);
                _stateInfo.CurrentProductionRate = BASE_HERB_PRODUCTION_RATE * efficiencyMultiplier;
                
                _stateMachine.Fire(ShopKeeperTrigger.StartGathering);
                return true;
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error starting herb gathering");
            return false;
        }
    }

    /// <summary>
    /// Starts crafting potions if valid transition.
    /// </summary>
    public bool StartCraftingPotions(string recipeId = "basic_healing_potion", float efficiencyMultiplier = 1.0f)
    {
        try
        {
            lock (_stateLock)
            {
                if (!_stateMachine.CanFire(ShopKeeperTrigger.StartCrafting))
                {
                    GameLogger.Warning($"Cannot start crafting potions from state {_stateInfo.CurrentState} (herbs: {_herbCount})");
                    return false;
                }

                // Set up activity parameters before firing trigger
                _stateInfo.CurrentProductionRate = BASE_POTION_CRAFTING_RATE * efficiencyMultiplier;
                _stateInfo.AvailableResources = _herbCount;
                
                _stateMachine.Fire(ShopKeeperTrigger.StartCrafting);
                return true;
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error starting potion crafting");
            return false;
        }
    }

    /// <summary>
    /// Starts running the shop if valid transition.
    /// </summary>
    public bool StartRunningShop(int durationMinutes = 0, float priceMultiplier = 1.0f)
    {
        try
        {
            lock (_stateLock)
            {
                if (!_stateMachine.CanFire(ShopKeeperTrigger.StartRunningShop))
                {
                    GameLogger.Warning($"Cannot start running shop from state {_stateInfo.CurrentState} (potions: {_potionCount})");
                    return false;
                }

                // Set up activity parameters before firing trigger
                _stateInfo.ActivityDuration = durationMinutes > 0 ? TimeSpan.FromMinutes(durationMinutes) : null;
                _stateInfo.CurrentProductionRate = BASE_POTION_SALE_RATE;
                _stateInfo.AvailableResources = _potionCount;
                
                _stateMachine.Fire(ShopKeeperTrigger.StartRunningShop);
                return true;
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error starting shop operations");
            return false;
        }
    }

    /// <summary>
    /// Stops the current activity.
    /// </summary>
    public bool StopCurrentActivity(string reason = "Player requested")
    {
        try
        {
            lock (_stateLock)
            {
                if (_stateInfo.CurrentState == ShopKeeperState.Idle)
                {
                    return true; // Already idle
                }

                if (_stateMachine.CanFire(ShopKeeperTrigger.StopActivity))
                {
                    GameLogger.Info($"Stopping {_stateInfo.CurrentState} activity - {reason}");
                    _stateMachine.Fire(ShopKeeperTrigger.StopActivity);
                    return true;
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error stopping current activity");
            return false;
        }
    }

    /// <summary>
    /// Gets the list of triggers that can be fired in the current state.
    /// </summary>
    public async Task<IEnumerable<ShopKeeperTrigger>> GetPermittedTriggersAsync()
    {
        return await _stateMachine.GetPermittedTriggersAsync();
    }

    /// <summary>
    /// Checks if a specific trigger can be fired.
    /// </summary>
    public bool CanFire(ShopKeeperTrigger trigger)
    {
        return _stateMachine.CanFire(trigger);
    }

    #region State Entry/Exit Handlers

    private void OnIdleEntry()
    {
        lock (_stateLock)
        {
            _stateInfo.ResetToIdle();
        }
    }

    private void OnGatheringEntry()
    {
        lock (_stateLock)
        {
            _stateInfo.ActivityStartTime = DateTime.UtcNow;
            _stateInfo.StatusMessage = $"Gathering herbs for {_stateInfo.ActivityDuration?.TotalMinutes:F0} minutes...";
            _stateInfo.CanContinue = true;
            _stateInfo.TotalProcessedInSession = 0;
        }
    }

    private void OnGatheringExit()
    {
        GameLogger.Info($"Herb gathering session completed. Total gathered: {_stateInfo.TotalProcessedInSession}");
    }

    private void OnCraftingEntry()
    {
        lock (_stateLock)
        {
            _stateInfo.ActivityStartTime = DateTime.UtcNow;
            _stateInfo.ActivityDuration = null; // Runs until herbs depleted
            _stateInfo.StatusMessage = $"Crafting potions from {_herbCount} herbs...";
            _stateInfo.CanContinue = true;
            _stateInfo.TotalProcessedInSession = 0;
        }
    }

    private void OnCraftingExit()
    {
        GameLogger.Info($"Potion crafting session completed. Total crafted: {_stateInfo.TotalProcessedInSession}");
    }

    private void OnShopEntry()
    {
        lock (_stateLock)
        {
            _stateInfo.ActivityStartTime = DateTime.UtcNow;
            var durationText = _stateInfo.ActivityDuration?.TotalMinutes.ToString("F0") ?? "unlimited";
            _stateInfo.StatusMessage = $"Running shop for {durationText} minutes with {_potionCount} potions...";
            _stateInfo.CanContinue = true;
            _stateInfo.TotalProcessedInSession = 0;
        }
    }

    private void OnShopExit()
    {
        GameLogger.Info($"Shop operation completed. Total sold: {_stateInfo.TotalProcessedInSession}");
    }

    #endregion

    #region Update Logic

    /// <summary>
    /// Periodic update method called by timer to progress activities.
    /// </summary>
    private void UpdateState(object? state)
    {
        try
        {
            lock (_stateLock)
            {
                switch (_stateInfo.CurrentState)
                {
                    case ShopKeeperState.GatheringHerbs:
                        UpdateGatheringProgress();
                        break;

                    case ShopKeeperState.CraftingPotions:
                        UpdateCraftingProgress();
                        break;

                    case ShopKeeperState.RunningShop:
                        UpdateShopProgress();
                        break;
                }

                ProgressUpdated?.Invoke(_stateInfo);
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error updating ShopKeeper state");
        }
    }

    private void UpdateGatheringProgress()
    {
        if (!_stateInfo.ActivityStartTime.HasValue || !_stateInfo.ActivityDuration.HasValue)
            return;

        var elapsed = DateTime.UtcNow - _stateInfo.ActivityStartTime.Value;
        var progress = (float)(elapsed.TotalSeconds / _stateInfo.ActivityDuration.Value.TotalSeconds);

        // Produce herbs based on production rate
        var herbsToGenerate = Math.Max(1, (int)(_stateInfo.CurrentProductionRate * 1.0f));
        _herbCount += herbsToGenerate;
        _stateInfo.TotalProcessedInSession += herbsToGenerate;
        
        var statusMessage = $"Gathering herbs... {_stateInfo.TotalProcessedInSession} herbs collected (Total: {_herbCount})";
        _stateInfo.UpdateProgress(progress, _stateInfo.TotalProcessedInSession, 0, statusMessage);

        // Check if activity should end
        if (progress >= 1.0f && _stateMachine.CanFire(ShopKeeperTrigger.CompleteDuration))
        {
            _stateMachine.Fire(ShopKeeperTrigger.CompleteDuration);
        }
    }

    private void UpdateCraftingProgress()
    {
        if (_herbCount <= 0)
        {
            if (_stateMachine.CanFire(ShopKeeperTrigger.ResourcesDepleted))
            {
                _stateMachine.Fire(ShopKeeperTrigger.ResourcesDepleted);
            }
            return;
        }

        // Simulate potion crafting (consume herbs, produce potions)
        var potionsToMake = Math.Max(1, (int)(_stateInfo.CurrentProductionRate * 1.0f));
        var herbsToConsume = Math.Min(potionsToMake * 2, _herbCount); // 2 herbs per potion
        var actualPotions = herbsToConsume / 2;

        if (actualPotions > 0)
        {
            _herbCount -= herbsToConsume;
            _potionCount += actualPotions;
            _stateInfo.TotalProcessedInSession += actualPotions;
            
            var progress = _herbCount > 0 ? 1.0f - ((float)_herbCount / _stateInfo.AvailableResources) : 1.0f;
            var statusMessage = $"Crafting potions... {_stateInfo.TotalProcessedInSession} potions crafted, {_herbCount} herbs remaining (Total potions: {_potionCount})";
            
            _stateInfo.UpdateProgress(progress, _stateInfo.TotalProcessedInSession, _herbCount, statusMessage);
        }
    }

    private void UpdateShopProgress()
    {
        if (_potionCount <= 0)
        {
            if (_stateMachine.CanFire(ShopKeeperTrigger.ResourcesDepleted))
            {
                _stateMachine.Fire(ShopKeeperTrigger.ResourcesDepleted);
            }
            return;
        }

        // Simulate potion sales
        var potionsToSell = Math.Max(1, (int)(_stateInfo.CurrentProductionRate * 1.0f));
        potionsToSell = Math.Min(potionsToSell, _potionCount);

        if (potionsToSell > 0)
        {
            _potionCount -= potionsToSell;
            _stateInfo.TotalProcessedInSession += potionsToSell;
            
            var progress = _potionCount > 0 ? 1.0f - ((float)_potionCount / _stateInfo.AvailableResources) : 1.0f;
            var statusMessage = $"Running shop... {_stateInfo.TotalProcessedInSession} potions sold, {_potionCount} potions remaining";
            
            _stateInfo.UpdateProgress(progress, _stateInfo.TotalProcessedInSession, _potionCount, statusMessage);
        }

        // Check duration limit
        if (_stateInfo.ActivityDuration.HasValue && _stateInfo.ActivityStartTime.HasValue)
        {
            var elapsed = DateTime.UtcNow - _stateInfo.ActivityStartTime.Value;
            if (elapsed >= _stateInfo.ActivityDuration.Value && _stateMachine.CanFire(ShopKeeperTrigger.CompleteDuration))
            {
                _stateMachine.Fire(ShopKeeperTrigger.CompleteDuration);
            }
        }
    }

    #endregion

    /// <summary>
    /// Disposes of resources used by the state manager.
    /// </summary>
    public void Dispose()
    {
        _updateTimer?.Dispose();
        GameLogger.Info("ShopKeeperStateManager disposed");
    }
}
