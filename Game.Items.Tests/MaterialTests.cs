using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Tests;

public class MaterialTests
{
    [Fact]
    public void Constructor_SetsValues_Correctly()
    {
        // Arrange & Act
        var material = new Material(
            itemId: "material_001",
            name: "Iron Ore",
            description: "Raw iron ore",
            quality: QualityTier.Common,
            value: 5,
            Category.Metal,
            stackable: true,
            maxStackSize: 99
        );

        // Assert
        Assert.Equal("material_001", material.ItemId);
        Assert.Equal("Iron Ore", material.Name);
        Assert.Equal("Raw iron ore", material.Description);
        Assert.Equal(ItemType.Material, material.ItemType);
        Assert.Equal(QualityTier.Common, material.Quality);
        Assert.Equal(5, material.Value);
        Assert.Equal(Category.Metal, material.Category);
        Assert.True(material.Stackable);
        Assert.Equal(99, material.MaxStackSize);
    }

    [Fact]
    public void Constructor_DefaultsStackable_ToTrue()
    {
        // Arrange & Act
        var material = new Material(
            itemId: "material_001",
            name: "Test Material",
            description: "Test",
            quality: QualityTier.Common,
            value: 10,
            Category.Wood
        );

        // Assert
        Assert.True(material.Stackable);
    }

    [Fact]
    public void Constructor_DefaultsMaxStackSize_To99()
    {
        // Arrange & Act
        var material = new Material(
            itemId: "material_001",
            name: "Test Material",
            description: "Test",
            quality: QualityTier.Common,
            value: 10,
            Category.Wood
        );

        // Assert
        Assert.Equal(99, material.MaxStackSize);
    }

    [Fact]
    public void Constructor_ClampsNegativeMaxStackSize_ToOne()
    {
        // Arrange & Act
        var material = new Material(
            itemId: "material_001",
            name: "Test",
            description: "Test",
            quality: QualityTier.Common,
            value: 10,
            Category.Metal,
            stackable: true,
            maxStackSize: -10
        );

        // Assert
        Assert.Equal(1, material.MaxStackSize);
    }

    [Fact]
    public void Constructor_ClampsZeroMaxStackSize_ToOne()
    {
        // Arrange & Act
        var material = new Material(
            itemId: "material_001",
            name: "Test",
            description: "Test",
            quality: QualityTier.Common,
            value: 10,
            Category.Metal,
            stackable: true,
            maxStackSize: 0
        );

        // Assert
        Assert.Equal(1, material.MaxStackSize);
    }

    [Fact]
    public void MaxStackSize_Setter_ClampsNegativeValue_ToOne()
    {
        // Arrange
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 10, Category.Wood);

        // Act
        material.MaxStackSize = -5;

        // Assert
        Assert.Equal(1, material.MaxStackSize);
    }

    [Fact]
    public void MaxStackSize_Setter_ClampsZero_ToOne()
    {
        // Arrange
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 10, Category.Wood);

        // Act
        material.MaxStackSize = 0;

        // Assert
        Assert.Equal(1, material.MaxStackSize);
    }

    [Fact]
    public void MaxStackSize_Setter_AcceptsPositiveValue()
    {
        // Arrange
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 10, Category.Wood);

        // Act
        material.MaxStackSize = 50;

        // Assert
        Assert.Equal(50, material.MaxStackSize);
    }

    [Fact]
    public void ToString_ReturnsFormattedString_WithCategory()
    {
        // Arrange
        var material = new Material(
            "mat_001",
            "Ruby",
            "Test",
            QualityTier.Legendary,
            1000,
            Category.Gem,
            true,
            20
        );

        // Act
        var result = material.ToString();

        // Assert
        Assert.Equal("Ruby (Legendary Gem) - 1000g [Stack: 20]", result);
    }

    [Fact]
    public void Material_InheritsFrom_Item()
    {
        // Arrange
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 5, Category.Metal);

        // Act & Assert
        Assert.IsAssignableFrom<Item>(material);
    }

    [Theory]
    [InlineData(Category.Metal)]
    [InlineData(Category.Wood)]
    [InlineData(Category.Leather)]
    [InlineData(Category.Cloth)]
    [InlineData(Category.Gem)]
    [InlineData(Category.Herb)]
    [InlineData(Category.Bone)]
    [InlineData(Category.Essence)]
    public void Constructor_AcceptsAllCategories(Category category)
    {
        // Arrange & Act
        var material = new Material(
            "mat_001",
            "Test Material",
            "Test",
            QualityTier.Common,
            10,
            category
        );

        // Assert
        Assert.Equal(category, material.Category);
    }
}