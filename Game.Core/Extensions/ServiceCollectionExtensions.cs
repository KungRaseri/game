#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.CQS;
using Game.Core.Data.Interfaces;
using Game.Core.Data.Services;
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
        // Register the dispatcher as singleton for performance
        services.AddSingleton<IDispatcher, Dispatcher>();
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
        services.Add(new ServiceDescriptor(typeof(ICommandHandler<TCommand>), typeof(THandler), lifetime));
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
        services.Add(new ServiceDescriptor(typeof(ICommandHandler<TCommand, TResult>), typeof(THandler), lifetime));
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
        services.Add(new ServiceDescriptor(typeof(IQueryHandler<TQuery, TResult>), typeof(THandler), lifetime));
        return services;
    }

    /// <summary>
    /// Registers data services including JSON loading, caching, and validation.
    /// Call this method to enable JSON data loading with validation and caching.
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Register data loading services
        services.AddSingleton(typeof(IDataLoader<>), typeof(JsonDataLoader<>));
        services.AddSingleton(typeof(IDataCache<>), typeof(DataCacheService<>));
        
        // Register JSON schema validation services
        services.AddSingleton<IJsonSchemaValidator, JsonSchemaValidator>();
        
        return services;
    }
}
