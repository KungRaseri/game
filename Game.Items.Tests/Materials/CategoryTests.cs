#nullable enable

using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Main.Tests.Models.Materials;

public class CategoryTests
{
    [Fact]
    public void Category_ValidConfiguration_CreatesSuccessfully()
    {
        // Arrange & Act
        var material = new Material(
            "iron_ore",
            "Iron Ore", 
            "Common metal ore",
            QualityTier.Common,
            2,
            Category.Metal,
            true,
            999
        );

        // Assert
        Assert.Equal("iron_ore", material.ItemId);
        Assert.Equal("Iron Ore", material.Name);
        Assert.Equal("Common metal ore", material.Description);
        Assert.Equal(Category.Metal, material.Category);
        Assert.Equal(QualityTier.Common, material.Quality);
        Assert.Equal(999, material.MaxStackSize);
        Assert.Equal(2, material.BaseValue);
    }

    [Fact]
    public void Category_Validate_WithValidData_DoesNotThrow()
    {
        // Arrange
        var material = new Material(
            "test_material",
            "Test Material",
            "A test material",
            QualityTier.Common,
            1,
            Category.Metal
        );

        // Act & Assert (should not throw)
        material.Validate();
    }

    [Theory]
    [InlineData("", "Material ID cannot be null or empty")]
    [InlineData(null, "Material ID cannot be null or empty")]
    [InlineData("   ", "Material ID cannot be null or empty")]
    public void Category_Validate_WithInvalidId_ThrowsException(string? invalidId, string expectedMessage)
    {
        // Arrange
        var material = new Material(
            invalidId!,
            "Valid Name",
            "Valid Description",
            QualityTier.Common,
            1,
            Category.Metal
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => material.Validate());
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("", "Material Name cannot be null or empty")]
    [InlineData(null, "Material Name cannot be null or empty")]
    [InlineData("   ", "Material Name cannot be null or empty")]
    public void Category_Validate_WithInvalidName_ThrowsException(string? invalidName, string expectedMessage)
    {
        // Arrange
        var material = new Material(
            "valid_id",
            invalidName!,
            "Valid Description",
            QualityTier.Common,
            1,
            Category.Metal
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => material.Validate());
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Category_Validate_WithInvalidStackLimit_ThrowsException(int invalidStackLimit)
    {
        // Arrange
        var material = new Material(
            "valid_id",
            "Valid Name",
            "Valid Description",
            QualityTier.Common,
            1,
            Category.Metal,
            true,
            invalidStackLimit
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => material.Validate());
        Assert.Contains("Stack limit must be greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Category_Validate_WithNegativeBaseValue_ThrowsException(int invalidBaseValue)
    {
        // Arrange
        var material = new Material(
            "valid_id",
            "Valid Name",
            "Valid Description",
            QualityTier.Common,
            invalidBaseValue,
            Category.Metal
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => material.Validate());
        Assert.Contains("Base value cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(QualityTier.Common, "#808080")]
    [InlineData(QualityTier.Uncommon, "#00FF00")]
    [InlineData(QualityTier.Rare, "#0080FF")]
    [InlineData(QualityTier.Epic, "#8000FF")]
    [InlineData(QualityTier.Legendary, "#FFD700")]
    public void Category_GetRarityColor_ReturnsCorrectColor(QualityTier rarity, string expectedColor)
    {
        // Arrange
        var material = new Material(
            "test_material",
            "Test Material",
            "A test material",
            rarity,
            1,
            Category.Metal
        );

        // Act
        var color = material.GetRarityColor();

        // Assert
        Assert.Equal(expectedColor, color);
    }
}
