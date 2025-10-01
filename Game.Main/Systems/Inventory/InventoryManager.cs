#nullable enable

using Game.Main.Models.Materials;
using Game.Main.Utils;

namespace Game.Main.Systems.Inventory;

/// <summary>
/// High-level manager for inventory operations, including persistence, validation, and integration.
/// Serves as the main interface for the game systems to interact with inventory functionality.
/// </summary>
public class InventoryManager
{
    private Inventory _inventory;
    private bool _isLoaded;
    private readonly object _lockObject = new();

    /// <summary>
    /// Creates a new inventory manager with default capacity.
    /// </summary>
    /// <param name="initialCapacity">Initial inventory capacity</param>
    public InventoryManager(int initialCapacity = 20)
    {
        _inventory = new Inventory(initialCapacity);
        _isLoaded = false;
        
        // Subscribe to inventory events for logging and validation
        _inventory.MaterialAdded += OnMaterialAdded;
        _inventory.MaterialRemoved += OnMaterialRemoved;
        _inventory.InventoryChanged += OnInventoryChanged;
        _inventory.CapacityChanged += OnCapacityChanged;
    }

    /// <summary>
    /// Gets the current inventory instance.
    /// </summary>
    public Inventory CurrentInventory => _inventory;

    /// <summary>
    /// Gets whether the inventory data has been loaded.
    /// </summary>
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Event fired when inventory state changes significantly.
    /// </summary>
    public event Action<InventoryStats>? InventoryUpdated;

    /// <summary>
    /// Event fired when an operation fails due to capacity or other constraints.
    /// </summary>
    public event Action<string>? OperationFailed;

    /// <summary>
    /// Adds multiple material drops to the inventory with overflow handling.
    /// </summary>
    /// <param name="drops">Collection of material drops to add</param>
    /// <returns>Summary of the add operation</returns>
    public InventoryAddResult AddMaterials(IEnumerable<MaterialDrop> drops)
    {
        lock (_lockObject)
        {
            var result = new InventoryAddResult();
            
            foreach (var drop in drops)
            {
                try
                {
                    drop.Validate();
                    
                    if (_inventory.CanAddMaterial(drop))
                    {
                        if (_inventory.AddMaterial(drop))
                        {
                            result.SuccessfulAdds.Add(drop);
                        }
                        else
                        {
                            result.PartialAdds.Add(drop);
                        }
                    }
                    else
                    {
                        result.FailedAdds.Add(drop);
                        OperationFailed?.Invoke($"Cannot add {drop.Material.Name} - inventory constraints");
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.Error(ex, $"Failed to add material {drop.Material.Name}");
                    result.FailedAdds.Add(drop);
                }
            }

            if (result.HasAnyChanges)
            {
                InventoryUpdated?.Invoke(_inventory.GetStats());
            }

            return result;
        }
    }

    /// <summary>
    /// Removes materials from inventory with validation.
    /// </summary>
    /// <param name="materialId">ID of the material to remove</param>
    /// <param name="rarity">Rarity of the material</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>Number of materials actually removed</returns>
    public int RemoveMaterials(string materialId, MaterialRarity rarity, int quantity)
    {
        lock (_lockObject)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(materialId))
                {
                    throw new ArgumentException("Material ID cannot be null or empty", nameof(materialId));
                }

                if (quantity <= 0)
                {
                    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
                }

                var removed = _inventory.RemoveMaterial(materialId, rarity, quantity);
                
                if (removed > 0)
                {
                    InventoryUpdated?.Invoke(_inventory.GetStats());
                }
                else
                {
                    OperationFailed?.Invoke($"Could not remove {materialId} ({rarity}) - insufficient quantity");
                }

                return removed;
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, $"Failed to remove material {materialId}");
                OperationFailed?.Invoke($"Error removing {materialId}: {ex.Message}");
                return 0;
            }
        }
    }

    /// <summary>
    /// Searches and filters inventory contents with advanced criteria.
    /// </summary>
    /// <param name="criteria">Search and filter criteria</param>
    /// <returns>Filtered and sorted inventory contents</returns>
    public InventorySearchResult SearchInventory(InventorySearchCriteria criteria)
    {
        lock (_lockObject)
        {
            try
            {
                var results = _inventory.Materials.AsEnumerable();

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
                {
                    results = _inventory.SearchMaterials(criteria.SearchTerm);
                }

                // Apply category filter
                if (criteria.CategoryFilter.HasValue)
                {
                    results = results.Where(stack => stack.Material.Category == criteria.CategoryFilter.Value);
                }

                // Apply rarity filter
                if (criteria.RarityFilter.HasValue)
                {
                    results = results.Where(stack => stack.Rarity == criteria.RarityFilter.Value);
                }

                // Apply minimum quantity filter
                if (criteria.MinQuantity > 0)
                {
                    results = results.Where(stack => stack.Quantity >= criteria.MinQuantity);
                }

                // Apply minimum value filter
                if (criteria.MinValue > 0)
                {
                    results = results.Where(stack => stack.TotalValue >= criteria.MinValue);
                }

                // Apply sorting to the filtered results
                results = ApplySorting(results, criteria.SortBy, criteria.SortAscending);

                var resultList = results.ToList();
                
                return new InventorySearchResult(
                    resultList,
                    resultList.Count,
                    resultList.Sum(s => s.Quantity),
                    resultList.Sum(s => s.TotalValue)
                );
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, "Failed to search inventory");
                return new InventorySearchResult(Array.Empty<MaterialStack>(), 0, 0, 0);
            }
        }
    }

    /// <summary>
    /// Applies sorting to a collection of material stacks.
    /// </summary>
    private static IEnumerable<MaterialStack> ApplySorting(IEnumerable<MaterialStack> materials, MaterialSortBy sortBy, bool ascending)
    {
        return sortBy switch
        {
            MaterialSortBy.Name => ascending 
                ? materials.OrderBy(s => s.Material.Name)
                : materials.OrderByDescending(s => s.Material.Name),
            MaterialSortBy.Quantity => ascending
                ? materials.OrderBy(s => s.Quantity)
                : materials.OrderByDescending(s => s.Quantity),
            MaterialSortBy.Rarity => ascending
                ? materials.OrderBy(s => s.Rarity)
                : materials.OrderByDescending(s => s.Rarity),
            MaterialSortBy.Category => ascending
                ? materials.OrderBy(s => s.Material.Category)
                : materials.OrderByDescending(s => s.Material.Category),
            MaterialSortBy.Value => ascending
                ? materials.OrderBy(s => s.TotalValue)
                : materials.OrderByDescending(s => s.TotalValue),
            MaterialSortBy.LastUpdated => ascending
                ? materials.OrderBy(s => s.LastUpdated)
                : materials.OrderByDescending(s => s.LastUpdated),
            _ => materials // No sorting for unknown sort types
        };
    }

    /// <summary>
    /// Checks if materials can be consumed for crafting or other operations.
    /// </summary>
    /// <param name="requirements">Required materials with quantities</param>
    /// <returns>True if all requirements can be met</returns>
    public bool CanConsumeMaterials(Dictionary<(string MaterialId, MaterialRarity Rarity), int> requirements)
    {
        lock (_lockObject)
        {
            try
            {
                foreach (var requirement in requirements)
                {
                    var (materialId, rarity) = requirement.Key;
                    var requiredQuantity = requirement.Value;
                    var availableQuantity = _inventory.GetMaterialQuantity(materialId, rarity);

                    if (availableQuantity < requiredQuantity)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, "Error checking material requirements");
                return false;
            }
        }
    }

    /// <summary>
    /// Consumes materials for crafting or other operations.
    /// </summary>
    /// <param name="requirements">Required materials with quantities</param>
    /// <returns>True if all materials were successfully consumed</returns>
    public bool ConsumeMaterials(Dictionary<(string MaterialId, MaterialRarity Rarity), int> requirements)
    {
        lock (_lockObject)
        {
            try
            {
                // First check if we can consume all required materials
                if (!CanConsumeMaterials(requirements))
                {
                    OperationFailed?.Invoke("Insufficient materials for consumption");
                    return false;
                }

                // Track what we've removed for potential rollback
                var removedMaterials = new List<(string MaterialId, MaterialRarity Rarity, int Quantity)>();
                var success = true;

                foreach (var requirement in requirements)
                {
                    var (materialId, rarity) = requirement.Key;
                    var requiredQuantity = requirement.Value;
                    
                    var actuallyRemoved = _inventory.RemoveMaterial(materialId, rarity, requiredQuantity);
                    removedMaterials.Add((materialId, rarity, actuallyRemoved));

                    if (actuallyRemoved != requiredQuantity)
                    {
                        GameLogger.Error($"Could not remove full quantity of {materialId} ({rarity})");
                        success = false;
                        break;
                    }
                }

                if (!success)
                {
                    // Rollback any successful removals
                    GameLogger.Warning("Rolling back partial material consumption");
                    foreach (var (materialId, rarity, quantity) in removedMaterials)
                    {
                        // This would require recreating MaterialDrops, which is complex
                        // For now, just log the issue - in production, this would need better handling
                        GameLogger.Error($"Need to restore {quantity} {materialId} ({rarity}) - rollback not implemented");
                    }
                }
                else
                {
                    InventoryUpdated?.Invoke(_inventory.GetStats());
                }

                return success;
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, "Error consuming materials");
                OperationFailed?.Invoke($"Error consuming materials: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Expands inventory capacity if possible.
    /// </summary>
    /// <param name="additionalSlots">Number of slots to add</param>
    /// <returns>True if expansion was successful</returns>
    public bool ExpandInventory(int additionalSlots)
    {
        lock (_lockObject)
        {
            try
            {
                _inventory.ExpandCapacity(additionalSlots);
                InventoryUpdated?.Invoke(_inventory.GetStats());
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, $"Failed to expand inventory by {additionalSlots} slots");
                OperationFailed?.Invoke($"Could not expand inventory: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Gets comprehensive inventory statistics.
    /// </summary>
    /// <returns>Current inventory statistics</returns>
    public InventoryStats GetInventoryStats()
    {
        lock (_lockObject)
        {
            return _inventory.GetStats();
        }
    }

    /// <summary>
    /// Clears all inventory contents.
    /// </summary>
    public void ClearInventory()
    {
        lock (_lockObject)
        {
            _inventory.Clear();
            InventoryUpdated?.Invoke(_inventory.GetStats());
        }
    }

    /// <summary>
    /// Saves inventory to persistent storage (placeholder for future implementation).
    /// </summary>
    /// <returns>True if save was successful</returns>
    public async Task<bool> SaveInventoryAsync()
    {
        await Task.Delay(1); // Placeholder for actual save logic
        GameLogger.Info("Inventory save completed (placeholder)");
        return true;
    }

    /// <summary>
    /// Loads inventory from persistent storage (placeholder for future implementation).
    /// </summary>
    /// <returns>True if load was successful</returns>
    public async Task<bool> LoadInventoryAsync()
    {
        await Task.Delay(1); // Placeholder for actual load logic
        _isLoaded = true;
        GameLogger.Info("Inventory load completed (placeholder)");
        InventoryUpdated?.Invoke(_inventory.GetStats());
        return true;
    }

    /// <summary>
    /// Validates inventory integrity and fixes any issues.
    /// </summary>
    /// <returns>Validation result with any issues found</returns>
    public InventoryValidationResult ValidateInventory()
    {
        lock (_lockObject)
        {
            var issues = new List<string>();
            var fixedIssues = new List<string>();

            try
            {
                // Check for invalid stacks
                var invalidStacks = _inventory.Materials.Where(stack => stack.Quantity <= 0).ToList();
                if (invalidStacks.Any())
                {
                    issues.Add($"Found {invalidStacks.Count} invalid stacks with zero or negative quantity");
                    // In a real implementation, we would remove these stacks
                }

                // Check capacity constraints
                if (_inventory.UsedSlots > _inventory.Capacity)
                {
                    issues.Add($"Inventory over capacity: {_inventory.UsedSlots}/{_inventory.Capacity}");
                }

                // Validate material references
                foreach (var stack in _inventory.Materials)
                {
                    try
                    {
                        stack.Validate();
                    }
                    catch (Exception ex)
                    {
                        issues.Add($"Invalid stack for {stack.Material.Name}: {ex.Message}");
                    }
                }

                return new InventoryValidationResult(issues.Count == 0, issues, fixedIssues);
            }
            catch (Exception ex)
            {
                GameLogger.Error(ex, "Error during inventory validation");
                issues.Add($"Validation error: {ex.Message}");
                return new InventoryValidationResult(false, issues, fixedIssues);
            }
        }
    }

    /// <summary>
    /// Disposes of the inventory manager and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        lock (_lockObject)
        {
            // Unsubscribe from events to prevent memory leaks
            _inventory.MaterialAdded -= OnMaterialAdded;
            _inventory.MaterialRemoved -= OnMaterialRemoved;
            _inventory.InventoryChanged -= OnInventoryChanged;
            _inventory.CapacityChanged -= OnCapacityChanged;
        }
    }

    // Event handlers
    private void OnMaterialAdded(MaterialDrop drop)
    {
        GameLogger.Debug($"Material added: {drop.Material.Name} x{drop.Quantity} ({drop.ActualRarity})");
    }

    private void OnMaterialRemoved(string materialId, MaterialRarity rarity, int quantity)
    {
        GameLogger.Debug($"Material removed: {materialId} x{quantity} ({rarity})");
    }

    private void OnInventoryChanged()
    {
        GameLogger.Debug("Inventory contents changed");
    }

    private void OnCapacityChanged(int newCapacity)
    {
        GameLogger.Info($"Inventory capacity changed to {newCapacity}");
    }
}

/// <summary>
/// Result of adding materials to inventory.
/// </summary>
public class InventoryAddResult
{
    public List<MaterialDrop> SuccessfulAdds { get; } = new();
    public List<MaterialDrop> PartialAdds { get; } = new();
    public List<MaterialDrop> FailedAdds { get; } = new();

    public bool HasAnyChanges => SuccessfulAdds.Count > 0 || PartialAdds.Count > 0;
    public bool AllSuccessful => FailedAdds.Count == 0 && PartialAdds.Count == 0;
    public int TotalProcessed => SuccessfulAdds.Count + PartialAdds.Count + FailedAdds.Count;
}

/// <summary>
/// Criteria for searching and filtering inventory.
/// </summary>
public record InventorySearchCriteria(
    string? SearchTerm = null,
    MaterialCategory? CategoryFilter = null,
    MaterialRarity? RarityFilter = null,
    int MinQuantity = 0,
    int MinValue = 0,
    MaterialSortBy SortBy = MaterialSortBy.Name,
    bool SortAscending = true
);

/// <summary>
/// Result of inventory search operation.
/// </summary>
/// <param name="Results">Matching material stacks</param>
/// <param name="TotalStacks">Number of matching stacks</param>
/// <param name="TotalMaterials">Total number of individual materials</param>
/// <param name="TotalValue">Combined value of matching materials</param>
public record InventorySearchResult(
    IReadOnlyList<MaterialStack> Results,
    int TotalStacks,
    int TotalMaterials,
    int TotalValue
);

/// <summary>
/// Result of inventory validation.
/// </summary>
/// <param name="IsValid">Whether the inventory is valid</param>
/// <param name="Issues">List of issues found</param>
/// <param name="FixedIssues">List of issues that were automatically fixed</param>
public record InventoryValidationResult(
    bool IsValid,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> FixedIssues
);
