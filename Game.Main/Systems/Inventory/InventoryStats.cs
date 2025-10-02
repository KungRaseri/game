using Game.Main.Models.Materials;

namespace Game.Main.Systems.Inventory;

/// <summary>
/// Statistics about inventory contents and usage.
/// </summary>
/// <param name="Capacity">Maximum number of stacks</param>
/// <param name="UsedSlots">Number of occupied slots</param>
/// <param name="TotalMaterials">Total number of individual materials</param>
/// <param name="TotalValue">Combined value of all materials</param>
/// <param name="CategoryCounts">Materials count by category</param>
/// <param name="RarityCounts">Materials count by rarity</param>
public record InventoryStats(
    int Capacity,
    int UsedSlots,
    int TotalMaterials,
    int TotalValue,
    Dictionary<MaterialCategory, int> CategoryCounts,
    Dictionary<MaterialRarity, int> RarityCounts
);