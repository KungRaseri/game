#nullable enable

namespace Game.Core.CQS;

/// <summary>
/// Base class for validation exceptions thrown when command or query validation fails.
/// Provides consistent error handling across the CQS infrastructure.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}