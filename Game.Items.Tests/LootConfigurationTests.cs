#nullable enable

using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Tests;

public class LootConfigurationTests
{
    [Fact]
    public void LootConfiguration_GetMaterials_ReturnsValidMaterials()
    {
        // Act
        var materials = LootConfiguration.GetMaterials();

        // Assert
        Assert.NotEmpty(materials);

        // Verify all materials are valid
        foreach (var material in materials.Values)
        {
            // Should not throw when validating
            material.Validate();
        }
    }

    [Fact]
    public void LootConfiguration_GetMaterials_ContainsExpectedMaterials()
    {
        // Act
        var materials = LootConfiguration.GetMaterials();

        // Assert - Check for specific expected materials
        Assert.True(materials.ContainsKey("ore_iron"));
        Assert.True(materials.ContainsKey("silver_ore"));
        Assert.True(materials.ContainsKey("gold_ore"));
        Assert.True(materials.ContainsKey("crude_leather"));
        Assert.True(materials.ContainsKey("thick_leather"));
        Assert.True(materials.ContainsKey("herbs"));
        Assert.True(materials.ContainsKey("coal"));
        Assert.True(materials.ContainsKey("gem_shard"));
        Assert.True(materials.ContainsKey("enchanted_stone"));
        Assert.True(materials.ContainsKey("mana_crystal"));
    }

    [Fact]
    public void LootConfiguration_GetMaterials_HasCorrectCategories()
    {
        // Act
        var materials = LootConfiguration.GetMaterials();

        // Assert - Verify material categories
        Assert.Equal(Category.Metal, materials["ore_iron"].Category);
        Assert.Equal(Category.Metal, materials["silver_ore"].Category);
        Assert.Equal(Category.Metal, materials["gold_ore"].Category);

        Assert.Equal(Category.Leather, materials["crude_leather"].Category);
        Assert.Equal(Category.Leather, materials["thick_leather"].Category);
        Assert.Equal(Category.Herb, materials["herbs"].Category);
        Assert.Equal(Category.Fuel, materials["coal"].Category);

        Assert.Equal(Category.Gem, materials["gem_shard"].Category);

        Assert.Equal(Category.Magical, materials["enchanted_stone"].Category);
        Assert.Equal(Category.Magical, materials["mana_crystal"].Category);
    }

    [Fact]
    public void LootConfiguration_GetMaterials_HasCorrectRarities()
    {
        // Act
        var materials = LootConfiguration.GetMaterials();

        // Assert - Verify rarity progression
        Assert.Equal(QualityTier.Common, materials["ore_iron"].Quality);
        Assert.Equal(QualityTier.Uncommon, materials["silver_ore"].Quality);
        Assert.Equal(QualityTier.Rare, materials["gold_ore"].Quality);

        Assert.Equal(QualityTier.Common, materials["crude_leather"].Quality);
        Assert.Equal(QualityTier.Uncommon, materials["thick_leather"].Quality);

        Assert.Equal(QualityTier.Rare, materials["gem_shard"].Quality);
        Assert.Equal(QualityTier.Epic, materials["enchanted_stone"].Quality);
        Assert.Equal(QualityTier.Legendary, materials["mana_crystal"].Quality);
    }

    [Fact]
    public void LootConfiguration_GetMaterials_HasReasonableValues()
    {
        // Act
        var materials = LootConfiguration.GetMaterials();

        // Assert - Verify value progression matches rarity
        Assert.True(materials["ore_iron"].BaseValue < materials["silver_ore"].BaseValue);
        Assert.True(materials["silver_ore"].BaseValue < materials["gold_ore"].BaseValue);

        Assert.True(materials["crude_leather"].BaseValue < materials["thick_leather"].BaseValue);

        // Higher rarity materials should have higher base values
        Assert.True(materials["gem_shard"].BaseValue > materials["ore_iron"].BaseValue);
        Assert.True(materials["enchanted_stone"].BaseValue > materials["gem_shard"].BaseValue);
        Assert.True(materials["mana_crystal"].BaseValue > materials["enchanted_stone"].BaseValue);
    }

    [Fact]
    public void LootConfiguration_GetLootTables_ReturnsValidTables()
    {
        // Act
        var lootTables = LootConfiguration.GetLootTables();

        // Assert
        Assert.NotEmpty(lootTables);

        // Verify all loot tables are valid
        foreach (var table in lootTables.Values)
        {
            // Should not throw when validating
            table.Validate();
        }
    }

    [Fact]
    public void LootConfiguration_GetLootTables_ContainsExpectedMonsters()
    {
        // Act
        var lootTables = LootConfiguration.GetLootTables();

        // Assert - Check for expected monster types
        Assert.True(lootTables.ContainsKey("goblin"));
        Assert.True(lootTables.ContainsKey("orc"));
        Assert.True(lootTables.ContainsKey("troll"));
    }

    [Fact]
    public void LootConfiguration_GetLootTables_HasProgressiveDropRates()
    {
        // Act
        var lootTables = LootConfiguration.GetLootTables();

        // Assert - Verify that stronger monsters have better guaranteed drops
        var goblinTable = lootTables["goblin"];
        var orcTable = lootTables["orc"];
        var trollTable = lootTables["troll"];

        Assert.True(goblinTable.GuaranteedDropCount <= orcTable.GuaranteedDropCount);
        Assert.True(orcTable.GuaranteedDropCount <= trollTable.GuaranteedDropCount);

        Assert.True(goblinTable.MaximumDropCount <= orcTable.MaximumDropCount);
        Assert.True(orcTable.MaximumDropCount <= trollTable.MaximumDropCount);
    }

    [Fact]
    public void LootConfiguration_GetLootTables_GoblinHasBasicMaterials()
    {
        // Act
        var lootTables = LootConfiguration.GetLootTables();
        var goblinTable = lootTables["goblin"];

        // Assert - Goblin should primarily drop common materials
        var materials = goblinTable.PossibleDrops.Select(entry => entry.Material.ItemId).ToList();

        Assert.Contains("ore_iron", materials);
        Assert.Contains("crude_leather", materials);
        Assert.Contains("herbs", materials);

        // Should have some chance for better materials but not guaranteed
        var ironOreEntries = goblinTable.PossibleDrops.Where(e => e.Material.ItemId == "ore_iron").ToList();
        Assert.Contains(ironOreEntries, e => e.DropChance >= 0.5f); // High chance for common iron ore
    }

    [Fact]
    public void LootConfiguration_GetLootTables_TrollHasRareMaterials()
    {
        // Act
        var lootTables = LootConfiguration.GetLootTables();
        var trollTable = lootTables["troll"];

        // Assert - Troll should have access to rare materials
        var materials = trollTable.PossibleDrops.Select(entry => entry.Material.ItemId).ToList();

        Assert.Contains("gold_ore", materials);
        Assert.Contains("gem_shard", materials);
        Assert.Contains("enchanted_stone", materials);
        Assert.Contains("mana_crystal", materials);

        // Should have higher guaranteed drops than goblins
        Assert.True(trollTable.GuaranteedDropCount >= 3);
    }

    [Fact]
    public void LootConfiguration_CreateLootGenerator_ReturnsValidGenerator()
    {
        // Act
        var generator = LootConfiguration.CreateLootGenerator();

        // Assert
        Assert.NotNull(generator);

        // Verify it can generate drops for configured monsters
        Assert.True(generator.HasLootTable("goblin"));
        Assert.True(generator.HasLootTable("orc"));
        Assert.True(generator.HasLootTable("troll"));
    }

    [Fact]
    public void LootConfiguration_CreateLootGenerator_WithCustomRandom_UsesProvidedRandom()
    {
        // Arrange
        var customRandom = new Random(42);

        // Act
        var generator = LootConfiguration.CreateLootGenerator(customRandom);

        // Assert
        Assert.NotNull(generator);

        // Generate drops with fixed seed should be deterministic
        var drops1 = generator.GenerateDrops("goblin");

        // Create another generator with same seed
        var generator2 = LootConfiguration.CreateLootGenerator(new Random(42));
        var drops2 = generator2.GenerateDrops("goblin");

        // Results should be identical with same seed
        Assert.Equal(drops1.Count, drops2.Count);
    }

    [Fact]
    public void LootConfiguration_AllMaterialsUsedInLootTables()
    {
        // Act
        var materials = LootConfiguration.GetMaterials();
        var lootTables = LootConfiguration.GetLootTables();

        // Assert - Every material should be used in at least one loot table
        var usedMaterialIds = new HashSet<string>();

        foreach (var table in lootTables.Values)
        {
            foreach (var entry in table.PossibleDrops)
            {
                usedMaterialIds.Add(entry.Material.ItemId);
            }
        }

        foreach (var materialId in materials.Keys)
        {
            Assert.True(usedMaterialIds.Contains(materialId),
                $"Material '{materialId}' is not used in any loot table");
        }
    }
}