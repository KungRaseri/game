#nullable enable

using System;
using System.Diagnostics;
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
        GameLogger.Debug("[CQS] Dispatcher created with service provider");
    }

    /// <summary>
    /// Dispatches a command to its registered handler.
    /// Throws InvalidOperationException if no handler is registered.
    /// </summary>
    public async Task DispatchCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var commandType = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();
        
        GameLogger.Debug($"[CQS] Starting command dispatch: {commandType}");
        GameLogger.Debug($"[CQS] Command data: {command}");
        
        if (command == null)
        {
            GameLogger.Error($"[CQS] Command is null for type: {commandType}");
            throw new ArgumentNullException(nameof(command));
        }

        GameLogger.Debug($"[CQS] Resolving handler for command: {commandType}");
        var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
        
        if (handler == null)
        {
            GameLogger.Error($"[CQS] No command handler registered for command type: {commandType}");
            throw new InvalidOperationException(
                $"No command handler registered for command type: {commandType}");
        }

        GameLogger.Debug($"[CQS] Handler resolved: {handler.GetType().Name} for command: {commandType}");
        
        try
        {
            GameLogger.Debug($"[CQS] Executing command handler for: {commandType}");
            await handler.HandleAsync(command, cancellationToken);
            stopwatch.Stop();
            
            GameLogger.Debug($"[CQS] Command execution completed successfully: {commandType} (took {stopwatch.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            GameLogger.Error(ex, $"[CQS] Command execution failed: {commandType} (failed after {stopwatch.ElapsedMilliseconds}ms)");
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
        var resultType = typeof(TResult).Name;
        var stopwatch = Stopwatch.StartNew();
        
        GameLogger.Debug($"[CQS] Starting command dispatch with result: {commandType} -> {resultType}");
        GameLogger.Debug($"[CQS] Command data: {command}");
        
        if (command == null)
        {
            GameLogger.Error($"[CQS] Command is null for type: {commandType}");
            throw new ArgumentNullException(nameof(command));
        }

        GameLogger.Debug($"[CQS] Resolving handler for command with result: {commandType} -> {resultType}");
        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();
        
        if (handler == null)
        {
            GameLogger.Error($"[CQS] No command handler registered for command type: {commandType} -> {resultType}");
            throw new InvalidOperationException(
                $"No command handler registered for command type: {commandType}");
        }

        GameLogger.Debug($"[CQS] Handler resolved: {handler.GetType().Name} for command: {commandType} -> {resultType}");
        
        try
        {
            GameLogger.Debug($"[CQS] Executing command handler for: {commandType} -> {resultType}");
            var result = await handler.HandleAsync(command, cancellationToken);
            stopwatch.Stop();
            
            GameLogger.Debug($"[CQS] Command execution completed with result: {commandType} -> {resultType} (took {stopwatch.ElapsedMilliseconds}ms)");
            GameLogger.Debug($"[CQS] Command result: {result}");
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            GameLogger.Error(ex, $"[CQS] Command execution failed: {commandType} -> {resultType} (failed after {stopwatch.ElapsedMilliseconds}ms)");
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
        var resultType = typeof(TResult).Name;
        var stopwatch = Stopwatch.StartNew();
        
        GameLogger.Debug($"[CQS] Starting query dispatch: {queryType} -> {resultType}");
        GameLogger.Debug($"[CQS] Query data: {query}");
        
        if (query == null)
        {
            GameLogger.Error($"[CQS] Query is null for type: {queryType}");
            throw new ArgumentNullException(nameof(query));
        }

        GameLogger.Debug($"[CQS] Resolving handler for query: {queryType} -> {resultType}");
        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
        
        if (handler == null)
        {
            GameLogger.Error($"[CQS] No query handler registered for query type: {queryType} -> {resultType}");
            throw new InvalidOperationException(
                $"No query handler registered for query type: {queryType}");
        }

        GameLogger.Debug($"[CQS] Handler resolved: {handler.GetType().Name} for query: {queryType} -> {resultType}");
        
        try
        {
            GameLogger.Debug($"[CQS] Executing query handler for: {queryType} -> {resultType}");
            var result = await handler.HandleAsync(query, cancellationToken);
            stopwatch.Stop();
            
            GameLogger.Debug($"[CQS] Query execution completed successfully: {queryType} -> {resultType} (took {stopwatch.ElapsedMilliseconds}ms)");
            GameLogger.Debug($"[CQS] Query result: {result}");
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            GameLogger.Error(ex, $"[CQS] Query execution failed: {queryType} -> {resultType} (failed after {stopwatch.ElapsedMilliseconds}ms)");
            throw;
        }
    }
}
