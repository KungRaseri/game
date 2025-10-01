using Game.Main.Models;

namespace Game.Main.Data;

/// <summary>
/// Predefined item configurations for common items in the game.
/// </summary>
public static class ItemTypes
{
    // Weapons
    public static WeaponConfig IronSword { get; } = new(
        ItemId: "weapon_iron_sword",
        Name: "Iron Sword",
        Description: "A sturdy iron blade, reliable in combat.",
        BaseValue: 50,
        BaseDamageBonus: 5
    );

    public static WeaponConfig SteelAxe { get; } = new(
        ItemId: "weapon_steel_axe",
        Name: "Steel Axe",
        Description: "A sharp steel axe that cleaves through enemies.",
        BaseValue: 75,
        BaseDamageBonus: 8
    );

    public static WeaponConfig MithrilDagger { get; } = new(
        ItemId: "weapon_mithril_dagger",
        Name: "Mithril Dagger",
        Description: "A lightweight dagger forged from rare mithril.",
        BaseValue: 100,
        BaseDamageBonus: 6
    );

    // Armor
    public static ArmorConfig LeatherArmor { get; } = new(
        ItemId: "armor_leather",
        Name: "Leather Armor",
        Description: "Basic leather protection for adventurers.",
        BaseValue: 40,
        BaseDamageReduction: 3
    );

    public static ArmorConfig ChainMail { get; } = new(
        ItemId: "armor_chainmail",
        Name: "Chain Mail",
        Description: "Interlocking metal rings provide solid defense.",
        BaseValue: 80,
        BaseDamageReduction: 5
    );

    public static ArmorConfig PlateArmor { get; } = new(
        ItemId: "armor_plate",
        Name: "Plate Armor",
        Description: "Heavy steel plates offering maximum protection.",
        BaseValue: 150,
        BaseDamageReduction: 8
    );

    // Materials - Metals
    public static MaterialConfig IronOre { get; } = new(
        ItemId: "material_iron_ore",
        Name: "Iron Ore",
        Description: "Raw iron ore, can be smelted into ingots.",
        BaseValue: 5,
        MaterialType: MaterialType.Metal,
        Stackable: true,
        MaxStackSize: 99
    );

    public static MaterialConfig SteelIngot { get; } = new(
        ItemId: "material_steel_ingot",
        Name: "Steel Ingot",
        Description: "Refined steel ingot, ready for crafting.",
        BaseValue: 15,
        MaterialType: MaterialType.Metal,
        Stackable: true,
        MaxStackSize: 99
    );

    // Materials - Leather
    public static MaterialConfig MonsterHide { get; } = new(
        ItemId: "material_monster_hide",
        Name: "Monster Hide",
        Description: "Tough hide from a defeated monster.",
        BaseValue: 8,
        MaterialType: MaterialType.Leather,
        Stackable: true,
        MaxStackSize: 50
    );

    public static MaterialConfig TannedLeather { get; } = new(
        ItemId: "material_tanned_leather",
        Name: "Tanned Leather",
        Description: "Processed leather suitable for armor crafting.",
        BaseValue: 12,
        MaterialType: MaterialType.Leather,
        Stackable: true,
        MaxStackSize: 50
    );

    // Materials - Wood
    public static MaterialConfig OakWood { get; } = new(
        ItemId: "material_oak_wood",
        Name: "Oak Wood",
        Description: "Strong oak wood for weapon handles and shields.",
        BaseValue: 3,
        MaterialType: MaterialType.Wood,
        Stackable: true,
        MaxStackSize: 99
    );

    // Materials - Gems
    public static MaterialConfig Ruby { get; } = new(
        ItemId: "material_ruby",
        Name: "Ruby",
        Description: "A precious red gemstone that radiates power.",
        BaseValue: 100,
        MaterialType: MaterialType.Gem,
        Stackable: true,
        MaxStackSize: 20
    );
}
