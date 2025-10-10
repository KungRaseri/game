#nullable enable

using Game.Core.Data.Models;

namespace Game.Core.Data.Interfaces;

/// <summary>
/// Interface for validating data before it's used in the application
/// </summary>
/// <typeparam name="T">The type of data to validate</typeparam>
public interface IDataValidator<T> where T : class
{
    /// <summary>
    /// Validates the provided data
    /// </summary>
    /// <param name="data">The data to validate</param>
    /// <returns>Validation result indicating success or failure with details</returns>
    DataValidationResult Validate(T data);

    /// <summary>
    /// Validates JSON data before deserialization
    /// </summary>
    /// <param name="jsonData">The JSON string to validate</param>
    /// <returns>Validation result for the JSON structure</returns>
    DataValidationResult ValidateJson(string jsonData);
}
