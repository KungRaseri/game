#nullable enable

using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Systems;

namespace Game.Items.Data;

/// <summary>
/// Provides pre-configured loot tables and material definitions for the game.
/// This serves as the central configuration for all loot generation.
/// </summary>
public static class LootConfiguration
{
    /// <summary>
    /// Gets all predefined material types available in the game.
    /// </summary>
    public static Dictionary<string, Material> GetMaterials()
    {
        var materials = new Dictionary<string, Material>();

        // Metal materials
        materials["ore_iron"] = new Material(
            "ore_iron",
            "Iron Ore",
            "Common metal ore used in basic weapon and armor crafting.",
            QualityTier.Common,
            2,
            Category.Metal,
            true,
            999
        );

        materials["silver_ore"] = new Material(
            "silver_ore",
            "Silver Ore",
            "Refined metal ore with enhanced properties for quality equipment.",
            QualityTier.Uncommon,
            8,
            Category.Metal,
            true,
            999
        );

        materials["gold_ore"] = new Material(
            "gold_ore",
            "Gold Ore",
            "Precious metal ore used for high-value items and decorative enhancements.",
            QualityTier.Rare,
            25,
            Category.Metal,
            true,
            999
        );

        // Organic materials
        materials["crude_leather"] = new Material(
            "crude_leather",
            "Crude Leather",
            "Basic leather material from common creatures, suitable for simple armor.",
            QualityTier.Common,
            1,
            Category.Leather,
            true,
            999
        );

        materials["thick_leather"] = new Material(
            "thick_leather",
            "Thick Leather",
            "Durable leather from larger creatures, provides better protection.",
            QualityTier.Uncommon,
            4,
            Category.Leather,
            true,
            999
        );

        materials["herbs"] = new Material(
            "herbs",
            "Medicinal Herbs",
            "Common plants with healing properties, used in potion making.",
            QualityTier.Common,
            1,
            Category.Herb,
            true,
            999
        );

        materials["coal"] = new Material(
            "coal",
            "Coal",
            "Fuel material used in forging and smelting operations.",
            QualityTier.Common,
            1,
            Category.Fuel,
            true,
            999
        );

        // Gems
        materials["gem_shard"] = new Material(
            "gem_shard",
            "Gem Shard",
            "Small fragments of precious gems, can be combined for greater value.",
            QualityTier.Rare,
            15,
            Category.Gem,
            true,
            999
        );

        // Magical materials
        materials["enchanted_stone"] = new Material(
            "enchanted_stone",
            "Enchanted Stone",
            "Magically infused stone that enhances weapon and armor properties.",
            QualityTier.Epic,
            50,
            Category.Magical,
            true,
            999
        );

        materials["mana_crystal"] = new Material(
            "mana_crystal",
            "Mana Crystal",
            "Crystallized magical energy, extremely rare and powerful.",
            QualityTier.Legendary,
            200,
            Category.Magical,
            true,
            999
        );

        return materials;
    }

    /// <summary>
    /// Gets all predefined loot tables for monster types.
    /// </summary>
    public static Dictionary<string, LootTable> GetLootTables()
    {
        var materials = GetMaterials();
        var lootTables = new Dictionary<string, LootTable>();

        // Goblin loot table
        lootTables["goblin"] = new LootTable(
            "goblin",
            new List<LootEntry>
            {
                new(materials["ore_iron"], 0.8f, 1, 3),
                new(materials["crude_leather"], 0.6f, 1, 2),
                new(materials["herbs"], 0.4f, 1, 1),
                new(materials["ore_iron"], 0.15f, 1, 1, QualityTier.Uncommon),
                new(materials["gem_shard"], 0.02f, 1, 1)
            },
            1,
            4
        );

        // Orc loot table
        lootTables["orc"] = new LootTable(
            "orc",
            new List<LootEntry>
            {
                new(materials["ore_iron"], 0.9f, 2, 4),
                new(materials["thick_leather"], 0.7f, 1, 2),
                new(materials["coal"], 0.5f, 1, 2),
                new(materials["silver_ore"], 0.25f, 1, 1),
                new(materials["enchanted_stone"], 0.05f, 1, 1)
            },
            2,
            5
        );

        // Troll loot table (more generous drops)
        lootTables["troll"] = new LootTable(
            "troll",
            new List<LootEntry>
            {
                new(materials["ore_iron"], 0.95f, 3, 6),
                new(materials["silver_ore"], 0.6f, 1, 3),
                new(materials["thick_leather"], 0.8f, 2, 3),
                new(materials["gold_ore"], 0.3f, 1, 2),
                new(materials["gem_shard"], 0.15f, 1, 2),
                new(materials["enchanted_stone"], 0.1f, 1, 1),
                new(materials["mana_crystal"], 0.01f, 1, 1)
            },
            3,
            6
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