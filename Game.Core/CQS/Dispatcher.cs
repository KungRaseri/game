#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Game.Core.Utils;

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
        var commandType = typeof(TCommand).Name;

        if (command == null)
        {
            GameLogger.Error($"[CQS] Command is null for type: {commandType}");
            throw new ArgumentNullException(nameof(command));
        }

        var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();

        if (handler == null)
        {
            GameLogger.Error($"[CQS] No command handler registered for command type: {commandType}");
            throw new InvalidOperationException(
                $"No command handler registered for command type: {commandType}");
        }

        try
        {
            await handler.HandleAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"[CQS] Command execution failed: {commandType}");
            throw;
        }
    }

    /// <summary>
    /// Dispatches a command to its registered handler and returns the result.
    /// Throws InvalidOperationException if no handler is registered.
    /// </summary>
    public async Task<TResult> DispatchCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var commandType = typeof(TCommand).Name;
        
        if (command == null)
        {
            GameLogger.Error($"[CQS] Command is null for type: {commandType}");
            throw new ArgumentNullException(nameof(command));
        }

        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();
        
        if (handler == null)
        {
            GameLogger.Error($"[CQS] No command handler registered for command type: {commandType}");
            throw new InvalidOperationException(
                $"No command handler registered for command type: {commandType}");
        }
        
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"[CQS] Command execution failed: {commandType}");
            throw;
        }
    }

    /// <summary>
    /// Dispatches a query to its registered handler and returns the result.
    /// Throws InvalidOperationException if no handler is registered.
    /// </summary>
    public async Task<TResult> DispatchQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var queryType = typeof(TQuery).Name;
        
        if (query == null)
        {
            GameLogger.Error($"[CQS] Query is null for type: {queryType}");
            throw new ArgumentNullException(nameof(query));
        }

        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
        
        if (handler == null)
        {
            GameLogger.Error($"[CQS] No query handler registered for query type: {queryType}");
            throw new InvalidOperationException(
                $"No query handler registered for query type: {queryType}");
        }
        
        try
        {
            var result = await handler.HandleAsync(query, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"[CQS] Query execution failed: {queryType}");
            throw;
        }
    }
}
