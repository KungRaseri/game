#nullable enable

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
            Category.Metal,
            MaterialRarity.Common,
            StackLimit: 999,
            BaseValue: 2
        );

        // Assert
        Assert.Equal("iron_ore", material.Id);
        Assert.Equal("Iron Ore", material.Name);
        Assert.Equal("Common metal ore", material.Description);
        Assert.Equal(MaterialCategory.Metals, material.Category);
        Assert.Equal(MaterialRarity.Common, material.BaseRarity);
        Assert.Equal(999, material.StackLimit);
        Assert.Equal(2, material.BaseValue);
    }

    [Fact]
    public void Category_Validate_WithValidData_DoesNotThrow()
    {
        // Arrange
        var material = new Category(
            "test_material",
            "Test Material",
            "A test material",
            MaterialCategory.Metals,
            MaterialRarity.Common
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
        var material = new Category(
            invalidId!,
            "Valid Name",
            "Valid Description",
            MaterialCategory.Metals,
            MaterialRarity.Common
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
        var material = new Category(
            "valid_id",
            invalidName!,
            "Valid Description",
            MaterialCategory.Metals,
            MaterialRarity.Common
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
        var material = new Category(
            "valid_id",
            "Valid Name",
            "Valid Description",
            MaterialCategory.Metals,
            MaterialRarity.Common,
            StackLimit: invalidStackLimit
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
        var material = new Category(
            "valid_id",
            "Valid Name",
            "Valid Description",
            MaterialCategory.Metals,
            MaterialRarity.Common,
            BaseValue: invalidBaseValue
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => material.Validate());
        Assert.Contains("Base value cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(MaterialRarity.Common, "#808080")]
    [InlineData(MaterialRarity.Uncommon, "#00FF00")]
    [InlineData(MaterialRarity.Rare, "#0080FF")]
    [InlineData(MaterialRarity.Epic, "#8000FF")]
    [InlineData(MaterialRarity.Legendary, "#FFD700")]
    public void Category_GetRarityColor_ReturnsCorrectColor(MaterialRarity rarity, string expectedColor)
    {
        // Arrange
        var material = new Category(
            "test_material",
            "Test Material",
            "A test material",
            MaterialCategory.Metals,
            rarity
        );

        // Act
        var color = material.GetRarityColor();

        // Assert
        Assert.Equal(expectedColor, color);
    }
}
