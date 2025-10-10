#nullable enable

namespace Game.Core.Data.Models;

/// <summary>
/// Result of data validation operations
/// </summary>
public sealed class DataValidationResult
{
    private DataValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// List of validation error messages (empty if valid)
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Whether the validation failed
    /// </summary>
    public bool IsInvalid => !IsValid;

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    /// <returns>Valid result</returns>
    public static DataValidationResult Valid()
    {
        return new DataValidationResult(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed validation result with error messages
    /// </summary>
    /// <param name="errors">List of validation errors</param>
    /// <returns>Invalid result</returns>
    public static DataValidationResult Invalid(params string[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return new DataValidationResult(false, errors.ToList().AsReadOnly());
    }

    /// <summary>
    /// Creates a failed validation result with error messages
    /// </summary>
    /// <param name="errors">List of validation errors</param>
    /// <returns>Invalid result</returns>
    public static DataValidationResult Invalid(IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return new DataValidationResult(false, errors.ToList().AsReadOnly());
    }
}
