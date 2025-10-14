#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Adventure.Models;

namespace Game.Adventure.Data.Json;

/// <summary>
/// JSON converter for EntityType enum with backward compatibility.
/// Handles both string representations and enum values.
/// </summary>
public class EntityTypeJsonConverter : JsonConverter<EntityType>
{
    public override EntityType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token for EntityType, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return EntityType.Monster; // Default fallback
        }

        // Handle both enum names and legacy string values
        return value.ToLowerInvariant() switch
        {
            "adventurer" => EntityType.Adventurer,
            "monster" => EntityType.Monster,
            "npc" => EntityType.NPC,
            "boss" => EntityType.Boss,
            _ => Enum.TryParse<EntityType>(value, true, out var result) 
                ? result 
                : throw new JsonException($"Invalid EntityType value: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, EntityType value, JsonSerializerOptions options)
    {
        // Write as lowercase string for backward compatibility
        var stringValue = value switch
        {
            EntityType.Adventurer => "adventurer",
            EntityType.Monster => "monster",
            EntityType.NPC => "npc",
            EntityType.Boss => "boss",
            _ => value.ToString().ToLowerInvariant()
        };
        
        writer.WriteStringValue(stringValue);
    }
}
