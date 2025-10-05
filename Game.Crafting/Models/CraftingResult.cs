using Game.Items.Models;

namespace Game.Crafting.Models;

/// <summary>
/// Represents the result of a crafting recipe when successfully completed.
/// </summary>
public class CraftingResult
{
    /// <summary>
    /// The item ID that will be created.
    /// </summary>
    public string ItemId { get; }

    /// <summary>
    /// The name of the item being crafted.
    /// </summary>
    public string ItemName { get; }

    /// <summary>
    /// The type of item being created.
    /// </summary>
    public ItemType ItemType { get; }

    /// <summary>
    /// The base quality tier of the crafted item.
    /// Final quality may be modified based on materials used.
    /// </summary>
    public QualityTier BaseQuality { get; }

    /// <summary>
    /// The quantity of items produced from this recipe.
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// The base value of the crafted item before quality modifiers.
    /// </summary>
    public int BaseValue { get; }

    /// <summary>
    /// Additional properties specific to the item being crafted.
    /// For example, damage bonus for weapons, defense for armor.
    /// </summary>
    public Dictionary<string, object> ItemProperties { get; }

    public CraftingResult(
        string itemId,
        string itemName,
        ItemType itemType,
        QualityTier baseQuality,
        int quantity,
        int baseValue,
        Dictionary<string, object>? itemProperties = null)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or empty", nameof(itemId));
        }

        if (string.IsNullOrWhiteSpace(itemName))
        {
            throw new ArgumentException("Item name cannot be null or empty", nameof(itemName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        if (baseValue < 0)
        {
            throw new ArgumentException("Base value cannot be negative", nameof(baseValue));
        }

        ItemId = itemId;
        ItemName = itemName;
        ItemType = itemType;
        BaseQuality = baseQuality;
        Quantity = quantity;
        BaseValue = baseValue;
        ItemProperties = itemProperties ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets a typed property value from the ItemProperties dictionary.
    /// </summary>
    /// <typeparam name="T">The expected type of the property</typeparam>
    /// <param name="propertyName">The name of the property to retrieve</param>
    /// <param name="defaultValue">The default value if the property doesn't exist</param>
    /// <returns>The property value or default value</returns>
    public T GetProperty<T>(string propertyName, T defaultValue = default!)
    {
        if (ItemProperties.TryGetValue(propertyName, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// Sets a property value in the ItemProperties dictionary.
    /// </summary>
    /// <param name="propertyName">The name of the property to set</param>
    /// <param name="value">The value to set</param>
    public void SetProperty(string propertyName, object value)
    {
        ItemProperties[propertyName] = value;
    }

    public override string ToString()
    {
        return $"{Quantity}x {ItemName} ({BaseQuality} {ItemType}) - {BaseValue}g";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CraftingResult other)
            return false;

        return ItemId == other.ItemId &&
               ItemName == other.ItemName &&
               ItemType == other.ItemType &&
               BaseQuality == other.BaseQuality &&
               Quantity == other.Quantity &&
               BaseValue == other.BaseValue;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ItemId, ItemName, ItemType, BaseQuality, Quantity, BaseValue);
    }
}
