using System;

namespace Game.Core.Models;

/// <summary>
/// Defines the rarity levels for materials, affecting drop rates and value.
/// Each rarity level has associated drop rate ranges and visual styling.
/// </summary>
public enum MaterialRarity
{
    /// <summary>
    /// Common materials (Gray) - 60-80% drop rates.
    /// Basic crafting materials that form the foundation of most recipes.
    /// </summary>
    Common,

    /// <summary>
    /// Uncommon materials (Green) - 15-30% drop rates.
    /// Improved materials that enhance item quality and provide better stats.
    /// </summary>
    Uncommon,

    /// <summary>
    /// Rare materials (Blue) - 5-15% drop rates.
    /// Special materials used for advanced recipes and significant bonuses.
    /// </summary>
    Rare,

    /// <summary>
    /// Epic materials (Purple) - 1-5% drop rates.
    /// High-tier crafting components for powerful items and equipment.
    /// </summary>
    Epic,

    /// <summary>
    /// Legendary materials (Gold) - 0.1-1% drop rates.
    /// Unique and extremely powerful materials for the most exclusive items.
    /// </summary>
    Legendary
}

/// <summary>
/// Defines the different categories of materials that can be collected.
/// Each category represents a broad classification for organizing materials.
/// </summary>
public enum MaterialCategory
{
    /// <summary>
    /// Metal-based materials like Iron Ore, Silver Ore, Gold Ore, Mithril.
    /// Used primarily for weapon and armor crafting.
    /// </summary>
    Metals,

    /// <summary>
    /// Organic materials like Herbs, Leather, Bone, Wood.
    /// Used for consumables, light armor, and utility items.
    /// </summary>
    Organic,

    /// <summary>
    /// Precious gems like Sapphire, Ruby, Diamond, Enchanted Crystals.
    /// Used for high-value items and magical enhancements.
    /// </summary>
    Gems,

    /// <summary>
    /// Magical components like Essence, Dust, Shards, Cores.
    /// Used for enchantments and magical item creation.
    /// </summary>
    Magical,

    /// <summary>
    /// Rare and unique components for special recipes.
    /// Used for legendary items and unique crafting combinations.
    /// </summary>
    Specialty
}

/// <summary>
/// Defines a type of material that can be collected from monsters.
/// This is the master definition for a material, separate from individual drops.
/// </summary>
/// <param name="Id">Unique identifier for the material type</param>
/// <param name="Name">Display name of the material</param>
/// <param name="Description">Detailed description of the material and its uses</param>
/// <param name="Category">The category this material belongs to</param>
/// <param name="BaseRarity">The most common rarity level for this material</param>
/// <param name="StackLimit">Maximum number that can be stacked in a single inventory slot</param>
/// <param name="BaseValue">Base gold value per unit for trading calculations</param>
public record MaterialType(
    string Id,
    string Name,
    string Description,
    MaterialCategory Category,
    MaterialRarity BaseRarity,
    int StackLimit = 999,
    int BaseValue = 1
)
{
    /// <summary>
    /// Validates that the MaterialType has valid configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("Material ID cannot be null or empty", nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("Material Name cannot be null or empty", nameof(Name));
        }

        if (StackLimit <= 0)
        {
            throw new ArgumentException("Stack limit must be greater than zero", nameof(StackLimit));
        }

        if (BaseValue < 0)
        {
            throw new ArgumentException("Base value cannot be negative", nameof(BaseValue));
        }
    }

    /// <summary>
    /// Gets the display color associated with the base rarity.
    /// </summary>
    public string GetRarityColor() => BaseRarity switch
    {
        MaterialRarity.Common => "#808080",     // Gray
        MaterialRarity.Uncommon => "#00FF00",   // Green
        MaterialRarity.Rare => "#0080FF",       // Blue
        MaterialRarity.Epic => "#8000FF",       // Purple
        MaterialRarity.Legendary => "#FFD700",  // Gold
        _ => "#FFFFFF"                          // White fallback
    };
}

/// <summary>
/// Represents a crafting material used to create items.
/// </summary>
public class Material : Item
{
    private int _maxStackSize;

    /// <summary>
    /// The specific type of material (Metal, Wood, Leather, etc.).
    /// </summary>
    public MaterialCategory Category { get; }

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
        MaterialCategory category,
        bool stackable = true,
        int maxStackSize = 99)
        : base(itemId, name, description, ItemType.Material, quality, value)
    {
        Category = category;
        Stackable = stackable;
        _maxStackSize = Math.Max(1, maxStackSize);
    }

    public override string ToString()
    {
        return $"{Name} ({Quality} {Category}) - {Value}g [Stack: {MaxStackSize}]";
    }
}
