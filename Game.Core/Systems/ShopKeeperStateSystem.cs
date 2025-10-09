#nullable enable

using Game.Core.Models;
using Game.Core.Utils;
using Game.Core.CQS;
using Game.Gathering.Commands;
using Game.Crafting.Commands;
using Game.Items.Queries;
using Game.Inventories.Queries;

namespace Game.Core.Systems;

/// <summary>
/// Manages the ShopKeeper's current state and activities.
/// Enforces single-activity constraints and handles state transitions.
/// </summary>
public class ShopKeeperStateSystem
{
    private readonly IDispatcher _dispatcher;
    private readonly System.Threading.Timer _updateTimer;
    private ShopKeeperStateInfo _currentState;
    private readonly object _stateLock = new();
    private readonly Dictionary<ShopKeeperState, DateTime> _activityHistory = new();

    // Production rates (per second)
    private const float BASE_HERB_PRODUCTION_RATE = 0.5f;      // 30 herbs per minute
    private const float BASE_POTION_CRAFTING_RATE = 0.25f;    // 15 potions per minute
    private const float BASE_POTION_SALE_RATE = 0.33f;        // 20 potions per minute

    public ShopKeeperStateSystem(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
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
    /// Starts gathering herbs if the player is idle.
    /// </summary>
    public async Task<bool> StartGatheringHerbsAsync(int durationMinutes = 5, float efficiencyMultiplier = 1.0f)
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

            _activityHistory[ShopKeeperState.GatheringHerbs] = DateTime.UtcNow;
        }

        GameLogger.Info($"Started gathering herbs for {durationMinutes} minutes");
        return true;
    }

    /// <summary>
    /// Starts crafting potions if the player is idle and has herbs.
    /// </summary>
    public async Task<bool> StartCraftingPotionsAsync(string recipeId = "basic_healing_potion", float efficiencyMultiplier = 1.0f, int maxPotions = 0)
    {
        // Check available herbs
        var herbQuery = new GetInventoryItemsQuery { ItemType = "Material" };
        var herbsResult = await _dispatcher.DispatchQueryAsync<GetInventoryItemsQuery, Dictionary<string, int>>(herbQuery);
        var availableHerbs = herbsResult.Where(kvp => kvp.Key.Contains("herb", StringComparison.OrdinalIgnoreCase))
                                       .Sum(kvp => kvp.Value);

        lock (_stateLock)
        {
            if (_currentState.CurrentState != ShopKeeperState.Idle)
            {
                GameLogger.Warning($"Cannot start crafting potions - current state is {_currentState.CurrentState}");
                return false;
            }

            if (availableHerbs <= 0)
            {
                GameLogger.Warning("Cannot start crafting potions - no herbs available");
                return false;
            }

            _currentState.CurrentState = ShopKeeperState.CraftingPotions;
            _currentState.ActivityStartTime = DateTime.UtcNow;
            _currentState.ActivityDuration = null; // Runs until herbs depleted
            _currentState.CurrentProductionRate = BASE_POTION_CRAFTING_RATE * efficiencyMultiplier;
            _currentState.AvailableResources = availableHerbs;
            _currentState.StatusMessage = $"Crafting potions from {availableHerbs} herbs...";
            _currentState.CanContinue = true;
            _currentState.TotalProcessedInSession = 0;

            _activityHistory[ShopKeeperState.CraftingPotions] = DateTime.UtcNow;
        }

        GameLogger.Info($"Started crafting potions with {availableHerbs} herbs available");
        return true;
    }

    /// <summary>
    /// Starts running the shop if the player is idle and has potions.
    /// </summary>
    public async Task<bool> StartRunningShopAsync(int durationMinutes = 0, float priceMultiplier = 1.0f, bool autoCloseWhenEmpty = true)
    {
        // Check available potions
        var potionQuery = new GetInventoryItemsQuery { ItemType = "Consumable" };
        var potionsResult = await _dispatcher.DispatchQueryAsync<GetInventoryItemsQuery, Dictionary<string, int>>(potionQuery);
        var availablePotions = potionsResult.Where(kvp => kvp.Key.Contains("potion", StringComparison.OrdinalIgnoreCase))
                                          .Sum(kvp => kvp.Value);

        lock (_stateLock)
        {
            if (_currentState.CurrentState != ShopKeeperState.Idle)
            {
                GameLogger.Warning($"Cannot start running shop - current state is {_currentState.CurrentState}");
                return false;
            }

            if (availablePotions <= 0)
            {
                GameLogger.Warning("Cannot start running shop - no potions available");
                return false;
            }

            _currentState.CurrentState = ShopKeeperState.RunningShop;
            _currentState.ActivityStartTime = DateTime.UtcNow;
            _currentState.ActivityDuration = durationMinutes > 0 ? TimeSpan.FromMinutes(durationMinutes) : null;
            _currentState.CurrentProductionRate = BASE_POTION_SALE_RATE; // Sales rate
            _currentState.AvailableResources = availablePotions;
            _currentState.StatusMessage = durationMinutes > 0 
                ? $"Running shop for {durationMinutes} minutes with {availablePotions} potions..."
                : $"Running shop with {availablePotions} potions until sold out...";
            _currentState.CanContinue = true;
            _currentState.TotalProcessedInSession = 0;

            _activityHistory[ShopKeeperState.RunningShop] = DateTime.UtcNow;
        }

        GameLogger.Info($"Started running shop with {availablePotions} potions available");
        return true;
    }

    /// <summary>
    /// Stops the current activity and returns to idle state.
    /// </summary>
    public async Task<bool> StopCurrentActivityAsync(string reason = "Player requested")
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
    public async Task<bool> CanTransitionToStateAsync(ShopKeeperState targetState, bool ignoreResources = false)
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
        switch (targetState)
        {
            case ShopKeeperState.GatheringHerbs:
                return true; // No resource requirements

            case ShopKeeperState.CraftingPotions:
                var herbQuery = new GetInventoryItemsQuery { ItemType = "Material" };
                var herbsResult = await _dispatcher.DispatchQueryAsync<GetInventoryItemsQuery, Dictionary<string, int>>(herbQuery);
                return herbsResult.Any(kvp => kvp.Key.Contains("herb", StringComparison.OrdinalIgnoreCase) && kvp.Value > 0);

            case ShopKeeperState.RunningShop:
                var potionQuery = new GetInventoryItemsQuery { ItemType = "Consumable" };
                var potionsResult = await _dispatcher.DispatchQueryAsync<GetInventoryItemsQuery, Dictionary<string, int>>(potionQuery);
                return potionsResult.Any(kvp => kvp.Key.Contains("potion", StringComparison.OrdinalIgnoreCase) && kvp.Value > 0);

            default:
                return true;
        }
    }

    /// <summary>
    /// Periodic update method called by timer to progress activities.
    /// </summary>
    private async void UpdateState(object? state)
    {
        try
        {
            await UpdateStateAsync();
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error updating ShopKeeper state");
        }
    }

    private async Task UpdateStateAsync()
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
                await UpdateGatheringProgressAsync();
                break;

            case ShopKeeperState.CraftingPotions:
                await UpdateCraftingProgressAsync();
                break;

            case ShopKeeperState.RunningShop:
                await UpdateShopProgressAsync();
                break;
        }
    }

    private async Task UpdateGatheringProgressAsync()
    {
        if (!_currentState.ActivityStartTime.HasValue || !_currentState.ActivityDuration.HasValue)
            return;

        var elapsed = DateTime.UtcNow - _currentState.ActivityStartTime.Value;
        var progress = (float)(elapsed.TotalSeconds / _currentState.ActivityDuration.Value.TotalSeconds);

        // Produce herbs based on production rate
        var herbsToGenerate = (int)(_currentState.CurrentProductionRate * 1.0f); // Per second
        if (herbsToGenerate > 0)
        {
            // Generate herbs using gathering system
            var gatherCommand = new GatherMaterialsCommand
            {
                GatheringLocation = "herb_garden",
                Effort = Gathering.Commands.GatheringEffort.Normal
            };

            try
            {
                // For now, simulate herb generation - in full implementation this would use actual gathering
                _currentState.TotalProcessedInSession += herbsToGenerate;
                
                var statusMessage = $"Gathering herbs... {_currentState.TotalProcessedInSession} herbs collected";
                _currentState.UpdateProgress(progress, _currentState.TotalProcessedInSession, 0, statusMessage);

                // Check if activity should end
                if (progress >= 1.0f)
                {
                    await StopCurrentActivityAsync("Gathering duration completed");
                }
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, "Error during herb gathering update");
            }
        }
    }

    private async Task UpdateCraftingProgressAsync()
    {
        // Check available herbs
        var herbQuery = new GetInventoryItemsQuery { ItemType = "Material" };
        var herbsResult = await _dispatcher.DispatchQueryAsync<GetInventoryItemsQuery, Dictionary<string, int>>(herbQuery);
        var availableHerbs = herbsResult.Where(kvp => kvp.Key.Contains("herb", StringComparison.OrdinalIgnoreCase))
                                       .Sum(kvp => kvp.Value);

        if (availableHerbs <= 0)
        {
            await StopCurrentActivityAsync("No more herbs available for crafting");
            return;
        }

        // Simulate potion crafting (consume herbs, produce potions)
        var potionsToMake = (int)(_currentState.CurrentProductionRate * 1.0f); // Per second
        if (potionsToMake > 0 && availableHerbs >= potionsToMake)
        {
            _currentState.TotalProcessedInSession += potionsToMake;
            var remainingHerbs = availableHerbs - potionsToMake;
            
            var progress = remainingHerbs > 0 ? 1.0f - ((float)remainingHerbs / _currentState.AvailableResources) : 1.0f;
            var statusMessage = $"Crafting potions... {_currentState.TotalProcessedInSession} potions crafted, {remainingHerbs} herbs remaining";
            
            _currentState.UpdateProgress(progress, _currentState.TotalProcessedInSession, remainingHerbs, statusMessage);

            if (remainingHerbs <= 0)
            {
                await StopCurrentActivityAsync("All herbs used for crafting");
            }
        }
    }

    private async Task UpdateShopProgressAsync()
    {
        // Check available potions
        var potionQuery = new GetInventoryItemsQuery { ItemType = "Consumable" };
        var potionsResult = await _dispatcher.DispatchQueryAsync<GetInventoryItemsQuery, Dictionary<string, int>>(potionQuery);
        var availablePotions = potionsResult.Where(kvp => kvp.Key.Contains("potion", StringComparison.OrdinalIgnoreCase))
                                          .Sum(kvp => kvp.Value);

        if (availablePotions <= 0)
        {
            await StopCurrentActivityAsync("No more potions available for sale");
            return;
        }

        // Simulate potion sales
        var potionsToSell = (int)(_currentState.CurrentProductionRate * 1.0f); // Per second
        if (potionsToSell > 0 && availablePotions >= potionsToSell)
        {
            _currentState.TotalProcessedInSession += potionsToSell;
            var remainingPotions = availablePotions - potionsToSell;
            
            var progress = remainingPotions > 0 ? 1.0f - ((float)remainingPotions / _currentState.AvailableResources) : 1.0f;
            var statusMessage = $"Running shop... {_currentState.TotalProcessedInSession} potions sold, {remainingPotions} potions remaining";
            
            _currentState.UpdateProgress(progress, _currentState.TotalProcessedInSession, remainingPotions, statusMessage);

            if (remainingPotions <= 0)
            {
                await StopCurrentActivityAsync("All potions sold");
            }
        }

        // Check duration limit
        if (_currentState.ActivityDuration.HasValue && _currentState.ActivityStartTime.HasValue)
        {
            var elapsed = DateTime.UtcNow - _currentState.ActivityStartTime.Value;
            if (elapsed >= _currentState.ActivityDuration.Value)
            {
                await StopCurrentActivityAsync("Shop duration completed");
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
