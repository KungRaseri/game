#nullable enable

using Xunit;
using System.Text.Json;
using Game.Core.Serialization;
using Game.Items.Data.Models;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Tests.Data;

/// <summary>
/// Integration tests for Material JSON serialization with enum converters.
/// </summary>
public class MaterialJsonSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public MaterialJsonSerializationTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new EnumJsonConverterFactory() }
        };
    }

    [Fact]
    public void MaterialJsonData_Deserialize_ParsesEnumsCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "test_material",
            "name": "Test Material",
            "description": "A test material",
            "baseValue": 10,
            "category": "Metal",
            "qualityTier": "Uncommon",
            "stackable": true,
            "maxStackSize": 99,
            "properties": {}
        }
        """;

        // Act
        var material = JsonSerializer.Deserialize<MaterialJsonData>(json, _options);

        // Assert
        Assert.NotNull(material);
        Assert.Equal(Category.Metal, material.Category);
        Assert.Equal(QualityTier.Uncommon, material.QualityTier);
    }

    [Fact]
    public void MaterialJsonData_Serialize_WritesEnumsAsStrings()
    {
        // Arrange
        var material = new MaterialJsonData
        {
            Id = "test_gem",
            Name = "Test Gem",
            Description = "A precious gemstone",
            BaseValue = 100,
            Category = Category.Gem,
            QualityTier = QualityTier.Epic,
            Stackable = true,
            MaxStackSize = 50
        };

        // Act
        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new EnumJsonConverterFactory() }
        };
        var json = JsonSerializer.Serialize(material, serializeOptions);

        // Assert
        Assert.Contains("\"category\": \"Gem\"", json);
        Assert.Contains("\"qualityTier\": \"Epic\"", json);
    }

    [Fact]
    public void MaterialJsonData_CaseInsensitive_ParsesCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "test",
            "name": "Test",
            "description": "Test",
            "baseValue": 5,
            "category": "WOOD",
            "qualityTier": "rare",
            "stackable": true,
            "maxStackSize": 99,
            "properties": {}
        }
        """;

        // Act
        var material = JsonSerializer.Deserialize<MaterialJsonData>(json, _options);

        // Assert
        Assert.NotNull(material);
        Assert.Equal(Category.Wood, material.Category);
        Assert.Equal(QualityTier.Rare, material.QualityTier);
    }

    [Fact]
    public void MaterialJsonData_InvalidCategory_ThrowsJsonException()
    {
        // Arrange
        var json = """
        {
            "id": "test",
            "name": "Test",
            "description": "Test",
            "baseValue": 5,
            "category": "Fabric",
            "qualityTier": "Common",
            "stackable": true,
            "maxStackSize": 99,
            "properties": {}
        }
        """;

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<MaterialJsonData>(json, _options)
        );

        Assert.Contains("Invalid", exception.Message);
        Assert.Contains("Category", exception.Message);
    }

    [Fact]
    public void MaterialJsonData_ToMaterialConfig_ConvertsCorrectly()
    {
        // Arrange
        var materialData = new MaterialJsonData
        {
            Id = "test_herb",
            Name = "Test Herb",
            Description = "A healing herb",
            BaseValue = 15,
            Category = Category.Herb,
            QualityTier = QualityTier.Common,
            Stackable = true,
            MaxStackSize = 99
        };

        // Act
        var config = materialData.ToMaterialConfig();

        // Assert
        Assert.Equal("test_herb", config.ItemId);
        Assert.Equal(Category.Herb, config.Category);
        Assert.True(config.Stackable);
    }

    [Theory]
    [InlineData("Metal")]
    [InlineData("Wood")]
    [InlineData("Leather")]
    [InlineData("Cloth")]
    [InlineData("Gem")]
    [InlineData("Herb")]
    [InlineData("Bone")]
    [InlineData("Magical")]
    [InlineData("Essence")]
    [InlineData("Fuel")]
    public void MaterialJsonData_AllCategories_DeserializeCorrectly(string categoryName)
    {
        // Arrange
        var json = $$"""
        {
            "id": "test",
            "name": "Test",
            "description": "Test",
            "baseValue": 5,
            "category": "{{categoryName}}",
            "qualityTier": "Common",
            "stackable": true,
            "maxStackSize": 99,
            "properties": {}
        }
        """;

        // Act
        var material = JsonSerializer.Deserialize<MaterialJsonData>(json, _options);

        // Assert
        Assert.NotNull(material);
        Assert.Equal(categoryName, material.Category.ToString());
    }
}
