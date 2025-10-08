#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.CQS;
using Game.Core.Extensions;
using Game.Core.Tests.CQS;

namespace Game.Core.Tests.Extensions;

/// <summary>
/// Integration tests that verify the ServiceCollectionExtensions work correctly
/// with real dependency injection scenarios and complex handler configurations.
/// </summary>
public class ServiceCollectionExtensionsIntegrationTests : IDisposable
{
    private readonly ServiceCollection _services;
    private ServiceProvider? _serviceProvider;

    public ServiceCollectionExtensionsIntegrationTests()
    {
        _services = new ServiceCollection();
    }

    [Fact]
    public async Task CompleteWorkflow_RegisterAndExecuteMultipleHandlers_WorksCorrectly()
    {
        // Arrange
        _services
            .AddCQS()
            .AddCommandHandler<TestCommand, TestCommandHandler>()
            .AddCommandHandler<TestCommandWithResult, string, TestCommandWithResultHandler>()
            .AddQueryHandler<TestQuery, string, TestQueryHandler>();

        _serviceProvider = _services.BuildServiceProvider();
        var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();

        // Act & Assert - Execute multiple operations
        await dispatcher.DispatchCommandAsync(new TestCommand("Command 1"));
        await dispatcher.DispatchCommandAsync(new TestCommand("Command 2"));

        var commandResult = await dispatcher.DispatchCommandAsync<TestCommandWithResult, string>(
            new TestCommandWithResult("Command with result"));
        Assert.Equal("Processed: Command with result", commandResult);

        var queryResult = await dispatcher.DispatchQueryAsync<TestQuery, string>(
            new TestQuery("Query filter"));
        Assert.Equal("Query result for: Query filter", queryResult);
    }

    [Fact]
    public void MultipleHandlerRegistrations_ForSameInterface_LastOneWins()
    {
        // Arrange
        _services
            .AddCQS()
            .AddCommandHandler<TestCommand, TestCommandHandler>()
            .AddCommandHandler<TestCommand, CancellableCommandHandler>(); // This should override

        // Act
        _serviceProvider = _services.BuildServiceProvider();
        var handler = _serviceProvider.GetService<ICommandHandler<TestCommand>>();

        // Assert
        Assert.IsType<CancellableCommandHandler>(handler);
    }

    [Fact]
    public void DifferentLifetimes_WorkCorrectly()
    {
        // Arrange
        _services
            .AddCQS()
            .AddCommandHandler<TestCommand, TestCommandHandler>(ServiceLifetime.Singleton)
            .AddQueryHandler<TestQuery, string, TestQueryHandler>(ServiceLifetime.Transient);

        // Act
        _serviceProvider = _services.BuildServiceProvider();

        // Assert - Singleton behavior
        var handler1 = _serviceProvider.GetService<ICommandHandler<TestCommand>>();
        var handler2 = _serviceProvider.GetService<ICommandHandler<TestCommand>>();
        Assert.Same(handler1, handler2);

        // Assert - Transient behavior
        var queryHandler1 = _serviceProvider.GetService<IQueryHandler<TestQuery, string>>();
        var queryHandler2 = _serviceProvider.GetService<IQueryHandler<TestQuery, string>>();
        Assert.NotSame(queryHandler1, queryHandler2);
    }

    [Fact]
    public void ScopedServices_CreateNewInstancePerScope()
    {
        // Arrange
        _services
            .AddCQS()
            .AddCommandHandler<TestCommand, TestCommandHandler>(ServiceLifetime.Scoped);

        _serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        using (var scope1 = _serviceProvider.CreateScope())
        {
            var handler1a = scope1.ServiceProvider.GetService<ICommandHandler<TestCommand>>();
            var handler1b = scope1.ServiceProvider.GetService<ICommandHandler<TestCommand>>();
            Assert.Same(handler1a, handler1b); // Same within scope
        }

        using (var scope2 = _serviceProvider.CreateScope())
        {
            var handler2 = scope2.ServiceProvider.GetService<ICommandHandler<TestCommand>>();

            using (var scope3 = _serviceProvider.CreateScope())
            {
                var handler3 = scope3.ServiceProvider.GetService<ICommandHandler<TestCommand>>();
                Assert.NotSame(handler2, handler3); // Different across scopes
            }
        }
    }

    [Fact]
    public async Task HandlerWithDependencies_GetsInjectedCorrectly()
    {
        // Arrange - Handler that requires a dependency
        _services.AddSingleton<TestDependency>();
        _services
            .AddCQS()
            .AddCommandHandler<TestCommand, HandlerWithDependencies>();

        _serviceProvider = _services.BuildServiceProvider();
        var dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();

        // Act
        await dispatcher.DispatchCommandAsync(new TestCommand("test"));

        // Assert
        var dependency = _serviceProvider.GetService<TestDependency>();
        Assert.NotNull(dependency);
        Assert.True(dependency.WasCalled);
    }

    [Fact]
    public void ServiceRegistration_CanBeInspected()
    {
        // Arrange & Act
        _services
            .AddCQS()
            .AddCommandHandler<TestCommand, TestCommandHandler>()
            .AddQueryHandler<TestQuery, string, TestQueryHandler>();

        // Assert - Can inspect registered services
        var dispatcherService = _services.FirstOrDefault(s => s.ServiceType == typeof(IDispatcher));
        Assert.NotNull(dispatcherService);
        Assert.Equal(ServiceLifetime.Singleton, dispatcherService.Lifetime);

        var commandHandlerService = _services.FirstOrDefault(s => s.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(commandHandlerService);
        Assert.Equal(ServiceLifetime.Scoped, commandHandlerService.Lifetime);

        var queryHandlerService = _services.FirstOrDefault(s => s.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.NotNull(queryHandlerService);
        Assert.Equal(ServiceLifetime.Scoped, queryHandlerService.Lifetime);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}