using Game.Items.Models;
using Game.Items.Models.Materials;
using QualityTierModifiersUtils = Game.Items.Utils.QualityTierModifiers;

namespace Game.Items.Data;

/// <summary>
/// Factory for creating items from configuration.
/// Applies quality tier modifiers to base item stats.
/// </summary>
public static class ItemFactory
{
    /// <summary>
    /// Creates a weapon from a configuration with the specified quality tier.
    /// </summary>
    public static Weapon CreateWeapon(WeaponConfig config, QualityTier quality)
    {
        int damageBonus = QualityTierModifiersUtils.GetWeaponDamageBonus(quality);
        int finalValue = QualityTierModifiersUtils.CalculateItemValue(config.BaseValue, quality);

        return new Weapon(
            itemId: $"{config.ItemId}_{quality.ToString().ToLower()}",
            name: config.Name,
            description: config.Description,
            quality: quality,
            value: finalValue,
            damageBonus: damageBonus
        );
    }

    /// <summary>
    /// Creates an armor from a configuration with the specified quality tier.
    /// </summary>
    public static Armor CreateArmor(ArmorConfig config, QualityTier quality)
    {
        int damageReduction = QualityTierModifiersUtils.GetArmorDamageReduction(quality);
        int finalValue = QualityTierModifiersUtils.CalculateItemValue(config.BaseValue, quality);

        return new Armor(
            itemId: $"{config.ItemId}_{quality.ToString().ToLower()}",
            name: config.Name,
            description: config.Description,
            quality: quality,
            value: finalValue,
            damageReduction: damageReduction
        );
    }

    /// <summary>
    /// Creates a material from a configuration with the specified quality tier.
    /// </summary>
    public static Material CreateMaterial(MaterialConfig config, QualityTier quality)
    {
        int finalValue = QualityTierModifiersUtils.CalculateItemValue(config.BaseValue, quality);

        return new Material(
            itemId: $"{config.ItemId}_{quality.ToString().ToLower()}",
            name: config.Name,
            description: config.Description,
            quality: quality,
            value: finalValue,
            category: config.Category,
            stackable: config.Stackable,
            maxStackSize: config.MaxStackSize
        );
    }

    // Convenience methods for common items with Common quality as default

    public static Weapon CreateIronSword(QualityTier quality = QualityTier.Common)
        => CreateWeapon(ItemTypes.IronSword, quality);

    public static Weapon CreateSteelAxe(QualityTier quality = QualityTier.Common)
        => CreateWeapon(ItemTypes.SteelAxe, quality);

    public static Weapon CreateMithrilDagger(QualityTier quality = QualityTier.Common)
        => CreateWeapon(ItemTypes.MithrilDagger, quality);

    public static Armor CreateLeatherArmor(QualityTier quality = QualityTier.Common)
        => CreateArmor(ItemTypes.LeatherArmor, quality);

    public static Armor CreateChainMail(QualityTier quality = QualityTier.Common)
        => CreateArmor(ItemTypes.ChainMail, quality);

    public static Armor CreatePlateArmor(QualityTier quality = QualityTier.Common)
        => CreateArmor(ItemTypes.PlateArmor, quality);

    public static Material CreateIronOre(QualityTier quality = QualityTier.Common)
        => CreateMaterial(ItemTypes.IronOre, quality);

    public static Material CreateSteelIngot(QualityTier quality = QualityTier.Common)
        => CreateMaterial(ItemTypes.SteelIngot, quality);

    public static Material CreateMonsterHide(QualityTier quality = QualityTier.Common)
        => CreateMaterial(ItemTypes.MonsterHide, quality);

    public static Material CreateTannedLeather(QualityTier quality = QualityTier.Common)
        => CreateMaterial(ItemTypes.TannedLeather, quality);

    public static Material CreateOakWood(QualityTier quality = QualityTier.Common)
        => CreateMaterial(ItemTypes.OakWood, quality);

    public static Material CreateRuby(QualityTier quality = QualityTier.Common)
        => CreateMaterial(ItemTypes.Ruby, quality);
}