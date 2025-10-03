using Game.Item.Models.Materials;

namespace Game.Inventory.Models;

/// <summary>
/// Result of adding materials to inventory.
/// </summary>
public class InventoryAddResult
{
    public List<Drop> SuccessfulAdds { get; } = new();
    public List<Drop> PartialAdds { get; } = new();
    public List<Drop> FailedAdds { get; } = new();

    public bool HasAnyChanges => SuccessfulAdds.Count > 0 || PartialAdds.Count > 0;
    public bool AllSuccessful => FailedAdds.Count == 0 && PartialAdds.Count == 0;
    public int TotalProcessed => SuccessfulAdds.Count + PartialAdds.Count + FailedAdds.Count;
}

/// <summary>
/// Sort options for material inventory.
/// </summary>
public enum MaterialSortBy
{
    Name,
    Type,
    Rarity,
    Quantity,
    Value,
    DateAdded
}

/// <summary>
/// Statistics about inventory state and usage.
/// </summary>
public record InventoryStats
{
    /// <summary>Number of occupied slots.</summary>
    public int UsedSlots { get; init; }
    
    /// <summary>Total inventory capacity.</summary>
    public int TotalSlots { get; init; }
    
    /// <summary>Number of free slots remaining.</summary>
    public int FreeSlots { get; init; }
    
    /// <summary>Total quantity of all materials.</summary>
    public int TotalQuantity { get; init; }
    
    /// <summary>Total value of all materials.</summary>
    public int TotalValue { get; init; }
    
    /// <summary>Number of different material types.</summary>
    public int UniqueTypes { get; init; }
    
    /// <summary>Utilization percentage (0.0 to 1.0).</summary>
    public float Utilization => TotalSlots > 0 ? (float)UsedSlots / TotalSlots : 0f;
    
    /// <summary>Whether the inventory is at capacity.</summary>
    public bool IsFull => FreeSlots == 0;
    
    /// <summary>Whether the inventory is empty.</summary>
    public bool IsEmpty => UsedSlots == 0;
}
