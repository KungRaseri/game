#nullable enable

using Game.Main.Models.Materials;
using Xunit;

namespace Game.Main.Tests.Models.Materials;

public class MaterialDropTests
{
    private readonly MaterialType _testMaterial = new(
        "iron_ore",
        "Iron Ore",
        "Common metal ore",
        MaterialCategory.Metals,
        MaterialRarity.Common,
        BaseValue: 2
    );

    [Fact]
    public void MaterialDrop_ValidConfiguration_CreatesSuccessfully()
    {
        // Arrange
        var acquiredTime = DateTime.UtcNow;

        // Act
        var drop = new MaterialDrop(_testMaterial, MaterialRarity.Uncommon, 3, acquiredTime);

        // Assert
        Assert.Equal(_testMaterial, drop.Material);
        Assert.Equal(MaterialRarity.Uncommon, drop.ActualRarity);
        Assert.Equal(3, drop.Quantity);
        Assert.Equal(acquiredTime, drop.AcquiredAt);
    }

    [Fact]
    public void MaterialDrop_Validate_WithValidData_DoesNotThrow()
    {
        // Arrange
        var drop = new MaterialDrop(_testMaterial, MaterialRarity.Common, 1, DateTime.UtcNow);

        // Act & Assert (should not throw)
        drop.Validate();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void MaterialDrop_Validate_WithInvalidQuantity_ThrowsException(int invalidQuantity)
    {
        // Arrange
        var drop = new MaterialDrop(_testMaterial, MaterialRarity.Common, invalidQuantity, DateTime.UtcNow);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => drop.Validate());
        Assert.Contains("Quantity must be greater than zero", exception.Message);
    }

    [Fact]
    public void MaterialDrop_Validate_WithFutureDate_ThrowsException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var drop = new MaterialDrop(_testMaterial, MaterialRarity.Common, 1, futureDate);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => drop.Validate());
        Assert.Contains("Acquired date cannot be in the future", exception.Message);
    }

    [Theory]
    [InlineData(MaterialRarity.Common, 1, 2, 2)]      // Base value 2 * quantity 1 * multiplier 1.0 = 2
    [InlineData(MaterialRarity.Uncommon, 2, 2, 8)]    // Base value 2 * quantity 2 * multiplier 2.0 = 8
    [InlineData(MaterialRarity.Rare, 1, 2, 10)]       // Base value 2 * quantity 1 * multiplier 5.0 = 10
    [InlineData(MaterialRarity.Epic, 1, 2, 30)]       // Base value 2 * quantity 1 * multiplier 15.0 = 30
    [InlineData(MaterialRarity.Legendary, 1, 2, 100)] // Base value 2 * quantity 1 * multiplier 50.0 = 100
    public void MaterialDrop_GetTotalValue_CalculatesCorrectValue(MaterialRarity rarity, int quantity, int baseValue, int expectedValue)
    {
        // Arrange
        var material = new MaterialType("test", "Test", "Test", MaterialCategory.Metals, MaterialRarity.Common, BaseValue: baseValue);
        var drop = new MaterialDrop(material, rarity, quantity, DateTime.UtcNow);

        // Act
        var totalValue = drop.GetTotalValue();

        // Assert
        Assert.Equal(expectedValue, totalValue);
    }

    [Theory]
    [InlineData(MaterialRarity.Common, "#808080")]
    [InlineData(MaterialRarity.Uncommon, "#00FF00")]
    [InlineData(MaterialRarity.Rare, "#0080FF")]
    [InlineData(MaterialRarity.Epic, "#8000FF")]
    [InlineData(MaterialRarity.Legendary, "#FFD700")]
    public void MaterialDrop_GetRarityColor_ReturnsCorrectColor(MaterialRarity rarity, string expectedColor)
    {
        // Arrange
        var drop = new MaterialDrop(_testMaterial, rarity, 1, DateTime.UtcNow);

        // Act
        var color = drop.GetRarityColor();

        // Assert
        Assert.Equal(expectedColor, color);
    }

    [Theory]
    [InlineData(MaterialRarity.Common, 1, "Iron Ore (Common) x1")]
    [InlineData(MaterialRarity.Uncommon, 3, "Iron Ore (Uncommon) x3")]
    [InlineData(MaterialRarity.Legendary, 10, "Iron Ore (Legendary) x10")]
    public void MaterialDrop_ToString_ReturnsCorrectFormat(MaterialRarity rarity, int quantity, string expected)
    {
        // Arrange
        var drop = new MaterialDrop(_testMaterial, rarity, quantity, DateTime.UtcNow);

        // Act
        var result = drop.ToString();

        // Assert
        Assert.Equal(expected, result);
    }
}
