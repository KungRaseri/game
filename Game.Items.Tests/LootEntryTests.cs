#nullable enable

using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Systems;

namespace Game.Items.Tests;

public class LootEntryTests
{
    private readonly Material _testMaterial = new(
        "iron_ore",
        "Iron Ore",
        "Common metal ore",
        QualityTier.Common,
        2,
        Category.Metal,
        true,
        999
    );

    [Fact]
    public void LootEntry_ValidConfiguration_CreatesSuccessfully()
    {
        // Arrange & Act
        var entry = new LootEntry(_testMaterial, 0.8f, 1, 3, QualityTier.Uncommon);

        // Assert
        Assert.Equal(_testMaterial, entry.Material);
        Assert.Equal(0.8f, entry.DropChance);
        Assert.Equal(1, entry.MinQuantity);
        Assert.Equal(3, entry.MaxQuantity);
        Assert.Equal(QualityTier.Common, entry.Material.Quality); // Original material quality unchanged
        Assert.Equal(QualityTier.Uncommon, entry.GetEffectiveRarity()); // But effective quality is forced
    }

    [Fact]
    public void LootEntry_WithoutForceRarity_UsesNull()
    {
        // Arrange & Act
        var entry = new LootEntry(_testMaterial, 0.5f, 1, 2);

        // Assert - ForceQuality should not be set
        Assert.False(entry.ForceQuality.HasValue);
        // And GetEffectiveRarity should return the material's base quality
        Assert.Equal(QualityTier.Common, entry.GetEffectiveRarity());
    }

    [Fact]
    public void LootEntry_Validate_WithValidData_DoesNotThrow()
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, 0.5f, 1, 3);

        // Act & Assert (should not throw)
        entry.Validate();
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(2.0f)]
    public void LootEntry_Validate_WithInvalidDropChance_ThrowsException(float invalidDropChance)
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, invalidDropChance, 1, 3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => entry.Validate());
        Assert.Contains("Drop chance must be between 0.0 and 1.0", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void LootEntry_Validate_WithInvalidMinQuantity_ThrowsException(int invalidMinQuantity)
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, 0.5f, invalidMinQuantity, 3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => entry.Validate());
        Assert.Contains("Minimum quantity must be greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(5, 3)] // Max < Min
    [InlineData(10, 5)] // Max < Min
    public void LootEntry_Validate_WithMaxLessThanMin_ThrowsException(int minQuantity, int maxQuantity)
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, 0.5f, minQuantity, maxQuantity);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => entry.Validate());
        Assert.Contains("Maximum quantity cannot be less than minimum quantity", exception.Message);
    }

    [Fact]
    public void LootEntry_GetEffectiveRarity_WithForceRarity_ReturnsForceRarity()
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, 0.5f, 1, 3, QualityTier.Epic);

        // Act
        var effectiveRarity = entry.GetEffectiveRarity();

        // Assert
        Assert.Equal(QualityTier.Epic, effectiveRarity);
    }

    [Fact]
    public void LootEntry_GetEffectiveRarity_WithoutForceRarity_ReturnsBaseRarity()
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, 0.5f, 1, 3);

        // Act
        var effectiveRarity = entry.GetEffectiveRarity();

        // Assert
        Assert.Equal(QualityTier.Common, effectiveRarity); // Material's base rarity
    }

    [Theory]
    [InlineData(1, 1, "Iron Ore: 50% chance, 1 quantity (Common)")]
    [InlineData(1, 3, "Iron Ore: 50% chance, 1-3 quantity (Common)")]
    [InlineData(2, 2, "Iron Ore: 50% chance, 2 quantity (Common)")]
    public void LootEntry_ToString_ReturnsCorrectFormat(int minQuantity, int maxQuantity, string expected)
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, 0.5f, minQuantity, maxQuantity);

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void LootEntry_ToString_WithForceRarity_ShowsForceRarity()
    {
        // Arrange
        var entry = new LootEntry(_testMaterial, 0.8f, 1, 3, QualityTier.Epic);

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Equal("Iron Ore: 80% chance, 1-3 quantity (Epic)", result);
    }
}