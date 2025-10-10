#nullable enable

namespace Game.Core.Data.Interfaces;

/// <summary>
/// Interface for validating JSON data against schemas.
/// </summary>
public interface IJsonSchemaValidator
{
    /// <summary>
    /// Validates JSON content against a schema.
    /// </summary>
    /// <param name="jsonContent">The JSON content to validate</param>
    /// <param name="schemaContent">The JSON schema to validate against</param>
    /// <returns>Validation result with any errors found</returns>
    Task<ValidationResult> ValidateAsync(string jsonContent, string schemaContent);

    /// <summary>
    /// Validates a JSON file against a schema file.
    /// </summary>
    /// <param name="jsonFilePath">Path to the JSON file to validate</param>
    /// <param name="schemaFilePath">Path to the JSON schema file</param>
    /// <returns>Validation result with any errors found</returns>
    Task<ValidationResult> ValidateFileAsync(string jsonFilePath, string schemaFilePath);

    /// <summary>
    /// Generates a JSON schema from a C# type.
    /// </summary>
    /// <typeparam name="T">The type to generate schema for</typeparam>
    /// <returns>JSON schema as string</returns>
    string GenerateSchema<T>();
}
