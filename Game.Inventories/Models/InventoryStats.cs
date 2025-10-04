namespace Game.Inventories.Models;

/// <summary>
/// Statistics about inventory state and usage.
/// </summary>
public record InventoryStats
{
    /// <summary>Number of occupied slots.</summary>
    public int UsedSlots { get; init; }

    /// <summary>Total inventory capacity.</summary>
    public int TotalSlots { get; init; }
    
    /// <summary>Total inventory capacity (alias for TotalSlots).</summary>
    public int Capacity => TotalSlots;

    /// <summary>Number of free slots remaining.</summary>
    public int FreeSlots { get; init; }

    /// <summary>Total quantity of all materials.</summary>
    public int TotalQuantity { get; init; }
    
    /// <summary>Total quantity of all materials (alias for TotalQuantity).</summary>
    public int TotalMaterials => TotalQuantity;

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