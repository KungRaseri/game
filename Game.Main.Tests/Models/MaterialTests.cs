using Game.Item.Models;
using Godot;

namespace Game.Main.Tests.Models;

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
            materialType: MaterialType.Metal,
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
        Assert.Equal(MaterialType.Metal, material.MaterialType);
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
            materialType: MaterialType.Wood
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
            materialType: MaterialType.Wood
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
            materialType: MaterialType.Metal,
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
            materialType: MaterialType.Metal,
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
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 10, MaterialType.Wood);

        // Act
        material.MaxStackSize = -5;

        // Assert
        Assert.Equal(1, material.MaxStackSize);
    }

    [Fact]
    public void MaxStackSize_Setter_ClampsZero_ToOne()
    {
        // Arrange
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 10, MaterialType.Wood);

        // Act
        material.MaxStackSize = 0;

        // Assert
        Assert.Equal(1, material.MaxStackSize);
    }

    [Fact]
    public void MaxStackSize_Setter_AcceptsPositiveValue()
    {
        // Arrange
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 10, MaterialType.Wood);

        // Act
        material.MaxStackSize = 50;

        // Assert
        Assert.Equal(50, material.MaxStackSize);
    }

    [Fact]
    public void ToString_ReturnsFormattedString_WithMaterialType()
    {
        // Arrange
        var material = new Material(
            "mat_001",
            "Ruby",
            "Test",
            QualityTier.Legendary,
            1000,
            MaterialType.Gem,
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
        var material = new Material("mat_001", "Test", "Test", QualityTier.Common, 5, MaterialType.Metal);

        // Act & Assert
        Assert.IsAssignableFrom<Items>(material);
    }

    [Theory]
    [InlineData(MaterialType.Metal)]
    [InlineData(MaterialType.Wood)]
    [InlineData(MaterialType.Leather)]
    [InlineData(MaterialType.Cloth)]
    [InlineData(MaterialType.Gem)]
    [InlineData(MaterialType.Herb)]
    [InlineData(MaterialType.Bone)]
    [InlineData(MaterialType.Essence)]
    public void Constructor_AcceptsAllMaterialTypes(MaterialType materialType)
    {
        // Arrange & Act
        var material = new Material(
            "mat_001",
            "Test Material",
            "Test",
            QualityTier.Common,
            10,
            materialType
        );

        // Assert
        Assert.Equal(materialType, material.MaterialType);
    }
}
