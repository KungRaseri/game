#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Items.Models;

namespace Game.Items.Data.Json;

/// <summary>
/// JSON converter for WeaponType enum.
/// Provides case-insensitive serialization and deserialization.
/// </summary>
public class WeaponTypeJsonConverter : JsonConverter<WeaponType>
{
    public override WeaponType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("WeaponType value cannot be null or empty");
        }

        if (Enum.TryParse<WeaponType>(value, ignoreCase: true, out var weaponType))
        {
            return weaponType;
        }

        throw new JsonException($"Invalid WeaponType value: '{value}'. Valid values are: {string.Join(", ", Enum.GetNames<WeaponType>())}");
    }

    public override void Write(Utf8JsonWriter writer, WeaponType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
