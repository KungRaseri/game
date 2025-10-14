#nullable enable

using System.Text.Json;
using Game.Adventure.Data.Json;
using Game.Adventure.Models;

namespace Game.Adventure.Tests.Data.Json;

/// <summary>
/// Tests for EntityType JSON serialization and deserialization.
/// </summary>
public class EntityTypeJsonConverterTests
{
    [Fact]
    public void Read_ValidStringValues_ParsesCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EntityTypeJsonConverter());

        var testCases = new[]
        {
            ("\"adventurer\"", EntityType.Adventurer),
            ("\"monster\"", EntityType.Monster),
            ("\"npc\"", EntityType.NPC),
            ("\"boss\"", EntityType.Boss),
            ("\"Adventurer\"", EntityType.Adventurer), // Case insensitive
            ("\"MONSTER\"", EntityType.Monster) // Case insensitive
        };

        foreach (var (json, expected) in testCases)
        {
            // Act
            var result = JsonSerializer.Deserialize<EntityType>(json, options);

            // Assert
            Assert.Equal(expected, result);
        }
    }

    [Fact]
    public void Read_InvalidStringValue_ThrowsJsonException()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EntityTypeJsonConverter());
        var invalidJson = "\"invalid_type\"";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityType>(invalidJson, options));
    }

    [Fact]
    public void Read_EmptyString_ReturnsDefaultValue()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EntityTypeJsonConverter());
        var emptyJson = "\"\"";

        // Act
        var result = JsonSerializer.Deserialize<EntityType>(emptyJson, options);

        // Assert
        Assert.Equal(EntityType.Monster, result); // Default fallback
    }

    [Fact]
    public void Write_ValidEnumValues_WritesCorrectStrings()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EntityTypeJsonConverter());

        var testCases = new[]
        {
            (EntityType.Adventurer, "\"adventurer\""),
            (EntityType.Monster, "\"monster\""),
            (EntityType.NPC, "\"npc\""),
            (EntityType.Boss, "\"boss\"")
        };

        foreach (var (entityType, expected) in testCases)
        {
            // Act
            var result = JsonSerializer.Serialize(entityType, options);

            // Assert
            Assert.Equal(expected, result);
        }
    }

    [Fact]
    public void RoundTrip_AllEntityTypes_PreservesValues()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EntityTypeJsonConverter());
        var entityTypes = Enum.GetValues<EntityType>();

        foreach (var entityType in entityTypes)
        {
            // Act
            var json = JsonSerializer.Serialize(entityType, options);
            var result = JsonSerializer.Deserialize<EntityType>(json, options);

            // Assert
            Assert.Equal(entityType, result);
        }
    }
}
