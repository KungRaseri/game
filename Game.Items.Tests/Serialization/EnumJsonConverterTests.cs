#nullable enable

using Xunit;
using System.Text.Json;
using Game.Core.Serialization;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Tests.Serialization;

/// <summary>
/// Tests for EnumJsonConverter to ensure proper serialization/deserialization
/// and clear error messages for invalid values.
/// </summary>
public class EnumJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public EnumJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new EnumJsonConverter<Category>() }
        };
    }

    [Fact]
    public void Read_ValidEnumValue_ParsesCorrectly()
    {
        // Arrange
        var json = "\"Metal\"";

        // Act
        var result = JsonSerializer.Deserialize<Category>(json, _options);

        // Assert
        Assert.Equal(Category.Metal, result);
    }

    [Fact]
    public void Read_CaseInsensitiveValue_ParsesCorrectly()
    {
        // Arrange
        var json = "\"metal\""; // lowercase

        // Act
        var result = JsonSerializer.Deserialize<Category>(json, _options);

        // Assert
        Assert.Equal(Category.Metal, result);
    }

    [Theory]
    [InlineData("\"Metal\"", Category.Metal)]
    [InlineData("\"Wood\"", Category.Wood)]
    [InlineData("\"Leather\"", Category.Leather)]
    [InlineData("\"Cloth\"", Category.Cloth)]
    [InlineData("\"Gem\"", Category.Gem)]
    public void Read_VariousCategories_ParseCorrectly(string json, Category expected)
    {
        // Act
        var result = JsonSerializer.Deserialize<Category>(json, _options);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Read_InvalidEnumValue_ThrowsJsonExceptionWithHelpfulMessage()
    {
        // Arrange
        var json = "\"InvalidCategory\"";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Category>(json, _options)
        );

        Assert.Contains("InvalidCategory", exception.Message);
        Assert.Contains("Valid values are:", exception.Message);
        Assert.Contains("Metal", exception.Message); // Should show valid options
    }

    [Fact]
    public void Read_EmptyString_ThrowsJsonException()
    {
        // Arrange
        var json = "\"\"";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Category>(json, _options)
        );

        Assert.Contains("Empty string", exception.Message);
    }

    [Fact]
    public void Write_ValidEnumValue_WritesAsString()
    {
        // Arrange
        var category = Category.Metal;

        // Act
        var json = JsonSerializer.Serialize(category, _options);

        // Assert
        Assert.Equal("\"Metal\"", json);
    }

    [Theory]
    [InlineData(Category.Metal, "\"Metal\"")]
    [InlineData(Category.Wood, "\"Wood\"")]
    [InlineData(Category.Leather, "\"Leather\"")]
    public void Write_VariousCategories_WriteCorrectly(Category value, string expectedJson)
    {
        // Act
        var json = JsonSerializer.Serialize(value, _options);

        // Assert
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void RoundTrip_EnumValue_PreservesValue()
    {
        // Arrange
        var original = Category.Magical;

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<Category>(json, _options);

        // Assert
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void Read_QualityTier_ParsesCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new EnumJsonConverter<QualityTier>() }
        };
        var json = "\"Epic\"";

        // Act
        var result = JsonSerializer.Deserialize<QualityTier>(json, options);

        // Assert
        Assert.Equal(QualityTier.Epic, result);
    }

    [Fact]
    public void Read_ItemType_ParsesCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new EnumJsonConverter<ItemType>() }
        };
        var json = "\"Weapon\"";

        // Act
        var result = JsonSerializer.Deserialize<ItemType>(json, options);

        // Assert
        Assert.Equal(ItemType.Weapon, result);
    }
}
