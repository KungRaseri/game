using System.Text.Json;
using Game.Items.Data.Json;
using Game.Items.Models;

namespace Game.Items.Tests.Serialization;

public class WeaponTypeJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public WeaponTypeJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new WeaponTypeJsonConverter());
    }

    [Theory]
    [InlineData(WeaponType.Sword, "\"Sword\"")]
    [InlineData(WeaponType.Axe, "\"Axe\"")]
    [InlineData(WeaponType.Dagger, "\"Dagger\"")]
    public void Write_ValidWeaponType_WritesAsString(WeaponType weaponType, string expectedJson)
    {
        // Act
        var json = JsonSerializer.Serialize(weaponType, _options);

        // Assert
        Assert.Equal(expectedJson, json);
    }

    [Theory]
    [InlineData("\"Sword\"", WeaponType.Sword)]
    [InlineData("\"Axe\"", WeaponType.Axe)]
    [InlineData("\"Dagger\"", WeaponType.Dagger)]
    [InlineData("\"sword\"", WeaponType.Sword)] // Case insensitive
    [InlineData("\"AXE\"", WeaponType.Axe)] // Case insensitive
    [InlineData("\"dagger\"", WeaponType.Dagger)] // Case insensitive
    public void Read_ValidWeaponType_ParsesCorrectly(string json, WeaponType expectedWeaponType)
    {
        // Act
        var weaponType = JsonSerializer.Deserialize<WeaponType>(json, _options);

        // Assert
        Assert.Equal(expectedWeaponType, weaponType);
    }

    [Theory]
    [InlineData("\"InvalidWeaponType\"")]
    [InlineData("\"\"")]
    [InlineData("null")]
    public void Read_InvalidWeaponType_ThrowsJsonException(string invalidJson)
    {
        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<WeaponType>(invalidJson, _options));
    }

    [Fact]
    public void Read_InvalidWeaponType_ThrowsJsonExceptionWithHelpfulMessage()
    {
        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<WeaponType>("\"InvalidType\"", _options));

        Assert.Contains("Invalid WeaponType value: 'InvalidType'", exception.Message);
        Assert.Contains("Valid values are: Sword, Axe, Dagger", exception.Message);
    }
}
