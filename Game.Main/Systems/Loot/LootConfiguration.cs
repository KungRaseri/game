#nullable enable

using Game.Core.Models.Materials;

namespace Game.Main.Systems.Loot;

/// <summary>
/// Provides pre-configured loot tables and material definitions for the game.
/// This serves as the central configuration for all loot generation.
/// </summary>
public static class LootConfiguration
{
    /// <summary>
    /// Gets all predefined material types available in the game.
    /// </summary>
    public static Dictionary<string, MaterialType> GetMaterialTypes()
    {
        var materials = new Dictionary<string, MaterialType>();

        // Metal materials
        materials["iron_ore"] = new MaterialType(
            "iron_ore",
            "Iron Ore",
            "Common metal ore used in basic weapon and armor crafting.",
            MaterialCategory.Metals,
            MaterialRarity.Common,
            StackLimit: 999,
            BaseValue: 2
        );

        materials["silver_ore"] = new MaterialType(
            "silver_ore",
            "Silver Ore",
            "Refined metal ore with enhanced properties for quality equipment.",
            MaterialCategory.Metals,
            MaterialRarity.Uncommon,
            StackLimit: 999,
            BaseValue: 8
        );

        materials["gold_ore"] = new MaterialType(
            "gold_ore",
            "Gold Ore",
            "Precious metal ore used for high-value items and decorative enhancements.",
            MaterialCategory.Metals,
            MaterialRarity.Rare,
            StackLimit: 999,
            BaseValue: 25
        );

        // Organic materials
        materials["crude_leather"] = new MaterialType(
            "crude_leather",
            "Crude Leather",
            "Basic leather material from common creatures, suitable for simple armor.",
            MaterialCategory.Organic,
            MaterialRarity.Common,
            StackLimit: 999,
            BaseValue: 1
        );

        materials["thick_leather"] = new MaterialType(
            "thick_leather",
            "Thick Leather",
            "Durable leather from larger creatures, provides better protection.",
            MaterialCategory.Organic,
            MaterialRarity.Uncommon,
            StackLimit: 999,
            BaseValue: 4
        );

        materials["herbs"] = new MaterialType(
            "herbs",
            "Medicinal Herbs",
            "Common plants with healing properties, used in potion making.",
            MaterialCategory.Organic,
            MaterialRarity.Common,
            StackLimit: 999,
            BaseValue: 1
        );

        materials["coal"] = new MaterialType(
            "coal",
            "Coal",
            "Fuel material used in forging and smelting operations.",
            MaterialCategory.Organic,
            MaterialRarity.Common,
            StackLimit: 999,
            BaseValue: 1
        );

        // Gems
        materials["gem_shard"] = new MaterialType(
            "gem_shard",
            "Gem Shard",
            "Small fragments of precious gems, can be combined for greater value.",
            MaterialCategory.Gems,
            MaterialRarity.Rare,
            StackLimit: 999,
            BaseValue: 15
        );

        // Magical materials
        materials["enchanted_stone"] = new MaterialType(
            "enchanted_stone",
            "Enchanted Stone",
            "Magically infused stone that enhances weapon and armor properties.",
            MaterialCategory.Magical,
            MaterialRarity.Epic,
            StackLimit: 999,
            BaseValue: 50
        );

        materials["mana_crystal"] = new MaterialType(
            "mana_crystal",
            "Mana Crystal",
            "Crystallized magical energy, extremely rare and powerful.",
            MaterialCategory.Magical,
            MaterialRarity.Legendary,
            StackLimit: 999,
            BaseValue: 200
        );

        return materials;
    }

    /// <summary>
    /// Gets all predefined loot tables for monster types.
    /// </summary>
    public static Dictionary<string, LootTable> GetLootTables()
    {
        var materials = GetMaterialTypes();
        var lootTables = new Dictionary<string, LootTable>();

        // Goblin loot table
        lootTables["goblin"] = new LootTable(
            "goblin",
            new List<LootEntry>
            {
                new(materials["iron_ore"], 0.8f, 1, 3),
                new(materials["crude_leather"], 0.6f, 1, 2),
                new(materials["herbs"], 0.4f, 1, 1),
                new(materials["iron_ore"], 0.15f, 1, 1, MaterialRarity.Uncommon),
                new(materials["gem_shard"], 0.02f, 1, 1)
            },
            guaranteedDropCount: 1,
            maximumDropCount: 4
        );

        // Orc loot table
        lootTables["orc"] = new LootTable(
            "orc",
            new List<LootEntry>
            {
                new(materials["iron_ore"], 0.9f, 2, 4),
                new(materials["thick_leather"], 0.7f, 1, 2),
                new(materials["coal"], 0.5f, 1, 2),
                new(materials["silver_ore"], 0.25f, 1, 1),
                new(materials["enchanted_stone"], 0.05f, 1, 1)
            },
            guaranteedDropCount: 2,
            maximumDropCount: 5
        );

        // Troll loot table (more generous drops)
        lootTables["troll"] = new LootTable(
            "troll",
            new List<LootEntry>
            {
                new(materials["iron_ore"], 0.95f, 3, 6),
                new(materials["silver_ore"], 0.6f, 1, 3),
                new(materials["thick_leather"], 0.8f, 2, 3),
                new(materials["gold_ore"], 0.3f, 1, 2),
                new(materials["gem_shard"], 0.15f, 1, 2),
                new(materials["enchanted_stone"], 0.1f, 1, 1),
                new(materials["mana_crystal"], 0.01f, 1, 1)
            },
            guaranteedDropCount: 3,
            maximumDropCount: 6
        );

        return lootTables;
    }

    /// <summary>
    /// Creates a configured LootGenerator with all predefined loot tables.
    /// </summary>
    public static LootGenerator CreateLootGenerator(Random? random = null)
    {
        var lootTables = GetLootTables();
        return new LootGenerator(lootTables, random);
    }
}
