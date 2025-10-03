namespace Game.Item.Models;

/// <summary>
/// Base class for all items in the game.
/// </summary>
public class Item
{
    private string _itemId;
    private string _name;
    private string _description;
    private int _value;

    /// <summary>
    /// Unique identifier for this item.
    /// </summary>
    public string ItemId
    {
        get => _itemId;
        set => _itemId = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Display name of the item.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Description of the item.
    /// </summary>
    public string Description
    {
        get => _description;
        set => _description = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The type of item (Weapon, Armor, Material, etc.).
    /// </summary>
    public ItemType ItemType { get; set; }

    /// <summary>
    /// The quality tier of this item.
    /// </summary>
    public QualityTier Quality { get; set; }

    /// <summary>
    /// Gold value of this item.
    /// </summary>
    public int Value
    {
        get => _value;
        set => _value = Math.Max(0, value);
    }

    public Item(string itemId, string name, string description, ItemType itemType, QualityTier quality, int value)
    {
        _itemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _description = description ?? throw new ArgumentNullException(nameof(description));
        ItemType = itemType;
        Quality = quality;
        _value = Math.Max(0, value);
    }

    public override string ToString()
    {
        return $"{Name} ({Quality} {ItemType}) - {Value}g";
    }
}
