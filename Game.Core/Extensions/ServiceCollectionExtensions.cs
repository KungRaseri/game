#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.CQS;
using Game.Core.Utils;

namespace Game.Core.Extensions;

/// <summary>
/// Extension methods for registering CQS services with dependency injection.
/// Provides a clean API for configuring the Command Query Separation infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core CQS services (IDispatcher and Dispatcher implementation).
    /// Call this method to enable command and query dispatching in your application.
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddCQS(this IServiceCollection services)
    {
        GameLogger.Debug("🏗️ [CQS] Registering core CQS services...");

        // Register the dispatcher as singleton for performance
        services.AddSingleton<IDispatcher, Dispatcher>();

        GameLogger.Debug("✅ [CQS] Core CQS services registered successfully");
        return services;
    }

    /// <summary>
    /// Registers a command handler with the dependency injection container.
    /// Use this to register handlers for commands that don't return values.
    /// </summary>
    /// <typeparam name="TCommand">Command type the handler processes</typeparam>
    /// <typeparam name="THandler">Handler implementation type</typeparam>
    /// <param name="services">Service collection to configure</param>
    /// <param name="lifetime">Service lifetime (default: Scoped)</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddCommandHandler<TCommand, THandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        var commandType = typeof(TCommand).Name;
        var handlerType = typeof(THandler).Name;

        GameLogger.Debug($"📝 [CQS] Registering command handler: {commandType} -> {handlerType} ({lifetime})");

        services.Add(new ServiceDescriptor(typeof(ICommandHandler<TCommand>), typeof(THandler), lifetime));

        GameLogger.Debug($"✅ [CQS] Command handler registered: {commandType} -> {handlerType}");
        return services;
    }

    /// <summary>
    /// Registers a command handler with return value with the dependency injection container.
    /// Use this to register handlers for commands that return data.
    /// </summary>
    /// <typeparam name="TCommand">Command type the handler processes</typeparam>
    /// <typeparam name="TResult">Result type returned by the command</typeparam>
    /// <typeparam name="THandler">Handler implementation type</typeparam>
    /// <param name="services">Service collection to configure</param>
    /// <param name="lifetime">Service lifetime (default: Scoped)</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddCommandHandler<TCommand, TResult, THandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TCommand : ICommand<TResult>
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        var commandType = typeof(TCommand).Name;
        var resultType = typeof(TResult).Name;
        var handlerType = typeof(THandler).Name;

        GameLogger.Debug($"📝 [CQS] Registering command handler with result: {commandType} -> {resultType} via {handlerType} ({lifetime})");

        services.Add(new ServiceDescriptor(typeof(ICommandHandler<TCommand, TResult>), typeof(THandler), lifetime));

        GameLogger.Debug($"✅ [CQS] Command handler with result registered: {commandType} -> {resultType} via {handlerType}");
        return services;
    }

    /// <summary>
    /// Registers a query handler with the dependency injection container.
    /// Use this to register handlers for queries that return data.
    /// </summary>
    /// <typeparam name="TQuery">Query type the handler processes</typeparam>
    /// <typeparam name="TResult">Result type returned by the query</typeparam>
    /// <typeparam name="THandler">Handler implementation type</typeparam>
    /// <param name="services">Service collection to configure</param>
    /// <param name="lifetime">Service lifetime (default: Scoped)</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddQueryHandler<TQuery, TResult, THandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TQuery : IQuery<TResult>
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        var queryType = typeof(TQuery).Name;
        var resultType = typeof(TResult).Name;
        var handlerType = typeof(THandler).Name;

        GameLogger.Debug($"📋 [CQS] Registering query handler: {queryType} -> {resultType} via {handlerType} ({lifetime})");

        services.Add(new ServiceDescriptor(typeof(IQueryHandler<TQuery, TResult>), typeof(THandler), lifetime));

        GameLogger.Debug($"✅ [CQS] Query handler registered: {queryType} -> {resultType} via {handlerType}");
        return services;
    }
}
