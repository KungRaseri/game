#nullable enable

using Game.Main.Systems.Loot;

namespace Game.Main.Tests.Systems.Loot;

public class LootTableTests
{
    private readonly MaterialType _ironOre = new(
        "iron_ore",
        "Iron Ore",
        "Common metal ore",
        MaterialCategory.Metals,
        MaterialRarity.Common,
        BaseValue: 2
    );

    private readonly MaterialType _leather = new(
        "leather",
        "Leather",
        "Basic leather",
        MaterialCategory.Organic,
        MaterialRarity.Common,
        BaseValue: 1
    );

    private readonly MaterialType _gem = new(
        "gem",
        "Gem",
        "Precious gem",
        MaterialCategory.Gems,
        MaterialRarity.Rare,
        BaseValue: 10
    );

    [Fact]
    public void LootTable_ValidConfiguration_CreatesSuccessfully()
    {
        // Arrange
        var entries = new List<LootEntry>
        {
            new(_ironOre, 0.8f, 1, 3),
            new(_leather, 0.6f, 1, 2)
        };

        // Act
        var lootTable = new LootTable("goblin", entries, 1, 3);

        // Assert
        Assert.Equal("goblin", lootTable.MonsterTypeId);
        Assert.Equal(2, lootTable.PossibleDrops.Count);
        Assert.Equal(1, lootTable.GuaranteedDropCount);
        Assert.Equal(3, lootTable.MaximumDropCount);
    }

    [Fact]
    public void LootTable_WithDefaultValues_CreatesSuccessfully()
    {
        // Arrange
        var entries = new List<LootEntry>
        {
            new(_ironOre, 0.8f, 1, 3)
        };

        // Act
        var lootTable = new LootTable("goblin", entries);

        // Assert
        Assert.Equal(1, lootTable.GuaranteedDropCount);
        Assert.Equal(5, lootTable.MaximumDropCount);
    }

    [Theory]
    [InlineData("", "Monster type ID cannot be null or empty")]
    [InlineData(null, "Monster type ID cannot be null or empty")]
    [InlineData("   ", "Monster type ID cannot be null or empty")]
    public void LootTable_WithInvalidMonsterTypeId_ThrowsException(string? invalidId, string expectedMessage)
    {
        // Arrange
        var entries = new List<LootEntry> { new(_ironOre, 0.5f, 1, 1) };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LootTable(invalidId!, entries));
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public void LootTable_WithEmptyDropList_ThrowsException()
    {
        // Arrange
        var entries = new List<LootEntry>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LootTable("goblin", entries));
        Assert.Contains("Loot table must have at least one possible drop", exception.Message);
    }

    [Fact]
    public void LootTable_WithNegativeGuaranteedDropCount_ThrowsException()
    {
        // Arrange
        var entries = new List<LootEntry> { new(_ironOre, 0.5f, 1, 1) };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LootTable("goblin", entries, -1, 5));
        Assert.Contains("Guaranteed drop count cannot be negative", exception.Message);
    }

    [Fact]
    public void LootTable_WithMaxLessThanGuaranteed_ThrowsException()
    {
        // Arrange
        var entries = new List<LootEntry> { new(_ironOre, 0.5f, 1, 1) };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LootTable("goblin", entries, 5, 3));
        Assert.Contains("Maximum drop count cannot be less than guaranteed drop count", exception.Message);
    }

    [Fact]
    public void LootTable_WithInvalidLootEntry_ThrowsException()
    {
        // Arrange - Create an invalid loot entry (invalid drop chance)
        var entries = new List<LootEntry> { new(_ironOre, 1.5f, 1, 1) }; // Invalid drop chance > 1.0

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LootTable("goblin", entries));
        Assert.Contains("Drop chance must be between 0.0 and 1.0", exception.Message);
    }

    [Fact]
    public void LootTable_GetDropsByProbability_ReturnsSortedByDropChance()
    {
        // Arrange
        var entries = new List<LootEntry>
        {
            new(_ironOre, 0.5f, 1, 3),    // Medium chance
            new(_leather, 0.9f, 1, 2),    // High chance  
            new(_gem, 0.1f, 1, 1)         // Low chance
        };
        var lootTable = new LootTable("goblin", entries);

        // Act
        var sortedDrops = lootTable.GetDropsByProbability();

        // Assert
        Assert.Equal(3, sortedDrops.Count);
        Assert.Equal(0.9f, sortedDrops[0].DropChance); // Leather (highest)
        Assert.Equal(0.5f, sortedDrops[1].DropChance); // Iron Ore (medium)
        Assert.Equal(0.1f, sortedDrops[2].DropChance); // Gem (lowest)
    }

    [Fact]
    public void LootTable_GetDropsByProbability_ExcludesZeroChance()
    {
        // Arrange
        var entries = new List<LootEntry>
        {
            new(_ironOre, 0.5f, 1, 3),
            new(_leather, 0.0f, 1, 2),    // Zero chance - should be excluded
            new(_gem, 0.1f, 1, 1)
        };
        var lootTable = new LootTable("goblin", entries);

        // Act
        var validDrops = lootTable.GetDropsByProbability();

        // Assert
        Assert.Equal(2, validDrops.Count);
        Assert.All(validDrops, drop => Assert.True(drop.DropChance > 0.0f));
    }

    [Fact]
    public void LootTable_GetDropsByCategory_ReturnsCorrectMaterials()
    {
        // Arrange
        var entries = new List<LootEntry>
        {
            new(_ironOre, 0.8f, 1, 3),     // Metals
            new(_leather, 0.6f, 1, 2),     // Organic
            new(_gem, 0.1f, 1, 1)          // Gems
        };
        var lootTable = new LootTable("goblin", entries);

        // Act
        var metalDrops = lootTable.GetDropsByCategory(MaterialCategory.Metals);
        var organicDrops = lootTable.GetDropsByCategory(MaterialCategory.Organic);
        var gemDrops = lootTable.GetDropsByCategory(MaterialCategory.Gems);

        // Assert
        Assert.Single(metalDrops);
        Assert.Equal(_ironOre.Id, metalDrops[0].Material.Id);
        
        Assert.Single(organicDrops);
        Assert.Equal(_leather.Id, organicDrops[0].Material.Id);
        
        Assert.Single(gemDrops);
        Assert.Equal(_gem.Id, gemDrops[0].Material.Id);
    }

    [Fact]
    public void LootTable_GetDropsByRarity_ReturnsCorrectMaterials()
    {
        // Arrange
        var entries = new List<LootEntry>
        {
            new(_ironOre, 0.8f, 1, 3),                                    // Common (base rarity)
            new(_leather, 0.6f, 1, 2, MaterialRarity.Uncommon),          // Uncommon (forced)
            new(_gem, 0.1f, 1, 1)                                        // Rare (base rarity)
        };
        var lootTable = new LootTable("goblin", entries);

        // Act
        var commonDrops = lootTable.GetDropsByRarity(MaterialRarity.Common);
        var uncommonDrops = lootTable.GetDropsByRarity(MaterialRarity.Uncommon);
        var rareDrops = lootTable.GetDropsByRarity(MaterialRarity.Rare);

        // Assert
        Assert.Single(commonDrops);
        Assert.Equal(_ironOre.Id, commonDrops[0].Material.Id);
        
        Assert.Single(uncommonDrops);
        Assert.Equal(_leather.Id, uncommonDrops[0].Material.Id);
        
        Assert.Single(rareDrops);
        Assert.Equal(_gem.Id, rareDrops[0].Material.Id);
    }

    [Fact]
    public void LootTable_ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var entries = new List<LootEntry>
        {
            new(_ironOre, 0.8f, 1, 3),
            new(_leather, 0.6f, 1, 2)
        };
        var lootTable = new LootTable("goblin", entries, 1, 4);

        // Act
        var result = lootTable.ToString();

        // Assert
        Assert.Equal("LootTable for goblin: 2 possible drops, 1-4 guaranteed", result);
    }
}
