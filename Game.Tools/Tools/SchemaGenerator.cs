#nullable enable

using System.Text.Json;
using Game.Core.Data.Interfaces;
using Game.Core.Utils;

namespace Game.Tools.Tools;

/// <summary>
/// Utility for generating JSON schema files for data types
/// </summary>
public static class SchemaGenerator
{
    /// <summary>
    /// Generates and saves a JSON schema file for the specified type
    /// </summary>
    /// <typeparam name="T">The type to generate schema for</typeparam>
    /// <param name="validator">The JSON schema validator to use for generation</param>
    /// <param name="outputPath">The path to save the schema file</param>
    /// <param name="overwrite">Whether to overwrite existing schema files</param>
    /// <returns>True if the schema was generated successfully</returns>
    public static async Task<bool> GenerateSchemaFileAsync<T>(
        IJsonSchemaValidator validator,
        string outputPath,
        bool overwrite = false)
    {
        try
        {
            if (!overwrite && File.Exists(outputPath))
            {
                GameLogger.Info($"Schema file already exists: {outputPath}");
                return true;
            }

            var schema = validator.GenerateSchema<T>();
            
            // Format the JSON for better readability
            var jsonDocument = JsonDocument.Parse(schema);
            var formattedJson = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Ensure directory exists
            var directory = Path.GetDirectoryName(outputPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(outputPath, formattedJson);
            
            GameLogger.Info($"Generated JSON schema for {typeof(T).Name}: {outputPath}");
            return true;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to generate schema for {typeof(T).Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Generates schema files for multiple types
    /// </summary>
    /// <param name="validator">The JSON schema validator to use</param>
    /// <param name="schemasDirectory">The directory to save schema files</param>
    /// <param name="typeSchemaMap">Dictionary mapping types to schema file names</param>
    /// <param name="overwrite">Whether to overwrite existing schema files</param>
    /// <returns>Number of schemas generated successfully</returns>
    public static async Task<int> GenerateMultipleSchemasAsync(
        IJsonSchemaValidator validator,
        string schemasDirectory,
        Dictionary<Type, string> typeSchemaMap,
        bool overwrite = false)
    {
        var successCount = 0;
        
        foreach (var (type, fileName) in typeSchemaMap)
        {
            var outputPath = Path.Combine(schemasDirectory, fileName);
            
            // Use reflection to call the generic method
            var method = typeof(SchemaGenerator)
                .GetMethod(nameof(GenerateSchemaFileAsync))!
                .MakeGenericMethod(type);
                
            var task = (Task<bool>)method.Invoke(null, new object[] { validator, outputPath, overwrite })!;
            var success = await task;
            
            if (success)
            {
                successCount++;
            }
        }
        
        GameLogger.Info($"Generated {successCount}/{typeSchemaMap.Count} JSON schemas");
        return successCount;
    }
}
