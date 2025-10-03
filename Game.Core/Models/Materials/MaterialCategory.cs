#nullable enable

namespace Game.Core.Models.Materials;

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
