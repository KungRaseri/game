using System;

namespace Game.Core.Models;

/// <summary>
/// Represents a crafting material used to create items.
/// </summary>
public class Material : Item
{
    private int _maxStackSize;

    /// <summary>
    /// The specific type of material (Metal, Wood, Leather, etc.).
    /// </summary>
    public MaterialType MaterialType { get; }

    /// <summary>
    /// Whether this material can be stacked in inventory.
    /// </summary>
    public bool Stackable { get; }

    /// <summary>
    /// Maximum number of this material that can be in a single stack.
    /// </summary>
    public int MaxStackSize
    {
        get => _maxStackSize;
        set => _maxStackSize = Math.Max(1, value);
    }

    public Material(
        string itemId,
        string name,
        string description,
        QualityTier quality,
        int value,
        MaterialType materialType,
        bool stackable = true,
        int maxStackSize = 99)
        : base(itemId, name, description, ItemType.Material, quality, value)
    {
        MaterialType = materialType;
        Stackable = stackable;
        _maxStackSize = Math.Max(1, maxStackSize);
    }

    public override string ToString()
    {
        return $"{Name} ({Quality} {MaterialType}) - {Value}g [Stack: {MaxStackSize}]";
    }
}
