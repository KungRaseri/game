#nullable enable

namespace Game.Core.Data.Models;

/// <summary>
/// Represents the result of a JSON schema validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the validation error messages, if any.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// Gets the validation warning messages, if any.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result</returns>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    /// <returns>A failed validation result</returns>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    /// <summary>
    /// Creates a failed validation result with errors and warnings.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    /// <param name="warnings">The validation warnings</param>
    /// <returns>A failed validation result</returns>
    public static ValidationResult Failure(IEnumerable<string> errors, IEnumerable<string> warnings) => new()
    {
        IsValid = false,
        Errors = errors.ToList(),
        Warnings = warnings.ToList()
    };

    /// <summary>
    /// Creates a successful validation result with warnings.
    /// </summary>
    /// <param name="warnings">The validation warnings</param>
    /// <returns>A successful validation result with warnings</returns>
    public static ValidationResult SuccessWithWarnings(params string[] warnings) => new()
    {
        IsValid = true,
        Warnings = warnings.ToList()
    };

    /// <summary>
    /// Gets a summary of the validation result.
    /// </summary>
    /// <returns>A summary string</returns>
    public override string ToString()
    {
        if (IsValid && !Warnings.Any())
        {
            return "Validation successful";
        }

        if (IsValid && Warnings.Any())
        {
            return $"Validation successful with {Warnings.Count} warning(s)";
        }

        return $"Validation failed with {Errors.Count} error(s)" +
               (Warnings.Any() ? $" and {Warnings.Count} warning(s)" : "");
    }
}
