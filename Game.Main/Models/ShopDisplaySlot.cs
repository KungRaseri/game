#nullable enable

using System;
using Godot;

namespace Game.Main.Models;

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

/// <summary>
/// Types of display cases available for shop slots.
/// Each type affects customer appeal and item presentation.
/// </summary>
public enum DisplayCaseType
{
    /// <summary>Basic wooden display case - no bonus effects.</summary>
    Basic = 0,
    
    /// <summary>Glass display case - increases perceived value by 10%.</summary>
    Glass = 1,
    
    /// <summary>Illuminated case - attracts more customer attention.</summary>
    Illuminated = 2,
    
    /// <summary>Security case - prevents theft, allows higher-value items.</summary>
    Security = 3,
    
    /// <summary>Premium showcase - maximum customer appeal and value perception.</summary>
    Premium = 4
}

/// <summary>
/// Visual styling options for display slots.
/// Affects customer appeal and shop aesthetics.
/// </summary>
public record DisplaySlotStyling
{
    /// <summary>Background color for the display area.</summary>
    public Color BackgroundColor { get; init; } = Colors.White;
    
    /// <summary>Border/frame color for the display case.</summary>
    public Color BorderColor { get; init; } = Colors.SaddleBrown;
    
    /// <summary>Whether this slot has accent lighting.</summary>
    public bool HasAccentLighting { get; init; } = false;
    
    /// <summary>Custom decorative elements around the display.</summary>
    public string DecorationTheme { get; init; } = "None";
    
    /// <summary>Label style for item name and price display.</summary>
    public LabelStyle LabelStyle { get; init; } = LabelStyle.Standard;
}

/// <summary>
/// Label styles for price and item name display.
/// </summary>
public enum LabelStyle
{
    /// <summary>Basic text label.</summary>
    Standard = 0,
    
    /// <summary>Elegant script font.</summary>
    Elegant = 1,
    
    /// <summary>Bold, eye-catching style.</summary>
    Bold = 2,
    
    /// <summary>Minimalist, clean style.</summary>
    Minimalist = 3,
    
    /// <summary>Luxury gold-accented style.</summary>
    Luxury = 4
}
