using Game.Items.Models;

namespace Game.Shop.Systems;

/// <summary>
/// Summary of inventory status optimized for shop management operations.
/// </summary>
public record ShopInventorySummary
{
    /// <summary>Items available to be stocked in shop.</summary>
    public List<Item> AvailableItems { get; init; } = new();

    /// <summary>Items currently displayed in shop.</summary>
    public List<Item> DisplayedItems { get; init; } = new();

    /// <summary>Total estimated value of available items.</summary>
    public decimal TotalAvailableValue { get; init; }

    /// <summary>Total listed value of displayed items.</summary>
    public decimal TotalDisplayedValue { get; init; }

    /// <summary>Count of available items by type.</summary>
    public Dictionary<ItemType, int> ItemTypes { get; init; } = new();

    /// <summary>Count of available items by quality.</summary>
    public Dictionary<QualityTier, int> QualityDistribution { get; init; } = new();
}