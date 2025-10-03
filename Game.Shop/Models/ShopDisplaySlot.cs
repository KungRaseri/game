using Game.Items.Models;
using Godot;

namespace Game.Shop.Models;

/// <summary>
/// Types of display cases available for shop slots.
/// Each type provides different appeal levels to customers.
/// </summary>
public enum DisplayCaseType
{
    /// <summary>Basic wooden display case - standard appeal.</summary>
    Basic = 0,

    /// <summary>Glass display case - enhanced appeal for valuable items.</summary>
    Glass = 1,

    /// <summary>Ornate display case - premium appeal for luxury items.</summary>
    Ornate = 2,

    /// <summary>Magical display case - special effects and highest appeal.</summary>
    Magical = 3
}

/// <summary>
/// Visual styling options for display slots.
/// Affects customer appeal and shop aesthetics.
/// </summary>
public record DisplaySlotStyling
{
    /// <summary>Background color for the display slot.</summary>
    public Color BackgroundColor { get; init; } = Colors.White;

    /// <summary>Border color for the display slot.</summary>
    public Color BorderColor { get; init; } = Colors.Gray;

    /// <summary>Whether the slot has special lighting effects.</summary>
    public bool HasLighting { get; init; } = false;

    /// <summary>Decorative theme for the slot.</summary>
    public string Theme { get; init; } = "Default";

    /// <summary>Customer appeal multiplier for this styling (0.8 to 1.5).</summary>
    public float AppealMultiplier { get; init; } = 1.0f;
}

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
    public Item? CurrentItem { get; init; }
    
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
    public ShopDisplaySlot WithItem(Item item, decimal price)
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
