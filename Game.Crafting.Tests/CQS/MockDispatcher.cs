using Game.Core.CQS;

namespace Game.Crafting.Tests.CQS;

/// <summary>
/// Mock dispatcher for testing that tracks dispatched commands and queries.
/// </summary>
public class MockDispatcher : IDispatcher
{
    private readonly Dictionary<System.Type, object> _commandHandlers = new();
    private readonly Dictionary<System.Type, object> _queryHandlers = new();

    public List<object> DispatchedCommands { get; } = new();
    public List<object> DispatchedQueries { get; } = new();

    /// <summary>
    /// Registers a command handler for testing.
    /// </summary>
    public void RegisterCommandHandler<TCommand>(Func<TCommand, Task> handler)
        where TCommand : ICommand
    {
        _commandHandlers[typeof(TCommand)] = handler;
    }

    /// <summary>
    /// Registers a command handler with result for testing.
    /// </summary>
    public void RegisterCommandHandler<TCommand, TResult>(Func<TCommand, Task<TResult>> handler)
        where TCommand : ICommand<TResult>
    {
        _commandHandlers[typeof(TCommand)] = handler;
    }

    /// <summary>
    /// Registers a query handler for testing.
    /// </summary>
    public void RegisterQueryHandler<TQuery, TResult>(Func<TQuery, Task<TResult>> handler)
        where TQuery : IQuery<TResult>
    {
        _queryHandlers[typeof(TQuery)] = handler;
    }

    public async Task DispatchCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        DispatchedCommands.Add(command);

        if (_commandHandlers.TryGetValue(typeof(TCommand), out var handler))
        {
            var typedHandler = (Func<TCommand, Task>)handler;
            await typedHandler(command);
        }
    }

    public async Task<TResult> DispatchCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        DispatchedCommands.Add(command);

        if (_commandHandlers.TryGetValue(typeof(TCommand), out var handler))
        {
            var typedHandler = (Func<TCommand, Task<TResult>>)handler;
            return await typedHandler(command);
        }

        return default(TResult)!;
    }

    public async Task<TResult> DispatchQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        DispatchedQueries.Add(query);

        if (_queryHandlers.TryGetValue(typeof(TQuery), out var handler))
        {
            var typedHandler = (Func<TQuery, Task<TResult>>)handler;
            return await typedHandler(query);
        }

        return default(TResult)!;
    }
}