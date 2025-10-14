#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game.Core.Serialization;

/// <summary>
/// Generic JSON converter for enum types that provides case-insensitive parsing
/// and clear error messages when invalid values are encountered.
/// </summary>
/// <typeparam name="T">The enum type to convert</typeparam>
public class EnumJsonConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private readonly bool _allowIntegerValues;

    /// <summary>
    /// Creates a new enum JSON converter with default settings.
    /// </summary>
    public EnumJsonConverter() : this(false)
    {
    }

    /// <summary>
    /// Creates a new enum JSON converter.
    /// </summary>
    /// <param name="allowIntegerValues">Whether to allow integer values in JSON (default: false)</param>
    public EnumJsonConverter(bool allowIntegerValues)
    {
        _allowIntegerValues = allowIntegerValues;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException($"Empty string is not a valid value for {typeof(T).Name}");
            }

            // Case-insensitive parsing for better compatibility
            if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
            {
                return result;
            }

            // Provide helpful error message with valid values
            var validValues = string.Join(", ", Enum.GetNames<T>());
            throw new JsonException(
                $"Invalid {typeof(T).Name} value: '{value}'. Valid values are: {validValues}"
            );
        }

        if (reader.TokenType == JsonTokenType.Number && _allowIntegerValues)
        {
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(T), intValue))
            {
                return (T)Enum.ToObject(typeof(T), intValue);
            }

            throw new JsonException($"Invalid {typeof(T).Name} integer value: {intValue}");
        }

        throw new JsonException(
            $"Unexpected token type {reader.TokenType} when parsing {typeof(T).Name}. Expected String{(_allowIntegerValues ? " or Number" : "")}."
        );
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Always write as string for consistency
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// JSON converter factory that creates EnumJsonConverter instances.
/// Useful for applying the converter via attributes without specifying generic parameters.
/// </summary>
public class EnumJsonConverterFactory : JsonConverterFactory
{
    private readonly bool _allowIntegerValues;

    /// <summary>
    /// Creates a new enum JSON converter factory with default settings.
    /// </summary>
    public EnumJsonConverterFactory() : this(false)
    {
    }

    /// <summary>
    /// Creates a new enum JSON converter factory.
    /// </summary>
    /// <param name="allowIntegerValues">Whether to allow integer values in JSON (default: false)</param>
    public EnumJsonConverterFactory(bool allowIntegerValues)
    {
        _allowIntegerValues = allowIntegerValues;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType, _allowIntegerValues);
    }
}
