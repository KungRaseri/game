#nullable enable

namespace Game.Game.Item.Models.Materials;

/// <summary>
/// Defines the rarity levels for materials, affecting drop rates and value.
/// Each rarity level has associated drop rate ranges and visual styling.
/// </summary>
public enum Rarity
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
