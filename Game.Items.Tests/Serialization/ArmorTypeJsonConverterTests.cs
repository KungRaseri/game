using System.Text.Json;
using Game.Items.Data.Json;
using Game.Items.Models;

namespace Game.Items.Tests.Serialization;

public class ArmorTypeJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public ArmorTypeJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new ArmorTypeJsonConverter());
    }

    [Theory]
    [InlineData(ArmorType.Light, "\"Light\"")]
    [InlineData(ArmorType.Medium, "\"Medium\"")]
    [InlineData(ArmorType.Heavy, "\"Heavy\"")]
    public void Write_ValidArmorType_WritesAsString(ArmorType armorType, string expectedJson)
    {
        // Act
        var json = JsonSerializer.Serialize(armorType, _options);

        // Assert
        Assert.Equal(expectedJson, json);
    }

    [Theory]
    [InlineData("\"Light\"", ArmorType.Light)]
    [InlineData("\"Medium\"", ArmorType.Medium)]
    [InlineData("\"Heavy\"", ArmorType.Heavy)]
    [InlineData("\"light\"", ArmorType.Light)] // Case insensitive
    [InlineData("\"MEDIUM\"", ArmorType.Medium)] // Case insensitive
    [InlineData("\"heavy\"", ArmorType.Heavy)] // Case insensitive
    public void Read_ValidArmorType_ParsesCorrectly(string json, ArmorType expectedArmorType)
    {
        // Act
        var armorType = JsonSerializer.Deserialize<ArmorType>(json, _options);

        // Assert
        Assert.Equal(expectedArmorType, armorType);
    }

    [Theory]
    [InlineData("\"InvalidArmorType\"")]
    [InlineData("\"\"")]
    [InlineData("null")]
    public void Read_InvalidArmorType_ThrowsJsonException(string invalidJson)
    {
        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<ArmorType>(invalidJson, _options));
    }

    [Fact]
    public void Read_InvalidArmorType_ThrowsJsonExceptionWithHelpfulMessage()
    {
        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<ArmorType>("\"InvalidType\"", _options));

        Assert.Contains("Invalid ArmorType value: 'InvalidType'", exception.Message);
        Assert.Contains("Valid values are: Light, Medium, Heavy", exception.Message);
    }
}
