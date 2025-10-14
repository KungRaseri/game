#nullable enable

using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Tests.Materials;

public class DropTests
{
    private readonly Material _testMaterial = new(
        "ore_iron",
        "Iron Ore",
        "Common metal ore",
        QualityTier.Common,
        2,
        Category.Metal
    );

    [Fact]
    public void Drop_ValidConfiguration_CreatesSuccessfully()
    {
        // Arrange
        var acquiredTime = DateTime.UtcNow;

        // Act
        var drop = new Drop(_testMaterial, 3, acquiredTime);

        // Assert
        Assert.Equal(_testMaterial, drop.Material);
        Assert.Equal(QualityTier.Common, drop.Material.Quality);
        Assert.Equal(3, drop.Quantity);
        Assert.Equal(acquiredTime, drop.AcquiredAt);
    }

    [Fact]
    public void Drop_Validate_WithValidData_DoesNotThrow()
    {
        // Arrange
        var drop = new Drop(_testMaterial, 1, DateTime.UtcNow);

        // Act & Assert (should not throw)
        drop.Validate();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Drop_Validate_WithInvalidQuantity_ThrowsException(int invalidQuantity)
    {
        // Arrange
        var drop = new Drop(_testMaterial, invalidQuantity, DateTime.UtcNow);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => drop.Validate());
        Assert.Contains("Quantity must be greater than zero", exception.Message);
    }

    [Fact]
    public void Drop_Validate_WithFutureDate_ThrowsException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var drop = new Drop(_testMaterial, 1, futureDate);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => drop.Validate());
        Assert.Contains("Acquired date cannot be in the future", exception.Message);
    }

    [Theory]
    [InlineData(QualityTier.Common, 1, 2, 2)] // Base value 2 * quantity 1 * multiplier 1.0 = 2
    [InlineData(QualityTier.Uncommon, 2, 4, 8)] // Base value 2 * quantity 2 * multiplier 2.0 = 8 (final value = 2*2 = 4)
    [InlineData(QualityTier.Rare, 1, 8, 10)] // Base value 2 * quantity 1 * multiplier 5.0 = 10 (final value = 2*4 = 8)
    [InlineData(QualityTier.Epic, 1, 16, 30)] // Base value 2 * quantity 1 * multiplier 15.0 = 30 (final value = 2*8 = 16)
    [InlineData(QualityTier.Legendary, 1, 32, 100)] // Base value 2 * quantity 1 * multiplier 50.0 = 100 (final value = 2*16 = 32)
    public void Drop_GetTotalValue_CalculatesCorrectValue(QualityTier rarity, int quantity, int finalValue,
        int expectedValue)
    {
        // Arrange
        var material = new Material("test", "Test", "Test", rarity, finalValue, Category.Metal);
        var drop = new Drop(material, quantity, DateTime.UtcNow);

        // Act
        var totalValue = drop.GetTotalValue();

        // Assert
        Assert.Equal(expectedValue, totalValue);
    }

    [Theory]
    [InlineData(QualityTier.Common, "#808080")]
    [InlineData(QualityTier.Uncommon, "#00FF00")]
    [InlineData(QualityTier.Rare, "#0080FF")]
    [InlineData(QualityTier.Epic, "#8000FF")]
    [InlineData(QualityTier.Legendary, "#FFD700")]
    public void Drop_GetRarityColor_ReturnsCorrectColor(QualityTier rarity, string expectedColor)
    {
        // Arrange
        _testMaterial.Quality = rarity;
        var drop = new Drop(_testMaterial, 1, DateTime.UtcNow);

        // Act
        var color = drop.GetRarityColor();

        // Assert
        Assert.Equal(expectedColor, color);
    }

    [Theory]
    [InlineData(1, "Iron Ore (Common) x1")]
    [InlineData(3, "Iron Ore (Common) x3")]
    [InlineData(10, "Iron Ore (Common) x10")]
    public void Drop_ToString_ReturnsCorrectFormat(int quantity, string expected)
    {
        // Arrange
        var drop = new Drop(_testMaterial, quantity, DateTime.UtcNow);

        // Act
        var result = drop.ToString();

        // Assert
        Assert.Equal(expected, result);
    }
}