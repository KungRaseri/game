#nullable enable

using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;
using Game.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Game.Core.Data.Services;

/// <summary>
/// JSON schema validator implementation using Newtonsoft.Json.Schema.
/// </summary>
public class JsonSchemaValidator : IJsonSchemaValidator
{
    private readonly Dictionary<Type, string> _schemaCache = new();

    /// <summary>
    /// Validates JSON content against a schema.
    /// </summary>
    /// <param name="jsonContent">The JSON content to validate</param>
    /// <param name="schemaContent">The JSON schema to validate against</param>
    /// <returns>Validation result with any errors found</returns>
    public Task<ValidationResult> ValidateAsync(string jsonContent, string schemaContent)
    {
        try
        {
            var schema = JSchema.Parse(schemaContent);
            var json = JToken.Parse(jsonContent);

            var errors = new List<string>();
            var warnings = new List<string>();

            bool isValid = json.IsValid(schema, out IList<ValidationError> validationErrors);

            foreach (var error in validationErrors)
            {
                var errorMessage = $"Path: {error.Path} - {error.Message}";
                
                // Treat some validation issues as warnings rather than errors
                if (IsWarningLevel(error))
                {
                    warnings.Add(errorMessage);
                }
                else
                {
                    errors.Add(errorMessage);
                    isValid = false;
                }
            }

            if (isValid && warnings.Any())
            {
                GameLogger.Warning($"[Validation] JSON validation completed with {warnings.Count} warnings");
                return Task.FromResult(ValidationResult.SuccessWithWarnings(warnings.ToArray()));
            }

            if (isValid)
            {
                GameLogger.Debug("[Validation] JSON validation completed successfully");
                return Task.FromResult(ValidationResult.Success());
            }

            GameLogger.Error($"[Validation] JSON validation failed with {errors.Count} errors");
            return Task.FromResult(ValidationResult.Failure(errors, warnings));
        }
        catch (JsonReaderException ex)
        {
            GameLogger.Error($"[Validation] Invalid JSON format: {ex.Message}");
            return Task.FromResult(ValidationResult.Failure($"Invalid JSON format: {ex.Message}"));
        }
        catch (JSchemaException ex)
        {
            GameLogger.Error($"[Validation] Invalid JSON schema: {ex.Message}");
            return Task.FromResult(ValidationResult.Failure($"Invalid JSON schema: {ex.Message}"));
        }
        catch (Exception ex)
        {
            GameLogger.Error($"[Validation] Unexpected validation error: {ex.Message}");
            return Task.FromResult(ValidationResult.Failure($"Validation error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Validates a JSON file against a schema file.
    /// </summary>
    /// <param name="jsonFilePath">Path to the JSON file to validate</param>
    /// <param name="schemaFilePath">Path to the JSON schema file</param>
    /// <returns>Validation result with any errors found</returns>
    public async Task<ValidationResult> ValidateFileAsync(string jsonFilePath, string schemaFilePath)
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                return ValidationResult.Failure($"JSON file not found: {jsonFilePath}");
            }

            if (!File.Exists(schemaFilePath))
            {
                return ValidationResult.Failure($"Schema file not found: {schemaFilePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var schemaContent = await File.ReadAllTextAsync(schemaFilePath);

            GameLogger.Debug($"[Validation] Validating {Path.GetFileName(jsonFilePath)} against {Path.GetFileName(schemaFilePath)}");

            return await ValidateAsync(jsonContent, schemaContent);
        }
        catch (Exception ex)
        {
            GameLogger.Error($"[Validation] Error reading files for validation: {ex.Message}");
            return ValidationResult.Failure($"File reading error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a JSON schema from a C# type.
    /// </summary>
    /// <typeparam name="T">The type to generate schema for</typeparam>
    /// <returns>JSON schema as string</returns>
    public string GenerateSchema<T>()
    {
        var type = typeof(T);
        
        if (_schemaCache.TryGetValue(type, out var cachedSchema))
        {
            return cachedSchema;
        }

        var generator = new JSchemaGenerator();
        
        // Configure schema generation
        generator.SchemaIdGenerationHandling = SchemaIdGenerationHandling.TypeName;
        generator.DefaultRequired = Required.DisallowNull;
        
        var schema = generator.Generate(type);
        var schemaJson = schema.ToString();

        _schemaCache[type] = schemaJson;

        GameLogger.Debug($"[Validation] Generated JSON schema for type {type.Name}");
        return schemaJson;
    }

    /// <summary>
    /// Determines if a validation error should be treated as a warning.
    /// </summary>
    /// <param name="error">The validation error</param>
    /// <returns>True if this should be treated as a warning</returns>
    private static bool IsWarningLevel(ValidationError error)
    {
        // Treat additional properties as warnings for flexibility
        if (error.ErrorType == ErrorType.AdditionalProperties)
        {
            return true;
        }

        // Treat format validation as warnings (e.g., date format suggestions)
        if (error.ErrorType == ErrorType.Format)
        {
            return true;
        }

        // All other validation errors are actual errors
        return false;
    }
}
