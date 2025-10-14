#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Items.Models;

namespace Game.Items.Data.Json;

/// <summary>
/// JSON converter for ArmorType enum.
/// Provides case-insensitive serialization and deserialization.
/// </summary>
public class ArmorTypeJsonConverter : JsonConverter<ArmorType>
{
    public override ArmorType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("ArmorType value cannot be null or empty");
        }

        if (Enum.TryParse<ArmorType>(value, ignoreCase: true, out var armorType))
        {
            return armorType;
        }

        throw new JsonException($"Invalid ArmorType value: '{value}'. Valid values are: {string.Join(", ", Enum.GetNames<ArmorType>())}");
    }

    public override void Write(Utf8JsonWriter writer, ArmorType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
