#nullable enable

using Game.Core.Utils;
using Game.Scripts.Models;

namespace Game.Scripts.Systems;

/// <summary>
/// Simple ShopKeeper state management system that tracks the player's current activity.
/// Enforces single-activity constraints and provides basic state transitions.
/// </summary>
public class ShopKeeperStateSystem
{
    private ShopKeeperStateInfo _currentState;
    private readonly object _stateLock = new();
    private readonly System.Threading.Timer _updateTimer;

    // Mock resource counts for demonstration
    private int _herbCount = 0;
    private int _potionCount = 0;

    // Production rates (per second)
    private const float BASE_HERB_PRODUCTION_RATE = 0.5f;      // 30 herbs per minute
    private const float BASE_POTION_CRAFTING_RATE = 0.25f;    // 15 potions per minute
    private const float BASE_POTION_SALE_RATE = 0.33f;        // 20 potions per minute

    public ShopKeeperStateSystem()
    {
        _currentState = new ShopKeeperStateInfo();
        
        // Update state every second
        _updateTimer = new System.Threading.Timer(UpdateState, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        
        GameLogger.Info("ShopKeeperStateSystem initialized");
    }

    /// <summary>
    /// Gets the current state information.
    /// </summary>
    public ShopKeeperStateInfo GetCurrentState()
    {
        lock (_stateLock)
        {
            return _currentState;
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
    /// Starts gathering herbs if the player is idle.
    /// </summary>
    public bool StartGatheringHerbs(int durationMinutes = 5, float efficiencyMultiplier = 1.0f)
    {
        lock (_stateLock)
        {
            if (_currentState.CurrentState != ShopKeeperState.Idle)
            {
                GameLogger.Warning($"Cannot start gathering herbs - current state is {_currentState.CurrentState}");
                return false;
            }

            _currentState.CurrentState = ShopKeeperState.GatheringHerbs;
            _currentState.ActivityStartTime = DateTime.UtcNow;
            _currentState.ActivityDuration = TimeSpan.FromMinutes(durationMinutes);
            _currentState.CurrentProductionRate = BASE_HERB_PRODUCTION_RATE * efficiencyMultiplier;
            _currentState.StatusMessage = $"Gathering herbs for {durationMinutes} minutes...";
            _currentState.CanContinue = true;
            _currentState.TotalProcessedInSession = 0;
        }

        GameLogger.Info($"Started gathering herbs for {durationMinutes} minutes");
        return true;
    }

    /// <summary>
    /// Starts crafting potions if the player is idle and has herbs.
    /// </summary>
    public bool StartCraftingPotions(string recipeId = "basic_healing_potion", float efficiencyMultiplier = 1.0f)
    {
        lock (_stateLock)
        {
            if (_currentState.CurrentState != ShopKeeperState.Idle)
            {
                GameLogger.Warning($"Cannot start crafting potions - current state is {_currentState.CurrentState}");
                return false;
            }

            if (_herbCount <= 0)
            {
                GameLogger.Warning("Cannot start crafting potions - no herbs available");
                return false;
            }

            _currentState.CurrentState = ShopKeeperState.CraftingPotions;
            _currentState.ActivityStartTime = DateTime.UtcNow;
            _currentState.ActivityDuration = null; // Runs until herbs depleted
            _currentState.CurrentProductionRate = BASE_POTION_CRAFTING_RATE * efficiencyMultiplier;
            _currentState.AvailableResources = _herbCount;
            _currentState.StatusMessage = $"Crafting potions from {_herbCount} herbs...";
            _currentState.CanContinue = true;
            _currentState.TotalProcessedInSession = 0;
        }

        GameLogger.Info($"Started crafting potions with {_herbCount} herbs available");
        return true;
    }

    /// <summary>
    /// Starts running the shop if the player is idle and has potions.
    /// </summary>
    public bool StartRunningShop(int durationMinutes = 0, float priceMultiplier = 1.0f)
    {
        lock (_stateLock)
        {
            if (_currentState.CurrentState != ShopKeeperState.Idle)
            {
                GameLogger.Warning($"Cannot start running shop - current state is {_currentState.CurrentState}");
                return false;
            }

            if (_potionCount <= 0)
            {
                GameLogger.Warning("Cannot start running shop - no potions available");
                return false;
            }

            _currentState.CurrentState = ShopKeeperState.RunningShop;
            _currentState.ActivityStartTime = DateTime.UtcNow;
            _currentState.ActivityDuration = durationMinutes > 0 ? TimeSpan.FromMinutes(durationMinutes) : null;
            _currentState.CurrentProductionRate = BASE_POTION_SALE_RATE;
            _currentState.AvailableResources = _potionCount;
            _currentState.StatusMessage = durationMinutes > 0 
                ? $"Running shop for {durationMinutes} minutes with {_potionCount} potions..."
                : $"Running shop with {_potionCount} potions until sold out...";
            _currentState.CanContinue = true;
            _currentState.TotalProcessedInSession = 0;
        }

        GameLogger.Info($"Started running shop with {_potionCount} potions available");
        return true;
    }

    /// <summary>
    /// Stops the current activity and returns to idle state.
    /// </summary>
    public bool StopCurrentActivity(string reason = "Player requested")
    {
        lock (_stateLock)
        {
            if (_currentState.CurrentState == ShopKeeperState.Idle)
            {
                return true; // Already idle
            }

            var previousState = _currentState.CurrentState;
            _currentState.ResetToIdle();
            
            GameLogger.Info($"Stopped {previousState} activity - {reason}");
        }

        return true;
    }

    /// <summary>
    /// Checks if a transition to the specified state is allowed.
    /// </summary>
    public bool CanTransitionToState(ShopKeeperState targetState, bool ignoreResources = false)
    {
        lock (_stateLock)
        {
            // Can only transition if currently idle
            if (_currentState.CurrentState != ShopKeeperState.Idle)
            {
                return false;
            }
        }

        if (ignoreResources)
        {
            return true;
        }

        // Check resource requirements
        return targetState switch
        {
            ShopKeeperState.GatheringHerbs => true, // No resource requirements
            ShopKeeperState.CraftingPotions => _herbCount > 0,
            ShopKeeperState.RunningShop => _potionCount > 0,
            _ => true
        };
    }

    /// <summary>
    /// Periodic update method called by timer to progress activities.
    /// </summary>
    private void UpdateState(object? state)
    {
        try
        {
            UpdateStateInternal();
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error updating ShopKeeper state");
        }
    }

    private void UpdateStateInternal()
    {
        ShopKeeperStateInfo currentState;
        lock (_stateLock)
        {
            currentState = _currentState;
        }

        if (currentState.CurrentState == ShopKeeperState.Idle)
        {
            return; // Nothing to update
        }

        switch (currentState.CurrentState)
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
    }

    private void UpdateGatheringProgress()
    {
        if (!_currentState.ActivityStartTime.HasValue || !_currentState.ActivityDuration.HasValue)
            return;

        var elapsed = DateTime.UtcNow - _currentState.ActivityStartTime.Value;
        var progress = (float)(elapsed.TotalSeconds / _currentState.ActivityDuration.Value.TotalSeconds);

        // Produce herbs based on production rate
        var herbsToGenerate = Math.Max(1, (int)(_currentState.CurrentProductionRate * 1.0f)); // Per second
        _herbCount += herbsToGenerate;
        _currentState.TotalProcessedInSession += herbsToGenerate;
        
        var statusMessage = $"Gathering herbs... {_currentState.TotalProcessedInSession} herbs collected (Total: {_herbCount})";
        _currentState.UpdateProgress(progress, _currentState.TotalProcessedInSession, 0, statusMessage);

        // Check if activity should end
        if (progress >= 1.0f)
        {
            StopCurrentActivity("Gathering duration completed");
        }
    }

    private void UpdateCraftingProgress()
    {
        if (_herbCount <= 0)
        {
            StopCurrentActivity("No more herbs available for crafting");
            return;
        }

        // Simulate potion crafting (consume herbs, produce potions)
        var potionsToMake = Math.Max(1, (int)(_currentState.CurrentProductionRate * 1.0f)); // Per second
        var herbsToConsume = Math.Min(potionsToMake * 2, _herbCount); // 2 herbs per potion
        var actualPotions = herbsToConsume / 2;

        if (actualPotions > 0)
        {
            _herbCount -= herbsToConsume;
            _potionCount += actualPotions;
            _currentState.TotalProcessedInSession += actualPotions;
            
            var progress = _herbCount > 0 ? 1.0f - ((float)_herbCount / _currentState.AvailableResources) : 1.0f;
            var statusMessage = $"Crafting potions... {_currentState.TotalProcessedInSession} potions crafted, {_herbCount} herbs remaining (Total potions: {_potionCount})";
            
            _currentState.UpdateProgress(progress, _currentState.TotalProcessedInSession, _herbCount, statusMessage);

            if (_herbCount <= 0)
            {
                StopCurrentActivity("All herbs used for crafting");
            }
        }
    }

    private void UpdateShopProgress()
    {
        if (_potionCount <= 0)
        {
            StopCurrentActivity("No more potions available for sale");
            return;
        }

        // Simulate potion sales
        var potionsToSell = Math.Max(1, (int)(_currentState.CurrentProductionRate * 1.0f)); // Per second
        potionsToSell = Math.Min(potionsToSell, _potionCount);

        if (potionsToSell > 0)
        {
            _potionCount -= potionsToSell;
            _currentState.TotalProcessedInSession += potionsToSell;
            
            var progress = _potionCount > 0 ? 1.0f - ((float)_potionCount / _currentState.AvailableResources) : 1.0f;
            var statusMessage = $"Running shop... {_currentState.TotalProcessedInSession} potions sold, {_potionCount} potions remaining";
            
            _currentState.UpdateProgress(progress, _currentState.TotalProcessedInSession, _potionCount, statusMessage);

            if (_potionCount <= 0)
            {
                StopCurrentActivity("All potions sold");
            }
        }

        // Check duration limit
        if (_currentState.ActivityDuration.HasValue && _currentState.ActivityStartTime.HasValue)
        {
            var elapsed = DateTime.UtcNow - _currentState.ActivityStartTime.Value;
            if (elapsed >= _currentState.ActivityDuration.Value)
            {
                StopCurrentActivity("Shop duration completed");
            }
        }
    }

    /// <summary>
    /// Disposes of resources used by the state system.
    /// </summary>
    public void Dispose()
    {
        _updateTimer?.Dispose();
        GameLogger.Info("ShopKeeperStateSystem disposed");
    }
}
