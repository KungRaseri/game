namespace Game.Core.CQS;

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