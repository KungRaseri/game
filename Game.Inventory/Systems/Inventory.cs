#nullable enable

using Game.Core.Utils;
using Game.Item.Models;
using Game.Item.Models.Materials;

namespace Game.Inventory.Systems;

/// <summary>
/// Manages material storage with capacity limits, automatic stacking, and organization.
/// Provides events for UI updates and supports search, filtering, and sorting operations.
/// </summary>
public class Inventory
{
    private readonly Dictionary<string, MaterialStack> _materials;
    private int _capacity;

    /// <summary>
    /// Creates a new inventory with the specified capacity.
    /// </summary>
    /// <param name="capacity">Maximum number of different material stacks that can be stored</param>
    public Inventory(int capacity = 20)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));
        }

        _capacity = capacity;
        _materials = new Dictionary<string, MaterialStack>();
    }

    /// <summary>
    /// Gets the current capacity of the inventory.
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// Gets the number of occupied slots in the inventory.
    /// </summary>
    public int UsedSlots => _materials.Count;

    /// <summary>
    /// Gets the number of available slots in the inventory.
    /// </summary>
    public int FreeSlots => _capacity - _materials.Count;

    /// <summary>
    /// Gets whether the inventory is at maximum capacity.
    /// </summary>
    public bool IsFull => _materials.Count >= _capacity;

    /// <summary>
    /// Gets whether the inventory is empty.
    /// </summary>
    public bool IsEmpty => _materials.Count == 0;

    /// <summary>
    /// Gets all material stacks currently in the inventory.
    /// </summary>
    public IReadOnlyCollection<MaterialStack> Materials => _materials.Values;

    /// <summary>
    /// Event fired when a material is added to the inventory.
    /// </summary>
    public event Action<Drop>? MaterialAdded;

    /// <summary>
    /// Event fired when a material is removed from the inventory.
    /// </summary>
    public event Action<string, QualityTier, int>? MaterialRemoved;

    /// <summary>
    /// Event fired when the inventory contents change.
    /// </summary>
    public event Action? InventoryChanged;

    /// <summary>
    /// Event fired when the inventory capacity changes.
    /// </summary>
    public event Action<int>? CapacityChanged;

    /// <summary>
    /// Checks if a material drop can be added to the inventory.
    /// </summary>
    /// <param name="drop">The material drop to check</param>
    /// <returns>True if the drop can be fully accommodated</returns>
    public bool CanAddMaterial(Drop drop)
    {
        drop.Validate();

        var stackKey = GetStackKey(drop.Material.ItemId, drop.Material.Quality);

        // If we already have this material, check if it can fit in the existing stack
        if (_materials.TryGetValue(stackKey, out var existingStack))
        {
            return existingStack.RemainingSpace >= drop.Quantity;
        }

        // If we don't have this material, check if we have room for a new stack
        return !IsFull;
    }

    /// <summary>
    /// Adds a material drop to the inventory.
    /// </summary>
    /// <param name="drop">The material drop to add</param>
    /// <returns>True if the drop was fully added, false if only partially added or not added</returns>
    public bool AddMaterial(Drop drop)
    {
        drop.Validate();

        var stackKey = GetStackKey(drop.Material.ItemId, drop.Material.Quality);
        var quantityToAdd = drop.Quantity;
        var fullyAdded = true;

        // Try to add to existing stack first
        if (_materials.TryGetValue(stackKey, out var existingStack))
        {
            var (newStack, overflow) = existingStack.TryAdd(quantityToAdd);
            _materials[stackKey] = newStack;

            if (overflow > 0)
            {
                GameLogger.Warning($"Could not add {overflow} {drop.Material.Name} - stack full");
                fullyAdded = false;
            }
        }
        else
        {
            // Create new stack if we have space
            if (IsFull)
            {
                GameLogger.Warning($"Cannot add {drop.Material.Name} - inventory full");
                return false;
            }

            var newStack = MaterialStack.FromDrop(drop);
            _materials[stackKey] = newStack;
        }

        // Fire events
        MaterialAdded?.Invoke(drop);
        InventoryChanged?.Invoke();

        GameLogger.Info($"Added {drop.Quantity} {drop.Material.Name} ({drop.Material.Quality}) to inventory");
        return fullyAdded;
    }

    /// <summary>
    /// Removes materials from the inventory.
    /// </summary>
    /// <param name="materialId">ID of the material to remove</param>
    /// <param name="quality">Rarity of the material to remove</param>
    /// <param name="quantity">Number of materials to remove</param>
    /// <returns>The actual number of materials removed</returns>
    public int RemoveMaterial(string materialId, QualityTier quality, int quantity)
    {
        if (quantity <= 0)
        {
            return 0;
        }

        var stackKey = GetStackKey(materialId, quality);

        if (!_materials.TryGetValue(stackKey, out var existingStack))
        {
            GameLogger.Warning($"Cannot remove {materialId} ({quality}) - not found in inventory");
            return 0;
        }

        var (newStack, actuallyRemoved) = existingStack.TryRemove(quantity);

        if (newStack == null)
        {
            // Stack is now empty, remove it
            _materials.Remove(stackKey);
        }
        else
        {
            // Update the stack
            _materials[stackKey] = newStack;
        }

        // Fire events
        MaterialRemoved?.Invoke(materialId, quality, actuallyRemoved);
        InventoryChanged?.Invoke();

        GameLogger.Info($"Removed {actuallyRemoved} {existingStack.Material.Name} ({quality}) from inventory");
        return actuallyRemoved;
    }

    /// <summary>
    /// Gets the quantity of a specific material and rarity in the inventory.
    /// </summary>
    /// <param name="materialId">ID of the material</param>
    /// <param name="quality">Rarity of the material</param>
    /// <returns>The quantity available, or 0 if not found</returns>
    public int GetMaterialQuantity(string materialId, QualityTier quality)
    {
        var stackKey = GetStackKey(materialId, quality);
        return _materials.TryGetValue(stackKey, out var stack) ? stack.Quantity : 0;
    }

    /// <summary>
    /// Gets all stacks of a specific material across all rarities.
    /// </summary>
    /// <param name="materialId">ID of the material</param>
    /// <returns>Collection of stacks for this material</returns>
    public IEnumerable<MaterialStack> GetMaterialStacks(string materialId)
    {
        return _materials.Values.Where(stack => stack.Material.ItemId == materialId);
    }

    /// <summary>
    /// Gets all materials of a specific category.
    /// </summary>
    /// <param name="category">The material category to filter by</param>
    /// <returns>Collection of stacks in this category</returns>
    public IEnumerable<MaterialStack> GetMaterialsByCategory(Category category)
    {
        return _materials.Values.Where(stack => stack.Material.Category == category);
    }

    /// <summary>
    /// Gets all materials of a specific rarity.
    /// </summary>
    /// <param name="quality">The rarity to filter by</param>
    /// <returns>Collection of stacks with this rarity</returns>
    public IEnumerable<MaterialStack> GetMaterialsByQualityTier(QualityTier quality)
    {
        return _materials.Values.Where(stack => stack.Material.Quality == quality);
    }

    /// <summary>
    /// Searches materials by name or partial text match.
    /// </summary>
    /// <param name="searchTerm">Text to search for in material names</param>
    /// <returns>Collection of matching material stacks</returns>
    public IEnumerable<MaterialStack> SearchMaterials(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return _materials.Values;
        }

        var lowercaseSearch = searchTerm.ToLowerInvariant();
        return _materials.Values.Where(stack => 
            stack.Material.Name.ToLowerInvariant().Contains(lowercaseSearch) ||
            stack.Material.Description.ToLowerInvariant().Contains(lowercaseSearch)
        );
    }

    /// <summary>
    /// Sorts materials by the specified criteria.
    /// </summary>
    /// <param name="sortBy">The sorting criteria</param>
    /// <param name="ascending">True for ascending order, false for descending</param>
    /// <returns>Sorted collection of material stacks</returns>
    public IEnumerable<MaterialStack> SortMaterials(MaterialSortBy sortBy, bool ascending = true)
    {
        var query = _materials.Values.AsEnumerable();

        query = sortBy switch
        {
            MaterialSortBy.Name => ascending 
                ? query.OrderBy(s => s.Material.Name)
                : query.OrderByDescending(s => s.Material.Name),
            MaterialSortBy.Quantity => ascending
                ? query.OrderBy(s => s.Quantity)
                : query.OrderByDescending(s => s.Quantity),
            MaterialSortBy.Rarity => ascending
                ? query.OrderBy(s => s.Material.Quality)
                : query.OrderByDescending(s => s.Material.Quality),
            MaterialSortBy.Category => ascending
                ? query.OrderBy(s => s.Material.Category)
                : query.OrderByDescending(s => s.Material.Category),
            MaterialSortBy.Value => ascending
                ? query.OrderBy(s => s.TotalValue)
                : query.OrderByDescending(s => s.TotalValue),
            MaterialSortBy.LastUpdated => ascending
                ? query.OrderBy(s => s.LastUpdated)
                : query.OrderByDescending(s => s.LastUpdated),
            _ => query
        };

        return query;
    }

    /// <summary>
    /// Expands the inventory capacity.
    /// </summary>
    /// <param name="additionalSlots">Number of additional slots to add</param>
    public void ExpandCapacity(int additionalSlots)
    {
        if (additionalSlots <= 0)
        {
            throw new ArgumentException("Additional slots must be greater than zero", nameof(additionalSlots));
        }

        _capacity += additionalSlots;
        CapacityChanged?.Invoke(_capacity);
        InventoryChanged?.Invoke();

        GameLogger.Info($"Expanded inventory capacity by {additionalSlots} slots (now {_capacity})");
    }

    /// <summary>
    /// Gets the total value of all materials in the inventory.
    /// </summary>
    /// <returns>Combined value of all material stacks</returns>
    public int GetTotalValue()
    {
        return _materials.Values.Sum(stack => stack.TotalValue);
    }

    /// <summary>
    /// Gets inventory statistics.
    /// </summary>
    /// <returns>Inventory statistics object</returns>
    public InventoryStats GetStats()
    {
        var totalMaterials = _materials.Values.Sum(stack => stack.Quantity);
        var categoryCounts = _materials.Values
            .GroupBy(stack => stack.Material.Category)
            .ToDictionary(g => g.Key, g => g.Sum(stack => stack.Quantity));
        var rarityCounts = _materials.Values
            .GroupBy(stack => stack.Material.Quality)
            .ToDictionary(g => g.Key, g => g.Sum(stack => stack.Quantity));

        return new InventoryStats(
            _capacity,
            UsedSlots,
            totalMaterials,
            GetTotalValue(),
            categoryCounts,
            rarityCounts
        );
    }

    /// <summary>
    /// Clears all materials from the inventory.
    /// </summary>
    public void Clear()
    {
        _materials.Clear();
        InventoryChanged?.Invoke();
        GameLogger.Info("Inventory cleared");
    }

    /// <summary>
    /// Creates a unique stack key for materials.
    /// </summary>
    private static string GetStackKey(string materialId, QualityTier rarity)
    {
        return $"{materialId}_{rarity}";
    }
}