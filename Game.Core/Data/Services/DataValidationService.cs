#nullable enable

using System.Text.Json;
using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;

namespace Game.Core.Data.Services;

/// <summary>
/// Basic data validation service for JSON data
/// </summary>
/// <typeparam name="T">The type of data to validate</typeparam>
public class DataValidationService<T> : IDataValidator<T> where T : class
{
    /// <summary>
    /// Validates the provided data (override in derived classes for specific validation)
    /// </summary>
    public virtual DataValidationResult Validate(T data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        // Basic validation - just check that data is not null
        // Domain-specific validators can override this method
        return DataValidationResult.Valid();
    }

    /// <summary>
    /// Validates JSON data before deserialization
    /// </summary>
    public virtual DataValidationResult ValidateJson(string jsonData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonData);

        try
        {
            // Basic JSON structure validation
            using var document = JsonDocument.Parse(jsonData);
            
            // Check for required top-level properties (can be overridden)
            var validationErrors = ValidateJsonStructure(document.RootElement);
            
            return validationErrors.Count > 0 
                ? DataValidationResult.Invalid(validationErrors)
                : DataValidationResult.Valid();
        }
        catch (JsonException ex)
        {
            return DataValidationResult.Invalid($"Invalid JSON format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return DataValidationResult.Invalid($"JSON validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Override this method to validate specific JSON structure requirements
    /// </summary>
    /// <param name="rootElement">The root JSON element</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    protected virtual List<string> ValidateJsonStructure(JsonElement rootElement)
    {
        var errors = new List<string>();

        // Basic validation - check that it's an object
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            errors.Add("Root element must be a JSON object");
        }

        return errors;
    }
}
