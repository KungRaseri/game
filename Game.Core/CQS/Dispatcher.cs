#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Core.CQS;

/// <summary>
/// Default implementation of IDispatcher that uses dependency injection to resolve handlers.
/// Provides centralized command and query dispatching across the application.
/// </summary>
public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Dispatches a command to its registered handler.
    /// Throws InvalidOperationException if no handler is registered.
    /// </summary>
    public async Task DispatchCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No command handler registered for command type: {typeof(TCommand).Name}");
        }

        await handler.HandleAsync(command, cancellationToken);
    }

    /// <summary>
    /// Dispatches a command to its registered handler and returns the result.
    /// Throws InvalidOperationException if no handler is registered.
    /// </summary>
    public async Task<TResult> DispatchCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();
        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No command handler registered for command type: {typeof(TCommand).Name}");
        }

        return await handler.HandleAsync(command, cancellationToken);
    }

    /// <summary>
    /// Dispatches a query to its registered handler and returns the result.
    /// Throws InvalidOperationException if no handler is registered.
    /// </summary>
    public async Task<TResult> DispatchQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No query handler registered for query type: {typeof(TQuery).Name}");
        }

        return await handler.HandleAsync(query, cancellationToken);
    }
}
