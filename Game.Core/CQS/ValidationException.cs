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

/// <summary>
/// Exception thrown when a command or query handler is not found in the dependency injection container.
/// Indicates a configuration issue where handlers are not properly registered.
/// </summary>
public class HandlerNotFoundException : Exception
{
    public Type RequestType { get; }

    public HandlerNotFoundException(Type requestType) 
        : base($"No handler registered for request type: {requestType.Name}")
    {
        RequestType = requestType;
    }

    public HandlerNotFoundException(Type requestType, Exception innerException)
        : base($"No handler registered for request type: {requestType.Name}", innerException)
    {
        RequestType = requestType;
    }
}
