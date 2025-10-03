using Game.Items.Models;

namespace Game.Main.Utils;

/// <summary>
/// Provides stat modifiers and bonuses based on item quality tiers.
/// </summary>
public static class QualityTierModifiers
{
    /// <summary>
    /// Gets the weapon damage bonus for a given quality tier.
    /// </summary>
    public static int GetWeaponDamageBonus(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.Common => 5,
            QualityTier.Uncommon => 10,
            QualityTier.Rare => 15,
            QualityTier.Epic => 20,
            QualityTier.Legendary => 30,
            _ => 5
        };
    }

    /// <summary>
    /// Gets the armor damage reduction for a given quality tier.
    /// </summary>
    public static int GetArmorDamageReduction(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.Common => 3,
            QualityTier.Uncommon => 6,
            QualityTier.Rare => 9,
            QualityTier.Epic => 12,
            QualityTier.Legendary => 18,
            _ => 3
        };
    }

    /// <summary>
    /// Gets the value multiplier for a given quality tier.
    /// This can be used to calculate item gold value.
    /// </summary>
    public static float GetValueMultiplier(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.Common => 1.0f,
            QualityTier.Uncommon => 2.0f,
            QualityTier.Rare => 4.0f,
            QualityTier.Epic => 8.0f,
            QualityTier.Legendary => 16.0f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Calculates the item value based on a base value and quality tier.
    /// </summary>
    public static int CalculateItemValue(int baseValue, QualityTier quality)
    {
        return (int)Math.Round(baseValue * GetValueMultiplier(quality));
    }
}
