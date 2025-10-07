using Game.Core.Utils;
using Game.Items.Models.Materials;
using Game.Items.Models;
using Game.Crafting.Models;

namespace Game.Crafting.Systems;

/// <summary>
/// Event arguments for crafting-related events.
/// </summary>
public class CraftingEventArgs : EventArgs
{
    public CraftingOrder Order { get; }

    public CraftingEventArgs(CraftingOrder order)
    {
        Order = order ?? throw new ArgumentNullException(nameof(order));
    }
}

/// <summary>
/// Event arguments for crafting completion events.
/// </summary>
public class CraftingCompletedEventArgs : CraftingEventArgs
{
    public Item? CraftedItem { get; }
    public bool WasSuccessful { get; }

    public CraftingCompletedEventArgs(CraftingOrder order, Item? craftedItem, bool wasSuccessful)
        : base(order)
    {
        CraftedItem = craftedItem;
        WasSuccessful = wasSuccessful;
    }
}

/// <summary>
/// Manages the crafting queue and processes crafting orders over time.
/// Handles the timing and progression of crafting operations.
/// </summary>
public class CraftingStation : ICraftingStation
{
    private readonly RecipeManager _recipeManager;
    private readonly Random _random;
    private readonly Queue<CraftingOrder> _craftingQueue;
    private CraftingOrder? _currentOrder;
    private readonly Timer _craftingTimer;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Event raised when a crafting order starts.
    /// </summary>
    public event EventHandler<CraftingEventArgs>? CraftingStarted;

    /// <summary>
    /// Event raised when crafting progress updates.
    /// </summary>
    public event EventHandler<CraftingEventArgs>? CraftingProgressUpdated;

    /// <summary>
    /// Event raised when a crafting order completes (success or failure).
    /// </summary>
    public event EventHandler<CraftingCompletedEventArgs>? CraftingCompleted;

    /// <summary>
    /// Event raised when a crafting order is cancelled.
    /// </summary>
    public event EventHandler<CraftingEventArgs>? CraftingCancelled;

    /// <summary>
    /// Gets the current crafting order, if any.
    /// </summary>
    public CraftingOrder? CurrentOrder
    {
        get
        {
            lock (_lockObject)
            {
                return _currentOrder;
            }
        }
    }

    /// <summary>
    /// Gets the list of queued crafting orders.
    /// </summary>
    public IReadOnlyList<CraftingOrder> QueuedOrders
    {
        get
        {
            lock (_lockObject)
            {
                return _craftingQueue.ToList();
            }
        }
    }

    /// <summary>
    /// Gets whether the crafting station is currently active.
    /// </summary>
    public bool IsActive => CurrentOrder != null;

    /// <summary>
    /// Gets the total number of orders (current + queued).
    /// </summary>
    public int TotalOrderCount
    {
        get
        {
            lock (_lockObject)
            {
                return (_currentOrder != null ? 1 : 0) + _craftingQueue.Count;
            }
        }
    }

    public CraftingStation(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
        _random = new Random();
        _craftingQueue = new Queue<CraftingOrder>();
        
        // Timer for updating crafting progress (100ms intervals)
        _craftingTimer = new Timer(UpdateCraftingProgress, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Attempts to queue a crafting order.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to craft</param>
    /// <param name="materials">Materials to use for crafting</param>
    /// <returns>The created crafting order, or null if validation failed</returns>
    public CraftingOrder? QueueCraftingOrder(string recipeId, IReadOnlyDictionary<string, Material> materials)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            GameLogger.Warning("Cannot queue crafting order: Recipe ID is null or empty");
            return null;
        }

        var recipe = _recipeManager.GetRecipe(recipeId);
        if (recipe == null)
        {
            GameLogger.Warning($"Cannot queue crafting order: Recipe '{recipeId}' not found");
            return null;
        }

        if (!_recipeManager.IsRecipeUnlocked(recipeId))
        {
            GameLogger.Warning($"Cannot queue crafting order: Recipe '{recipe.Name}' is not unlocked");
            return null;
        }

        if (materials == null)
        {
            GameLogger.Warning("Cannot queue crafting order: Materials list is null");
            return null;
        }

        // Validate materials
        var validationResult = ValidateMaterials(recipe, materials);
        if (!validationResult.IsValid)
        {
            GameLogger.Warning($"Cannot queue crafting order: {validationResult.ErrorMessage}");
            return null;
        }

        // Create the order
        var orderId = Guid.NewGuid().ToString("N")[..8]; // Short UUID
        var order = new CraftingOrder(orderId, recipe, materials);

        lock (_lockObject)
        {
            _craftingQueue.Enqueue(order);
        }

        GameLogger.Info($"Queued crafting order: {recipe.Name} (Order: {orderId})");

        // Start processing if not already active
        TryStartNextOrder();

        return order;
    }

    /// <summary>
    /// Cancels a specific crafting order.
    /// </summary>
    /// <param name="orderId">The ID of the order to cancel</param>
    /// <returns>True if the order was cancelled, false if not found</returns>
    public bool CancelOrder(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return false;
        }

        lock (_lockObject)
        {
            // Check if it's the current order
            if (_currentOrder?.OrderId == orderId)
            {
                _currentOrder.Cancel();
                _craftingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                
                var cancelledOrder = _currentOrder;
                _currentOrder = null;
                
                GameLogger.Info($"Cancelled current crafting order: {cancelledOrder.Recipe.Name}");
                CraftingCancelled?.Invoke(this, new CraftingEventArgs(cancelledOrder));
                
                // Start next order
                TryStartNextOrder();
                return true;
            }

            // Check if it's in the queue
            var tempQueue = new Queue<CraftingOrder>();
            bool found = false;
            CraftingOrder? queuedCancelledOrder = null;

            while (_craftingQueue.Count > 0)
            {
                var order = _craftingQueue.Dequeue();
                if (order.OrderId == orderId)
                {
                    order.Cancel();
                    queuedCancelledOrder = order;
                    found = true;
                    GameLogger.Info($"Cancelled queued crafting order: {order.Recipe.Name}");
                }
                else
                {
                    tempQueue.Enqueue(order);
                }
            }

            // Restore queue
            while (tempQueue.Count > 0)
            {
                _craftingQueue.Enqueue(tempQueue.Dequeue());
            }

            if (found && queuedCancelledOrder != null)
            {
                CraftingCancelled?.Invoke(this, new CraftingEventArgs(queuedCancelledOrder));
            }

            return found;
        }
    }

    /// <summary>
    /// Cancels all crafting orders.
    /// </summary>
    public void CancelAllOrders()
    {
        lock (_lockObject)
        {
            // Cancel current order
            if (_currentOrder != null)
            {
                _currentOrder.Cancel();
                _craftingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                
                var cancelledOrder = _currentOrder;
                _currentOrder = null;
                CraftingCancelled?.Invoke(this, new CraftingEventArgs(cancelledOrder));
            }

            // Cancel all queued orders
            while (_craftingQueue.Count > 0)
            {
                var order = _craftingQueue.Dequeue();
                order.Cancel();
                CraftingCancelled?.Invoke(this, new CraftingEventArgs(order));
            }
        }

        GameLogger.Info("Cancelled all crafting orders");
    }

    /// <summary>
    /// Gets an order by its ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve</param>
    /// <returns>The order, or null if not found</returns>
    public CraftingOrder? GetOrder(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return null;
        }

        lock (_lockObject)
        {
            if (_currentOrder?.OrderId == orderId)
            {
                return _currentOrder;
            }

            return _craftingQueue.FirstOrDefault(order => order.OrderId == orderId);
        }
    }

    /// <summary>
    /// Gets statistics about the crafting station.
    /// </summary>
    /// <returns>Dictionary with various statistics</returns>
    public Dictionary<string, object> GetStatistics()
    {
        lock (_lockObject)
        {
            return new Dictionary<string, object>
            {
                ["IsActive"] = IsActive,
                ["CurrentOrder"] = _currentOrder?.OrderId ?? "None",
                ["QueuedOrders"] = _craftingQueue.Count,
                ["TotalOrders"] = TotalOrderCount,
                ["CurrentProgress"] = _currentOrder?.Progress ?? 0.0
            };
        }
    }

    /// <summary>
    /// Tries to start the next order in the queue.
    /// </summary>
    private void TryStartNextOrder()
    {
        lock (_lockObject)
        {
            // Don't start if already processing
            if (_currentOrder != null)
            {
                return;
            }

            // Check if there are queued orders
            if (_craftingQueue.Count == 0)
            {
                return;
            }

            _currentOrder = _craftingQueue.Dequeue();
            _currentOrder.Start();
            
            GameLogger.Info($"Started crafting: {_currentOrder.Recipe.Name} (Order: {_currentOrder.OrderId})");
            CraftingStarted?.Invoke(this, new CraftingEventArgs(_currentOrder));

            // Start the crafting timer (100ms intervals)
            _craftingTimer.Change(100, 100);
        }
    }

    /// <summary>
    /// Updates the progress of the current crafting order.
    /// </summary>
    /// <param name="state">Timer state (unused)</param>
    private void UpdateCraftingProgress(object? state)
    {
        CraftingOrder? orderToComplete = null;

        lock (_lockObject)
        {
            if (_currentOrder == null)
            {
                _craftingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            // Calculate progress based on elapsed time
            var elapsed = DateTime.UtcNow - _currentOrder.StartedAt!.Value;
            var totalTime = TimeSpan.FromSeconds(_currentOrder.Recipe.CraftingTime);
            var progress = Math.Min(1.0, elapsed.TotalSeconds / totalTime.TotalSeconds);

            _currentOrder.UpdateProgress(progress);
            CraftingProgressUpdated?.Invoke(this, new CraftingEventArgs(_currentOrder));

            // Check if completed
            if (progress >= 1.0)
            {
                orderToComplete = _currentOrder;
                _currentOrder = null;
                _craftingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        // Complete the order outside the lock to avoid deadlock
        if (orderToComplete != null)
        {
            CompleteCraftingOrder(orderToComplete);
            TryStartNextOrder();
        }
    }

    /// <summary>
    /// Completes a crafting order and creates the resulting item.
    /// </summary>
    /// <param name="order">The order to complete</param>
    private void CompleteCraftingOrder(CraftingOrder order)
    {
        try
        {
            // Calculate success rate and determine if crafting succeeds
            var successRate = order.CalculateSuccessRate();
            var roll = _random.NextDouble() * 100.0;
            var success = roll <= successRate;

            if (success)
            {
                // Calculate final quality based on materials
                var finalQuality = CalculateFinalQuality(order);
                order.Complete(finalQuality);

                // Create the actual item
                var craftedItem = CreateCraftedItem(order, finalQuality);

                GameLogger.Info($"Successfully crafted: {craftedItem.Name} ({finalQuality}) - Order: {order.OrderId}");
                CraftingCompleted?.Invoke(this, new CraftingCompletedEventArgs(order, craftedItem, true));
            }
            else
            {
                order.Fail($"Crafting failed (rolled {roll:F1}, needed {successRate:F1} or lower)");
                GameLogger.Info($"Crafting failed: {order.Recipe.Name} - Order: {order.OrderId}");
                CraftingCompleted?.Invoke(this, new CraftingCompletedEventArgs(order, null, false));
            }
        }
        catch (Exception ex)
        {
            order.Fail($"Unexpected error: {ex.Message}");
            GameLogger.Error(ex, $"Error completing crafting order: {order.OrderId}");
            CraftingCompleted?.Invoke(this, new CraftingCompletedEventArgs(order, null, false));
        }
    }

    /// <summary>
    /// Calculates the final quality of the crafted item based on materials used.
    /// </summary>
    /// <param name="order">The crafting order</param>
    /// <returns>The final quality tier</returns>
    private QualityTier CalculateFinalQuality(CraftingOrder order)
    {
        var baseQuality = order.Recipe.Result.BaseQuality;
        var materialQualities = order.AllocatedMaterials.Values.Select(m => m.Quality).ToList();
        
        if (materialQualities.Count == 0)
        {
            return baseQuality;
        }

        // Average material quality
        var averageQuality = materialQualities.Average(q => (int)q);
        var baseQualityValue = (int)baseQuality;

        // Calculate quality modifier based on material quality vs recipe base quality
        var qualityModifier = (averageQuality - baseQualityValue) * 0.3; // 30% influence
        var finalQualityValue = Math.Clamp(baseQualityValue + qualityModifier, 0, 4); // 0-4 for QualityTier enum

        return (QualityTier)Math.Round(finalQualityValue);
    }

    /// <summary>
    /// Creates the actual crafted item based on the completed order.
    /// </summary>
    /// <param name="order">The completed crafting order</param>
    /// <param name="finalQuality">The final quality of the item</param>
    /// <returns>The crafted item</returns>
    private Item CreateCraftedItem(CraftingOrder order, QualityTier finalQuality)
    {
        var result = order.Recipe.Result;
        var finalValue = CalculateFinalValue(result.BaseValue, finalQuality);

        return result.ItemType switch
        {
            ItemType.Weapon => new Weapon(
                result.ItemId,
                result.ItemName,
                $"A {finalQuality.ToString().ToLower()} quality {result.ItemName.ToLower()} crafted with care.",
                finalQuality,
                finalValue,
                result.GetProperty("DamageBonus", 0)),

            ItemType.Armor => new Armor(
                result.ItemId,
                result.ItemName,
                $"A {finalQuality.ToString().ToLower()} quality {result.ItemName.ToLower()} crafted with skill.",
                finalQuality,
                finalValue,
                result.GetProperty("DamageReduction", 0)),

            _ => new Item(
                result.ItemId,
                result.ItemName,
                $"A {finalQuality.ToString().ToLower()} quality {result.ItemName.ToLower()}.",
                result.ItemType,
                finalQuality,
                finalValue)
        };
    }

    /// <summary>
    /// Calculates the final value of an item based on its quality.
    /// </summary>
    /// <param name="baseValue">The base value before quality modifiers</param>
    /// <param name="quality">The final quality of the item</param>
    /// <returns>The final value</returns>
    private int CalculateFinalValue(int baseValue, QualityTier quality)
    {
        var multiplier = quality switch
        {
            QualityTier.Common => 1.0,
            QualityTier.Uncommon => 1.5,
            QualityTier.Rare => 2.0,
            QualityTier.Epic => 3.0,
            QualityTier.Legendary => 5.0,
            _ => 1.0
        };

        return (int)(baseValue * multiplier);
    }

    /// <summary>
    /// Validates that the provided materials satisfy the recipe requirements.
    /// </summary>
    /// <param name="recipe">The recipe to validate against</param>
    /// <param name="materials">The materials to validate</param>
    /// <returns>Validation result</returns>
    private MaterialValidationResult ValidateMaterials(Recipe recipe, IReadOnlyDictionary<string, Material> materials)
    {
        var materialsList = materials.Values.ToList();

        foreach (var requirement in recipe.MaterialRequirements)
        {
            var satisfyingMaterials = materialsList
                .Where(requirement.IsSatisfiedBy)
                .Take(requirement.Quantity)
                .ToList();

            if (satisfyingMaterials.Count < requirement.Quantity)
            {
                return new MaterialValidationResult(false, 
                    $"Insufficient {requirement.MaterialCategory} materials. Required: {requirement.Quantity}, Available: {satisfyingMaterials.Count}");
            }
        }

        return new MaterialValidationResult(true, string.Empty);
    }

    /// <summary>
    /// Disposes of the crafting station and stops all operations.
    /// </summary>
    public void Dispose()
    {
        _craftingTimer?.Dispose();
        CancelAllOrders();
    }

    /// <summary>
    /// Result of material validation.
    /// </summary>
    private readonly struct MaterialValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public MaterialValidationResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}
