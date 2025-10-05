#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Game.Core.CQS;

/// <summary>
/// Interface for command handlers that execute commands without return values.
/// Handlers contain the actual business logic for state-changing operations.
/// </summary>
/// <typeparam name="TCommand">Type of command to handle</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes the command asynchronously.
    /// Contains the actual business logic for the operation.
    /// </summary>
    /// <param name="command">Command to execute</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for command handlers that execute commands with return values.
/// Handlers contain the actual business logic for state-changing operations that need to return data.
/// </summary>
/// <typeparam name="TCommand">Type of command to handle</typeparam>
/// <typeparam name="TResult">Type of result returned by the command</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Executes the command asynchronously and returns a result.
    /// Contains the actual business logic for the operation.
    /// </summary>
    /// <param name="command">Command to execute</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Result of the command execution</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
