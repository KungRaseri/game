#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Game.Core.CQS;

/// <summary>
/// Interface for dispatching commands and queries to their respective handlers.
/// Provides a central point for executing CQS operations across the application.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Dispatches a command to its handler for execution.
    /// Commands modify state but don't return data.
    /// </summary>
    /// <typeparam name="TCommand">Type of command to dispatch</typeparam>
    /// <param name="command">Command to execute</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    /// Dispatches a command to its handler for execution and returns a result.
    /// Commands modify state and return minimal data (like IDs or status).
    /// </summary>
    /// <typeparam name="TCommand">Type of command to dispatch</typeparam>
    /// <typeparam name="TResult">Type of result returned by the command</typeparam>
    /// <param name="command">Command to execute</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Result of the command execution</returns>
    Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Dispatches a query to its handler for execution and returns the requested data.
    /// Queries return data without modifying state.
    /// </summary>
    /// <typeparam name="TQuery">Type of query to dispatch</typeparam>
    /// <typeparam name="TResult">Type of result returned by the query</typeparam>
    /// <param name="query">Query to execute</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Data requested by the query</returns>
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
