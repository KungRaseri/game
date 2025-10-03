#nullable enable

using Game.Core.Models.Materials;
using Game.Main.Systems.Loot;

namespace Game.Main.Tests.Systems.Loot;

public class LootGeneratorTests
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

    private Dictionary<string, LootTable> CreateTestLootTables()
    {
        var lootTables = new Dictionary<string, LootTable>();

        // Goblin with guaranteed drops
        lootTables["goblin"] = new LootTable(
            "goblin",
            new List<LootEntry>
            {
                new(_ironOre, 1.0f, 1, 2),     // Always drops 1-2 iron ore
                new(_leather, 0.5f, 1, 1),     // 50% chance for leather
                new(_gem, 0.1f, 1, 1)          // 10% chance for gem
            },
            guaranteedDropCount: 1,
            maximumDropCount: 3
        );

        // Test monster with no guaranteed drops
        lootTables["test_monster"] = new LootTable(
            "test_monster",
            new List<LootEntry>
            {
                new(_leather, 0.3f, 1, 1)
            },
            guaranteedDropCount: 0,
            maximumDropCount: 1
        );

        return lootTables;
    }

    [Fact]
    public void LootGenerator_ValidConfiguration_CreatesSuccessfully()
    {
        // Arrange
        var lootTables = CreateTestLootTables();
        var random = new Random(42); // Fixed seed for predictable tests

        // Act
        var generator = new LootGenerator(lootTables, random);

        // Assert
        Assert.NotNull(generator);
    }

    [Fact]
    public void LootGenerator_WithInvalidLootTable_ThrowsException()
    {
        // Arrange - This should throw during LootTable construction
        var invalidEntry = new LootEntry(_ironOre, 1.5f, 1, 1); // Invalid drop chance
        
        // Act & Assert - Exception thrown during LootTable creation
        Assert.Throws<ArgumentException>(() => new LootTable("invalid", new List<LootEntry> { invalidEntry }));
    }

    [Fact]
    public void LootGenerator_GenerateDrops_WithValidMonster_ReturnsDrops()
    {
        // Arrange
        var lootTables = CreateTestLootTables();
        var generator = new LootGenerator(lootTables, new Random(42));

        // Act
        var drops = generator.GenerateDrops("goblin");

        // Assert
        Assert.NotNull(drops);
        Assert.NotEmpty(drops); // Goblin has guaranteed drops
        Assert.All(drops, drop => 
        {
            Assert.True(drop.Quantity > 0);
            Assert.True(drop.AcquiredAt <= DateTime.UtcNow);
        });
    }

    [Fact]
    public void LootGenerator_GenerateDrops_WithInvalidMonster_ReturnsEmptyList()
    {
        // Arrange
        var lootTables = CreateTestLootTables();
        var generator = new LootGenerator(lootTables);

        // Act
        var drops = generator.GenerateDrops("nonexistent_monster");

        // Assert
        Assert.NotNull(drops);
        Assert.Empty(drops);
    }

    [Fact]
    public void LootGenerator_GenerateDrops_RespectsGuaranteedDropCount()
    {
        // Arrange
        var lootTables = new Dictionary<string, LootTable>
        {
            ["guaranteed_monster"] = new LootTable(
                "guaranteed_monster",
                new List<LootEntry>
                {
                    new(_ironOre, 0.01f, 1, 1),    // Very low chance
                    new(_leather, 0.01f, 1, 1),    // Very low chance
                    new(_gem, 0.01f, 1, 1)         // Very low chance
                },
                guaranteedDropCount: 2,
                maximumDropCount: 5
            )
        };
        var generator = new LootGenerator(lootTables, new Random(42));

        // Act
        var drops = generator.GenerateDrops("guaranteed_monster");

        // Assert
        Assert.True(drops.Count >= 2, "Should generate at least 2 drops due to guaranteed minimum");
    }

    [Fact]
    public void LootGenerator_GenerateDrops_RespectsMaximumDropCount()
    {
        // Arrange
        var lootTables = new Dictionary<string, LootTable>
        {
            ["max_drops_monster"] = new LootTable(
                "max_drops_monster",
                new List<LootEntry>
                {
                    new(_ironOre, 1.0f, 5, 10),    // Always drops 5-10 items
                    new(_leather, 1.0f, 5, 10),    // Always drops 5-10 items
                    new(_gem, 1.0f, 5, 10)         // Always drops 5-10 items
                },
                guaranteedDropCount: 0,
                maximumDropCount: 2
            )
        };
        var generator = new LootGenerator(lootTables);

        // Act
        var drops = generator.GenerateDrops("max_drops_monster");

        // Assert
        Assert.True(drops.Count <= 2, "Should not exceed maximum drop count of 2");
    }

    [Theory]
    [InlineData(42, 1000)]   // Different random seeds
    [InlineData(123, 1000)]
    [InlineData(999, 1000)]
    public void LootGenerator_GenerateDrops_ProbabilityDistribution_IsReasonable(int seed, int trials)
    {
        // Arrange - Create a monster with 50% drop chance
        var lootTables = new Dictionary<string, LootTable>
        {
            ["probability_test"] = new LootTable(
                "probability_test",
                new List<LootEntry>
                {
                    new(_ironOre, 0.5f, 1, 1)  // Exactly 50% chance
                },
                guaranteedDropCount: 0,
                maximumDropCount: 1
            )
        };
        var generator = new LootGenerator(lootTables, new Random(seed));

        // Act - Generate many drops and count successes
        int successCount = 0;
        for (int i = 0; i < trials; i++)
        {
            var drops = generator.GenerateDrops("probability_test");
            if (drops.Count > 0)
            {
                successCount++;
            }
        }

        // Assert - Should be reasonably close to 50% (within 10% tolerance for large sample)
        double actualRate = (double)successCount / trials;
        double expectedRate = 0.5;
        double tolerance = 0.1; // 10% tolerance

        Assert.True(Math.Abs(actualRate - expectedRate) < tolerance, 
            $"Drop rate {actualRate:P1} should be within {tolerance:P0} of expected {expectedRate:P0}");
    }

    [Fact]
    public void LootGenerator_GenerateDrops_QuantityWithinRange()
    {
        // Arrange
        var lootTables = new Dictionary<string, LootTable>
        {
            ["quantity_test"] = new LootTable(
                "quantity_test",
                new List<LootEntry>
                {
                    new(_ironOre, 1.0f, 3, 7)  // Always drops 3-7 items
                },
                guaranteedDropCount: 1,
                maximumDropCount: 1
            )
        };
        var generator = new LootGenerator(lootTables);

        // Act & Assert - Test multiple generations
        for (int i = 0; i < 100; i++)
        {
            var drops = generator.GenerateDrops("quantity_test");
            
            Assert.Single(drops);
            var drop = drops[0];
            Assert.True(drop.Quantity >= 3 && drop.Quantity <= 7, 
                $"Quantity {drop.Quantity} should be between 3 and 7");
        }
    }

    [Fact]
    public void LootGenerator_GetDropStatistics_ReturnsCorrectStats()
    {
        // Arrange
        var lootTables = CreateTestLootTables();
        var generator = new LootGenerator(lootTables);

        // Act
        var stats = generator.GetDropStatistics("goblin");

        // Assert
        Assert.Equal(3, stats.Count);
        Assert.Equal(1.0f, stats["Iron Ore"]);
        Assert.Equal(0.5f, stats["Leather"]);
        Assert.Equal(0.1f, stats["Gem"]);
    }

    [Fact]
    public void LootGenerator_GetDropStatistics_WithInvalidMonster_ReturnsEmpty()
    {
        // Arrange
        var lootTables = CreateTestLootTables();
        var generator = new LootGenerator(lootTables);

        // Act
        var stats = generator.GetDropStatistics("nonexistent_monster");

        // Assert
        Assert.Empty(stats);
    }

    [Theory]
    [InlineData("goblin", true)]
    [InlineData("test_monster", true)]
    [InlineData("nonexistent", false)]
    public void LootGenerator_HasLootTable_ReturnsCorrectValue(string monsterTypeId, bool expectedResult)
    {
        // Arrange
        var lootTables = CreateTestLootTables();
        var generator = new LootGenerator(lootTables);

        // Act
        var hasTable = generator.HasLootTable(monsterTypeId);

        // Assert
        Assert.Equal(expectedResult, hasTable);
    }

    [Fact]
    public void LootGenerator_GenerateDrops_SetsCorrectAcquisitionTime()
    {
        // Arrange
        var beforeGeneration = DateTime.UtcNow;
        var lootTables = CreateTestLootTables();
        var generator = new LootGenerator(lootTables);

        // Act
        var drops = generator.GenerateDrops("goblin");
        var afterGeneration = DateTime.UtcNow;

        // Assert
        Assert.All(drops, drop =>
        {
            Assert.True(drop.AcquiredAt >= beforeGeneration);
            Assert.True(drop.AcquiredAt <= afterGeneration);
        });
    }
}
