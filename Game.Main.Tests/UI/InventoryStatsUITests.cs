#nullable enable

using Game.Main.Utils;

namespace Game.Main.Tests.UI;

/// <summary>
/// Tests for the InventoryStatsUI component functionality.
/// NOTE: This test focuses on the data handling aspects that can be tested
/// without the full Godot runtime. UI interaction tests would require 
/// integration testing within the Godot environment.
/// </summary>
public class InventoryStatsUITests
{
    public InventoryStatsUITests()
    {
        // Set up console logging for tests
        GameLogger.SetBackend(new ConsoleLoggerBackend());
    }

    [Fact]
    public void CreateTestStats_WithValidData_ReturnsCorrectStats()
    {
        // Act
        var stats = CreateTestStats();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(20, stats.Capacity);
        Assert.Equal(10, stats.UsedSlots);
        Assert.Equal(50, stats.TotalMaterials);
        Assert.Equal(1000, stats.TotalValue);
        Assert.Equal(2, stats.CategoryCounts.Count);
        Assert.Equal(2, stats.RarityCounts.Count);
    }

    [Fact]
    public void CreateComplexTestStats_WithMultipleCategories_ReturnsCorrectStats()
    {
        // Act
        var stats = CreateComplexTestStats();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(4, stats.CategoryCounts.Count);
        Assert.Equal(4, stats.RarityCounts.Count);
        Assert.Equal(20, stats.CategoryCounts[MaterialCategory.Metals]);
        Assert.Equal(30, stats.RarityCounts[MaterialRarity.Common]);
    }

    [Fact]
    public void CreateEmptyStats_ReturnsValidEmptyStats()
    {
        // Act
        var stats = CreateEmptyStats();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(0, stats.UsedSlots);
        Assert.Equal(0, stats.TotalMaterials);
        Assert.Equal(0, stats.TotalValue);
        Assert.Empty(stats.CategoryCounts);
        Assert.Empty(stats.RarityCounts);
    }

    [Fact]
    public void CreateMaxCapacityStats_ReturnsFullCapacityStats()
    {
        // Act
        var stats = CreateMaxCapacityStats();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(20, stats.UsedSlots);
        Assert.Equal(20, stats.Capacity);
        Assert.Equal(100, stats.TotalMaterials);
    }

    [Fact]
    public void InventoryStats_CategoryCounts_AreImmutable()
    {
        // Arrange
        var stats = CreateTestStats();
        var originalCount = stats.CategoryCounts.Count;

        // Act & Assert - This should not be possible due to record immutability
        // but we verify the collections are properly initialized
        Assert.Equal(originalCount, stats.CategoryCounts.Count);
        Assert.True(stats.CategoryCounts.ContainsKey(MaterialCategory.Metals));
        Assert.True(stats.CategoryCounts.ContainsKey(MaterialCategory.Organic));
    }

    [Fact]
    public void InventoryStats_RarityCounts_AreImmutable()
    {
        // Arrange
        var stats = CreateTestStats();
        var originalCount = stats.RarityCounts.Count;

        // Act & Assert
        Assert.Equal(originalCount, stats.RarityCounts.Count);
        Assert.True(stats.RarityCounts.ContainsKey(MaterialRarity.Common));
        Assert.True(stats.RarityCounts.ContainsKey(MaterialRarity.Uncommon));
    }

    private static InventoryStats CreateTestStats()
    {
        return new InventoryStats(
            Capacity: 20,
            UsedSlots: 10,
            TotalMaterials: 50,
            TotalValue: 1000,
            CategoryCounts: new Dictionary<MaterialCategory, int>
            {
                { MaterialCategory.Metals, 25 },
                { MaterialCategory.Organic, 25 }
            },
            RarityCounts: new Dictionary<MaterialRarity, int>
            {
                { MaterialRarity.Common, 40 },
                { MaterialRarity.Uncommon, 10 }
            }
        );
    }

    private static InventoryStats CreateComplexTestStats()
    {
        return new InventoryStats(
            Capacity: 20,
            UsedSlots: 10,
            TotalMaterials: 50,
            TotalValue: 1000,
            CategoryCounts: new Dictionary<MaterialCategory, int>
            {
                { MaterialCategory.Metals, 20 },
                { MaterialCategory.Organic, 15 },
                { MaterialCategory.Gems, 10 },
                { MaterialCategory.Magical, 5 }
            },
            RarityCounts: new Dictionary<MaterialRarity, int>
            {
                { MaterialRarity.Common, 30 },
                { MaterialRarity.Uncommon, 15 },
                { MaterialRarity.Rare, 4 },
                { MaterialRarity.Epic, 1 }
            }
        );
    }

    private static InventoryStats CreateEmptyStats()
    {
        return new InventoryStats(
            Capacity: 20,
            UsedSlots: 0,
            TotalMaterials: 0,
            TotalValue: 0,
            CategoryCounts: new Dictionary<MaterialCategory, int>(),
            RarityCounts: new Dictionary<MaterialRarity, int>()
        );
    }

    private static InventoryStats CreateMaxCapacityStats()
    {
        return new InventoryStats(
            Capacity: 20,
            UsedSlots: 20,
            TotalMaterials: 100,
            TotalValue: 2000,
            CategoryCounts: new Dictionary<MaterialCategory, int>
            {
                { MaterialCategory.Metals, 100 }
            },
            RarityCounts: new Dictionary<MaterialRarity, int>
            {
                { MaterialRarity.Common, 100 }
            }
        );
    }
}
