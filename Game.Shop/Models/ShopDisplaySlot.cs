using Godot;

namespace Game.Shop.Models;

/// <summary>
/// Represents a configurable display slot in the shop where items can be placed for sale.
/// Includes position, pricing, and visual customization options.
/// </summary>
public record ShopDisplaySlot
{
    /// <summary>
    /// Unique identifier for this display slot (0-5 for the 6 initial slots).
    /// </summary>
    public int SlotId { get; init; }

    /// <summary>
    /// 2D position of the display slot in the shop layout (for UI positioning).
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// The item currently displayed in this slot, null if empty.
    /// </summary>
    public Item.Models.Item? CurrentItem { get; init; }

    /// <summary>
    /// The price set for the item in this slot.
    /// </summary>
    public decimal CurrentPrice { get; init; }

    /// <summary>
    /// Whether this slot currently has an item placed in it.
    /// </summary>
    public bool IsOccupied => CurrentItem != null;

    /// <summary>
    /// The type of display case used for this slot (affects customer appeal).
    /// </summary>
    public DisplayCaseType CaseType { get; init; } = DisplayCaseType.Basic;

    /// <summary>
    /// Visual styling options for this display slot.
    /// </summary>
    public DisplaySlotStyling Styling { get; init; } = new();

    /// <summary>
    /// When the item was last restocked in this slot.
    /// </summary>
    public DateTime LastRestocked { get; init; } = DateTime.Now;

    /// <summary>
    /// Creates a new display slot with an item and price.
    /// </summary>
    public ShopDisplaySlot WithItem(Item.Models.Item item, decimal price)
    {
        return this with
        {
            CurrentItem = item,
            CurrentPrice = price,
            LastRestocked = DateTime.Now
        };
    }

    /// <summary>
    /// Removes the item from this display slot.
    /// </summary>
    public ShopDisplaySlot WithoutItem()
    {
        return this with
        {
            CurrentItem = null,
            CurrentPrice = 0m,
            LastRestocked = DateTime.Now
        };
    }

    /// <summary>
    /// Updates the price for the current item.
    /// </summary>
    public ShopDisplaySlot WithPrice(decimal newPrice)
    {
        return this with { CurrentPrice = newPrice };
    }

    /// <summary>
    /// Updates the display case type.
    /// </summary>
    public ShopDisplaySlot WithCaseType(DisplayCaseType caseType)
    {
        return this with { CaseType = caseType };
    }
}