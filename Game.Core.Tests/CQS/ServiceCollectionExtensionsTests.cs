#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.CQS;
using Game.Core.Extensions;

namespace Game.Core.Tests.CQS;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCQS_RegistersDispatcherAsService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQS();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetService<IDispatcher>();
        
        Assert.NotNull(dispatcher);
        Assert.IsType<Dispatcher>(dispatcher);
    }

    [Fact]
    public void AddCQS_RegistersDispatcherAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQS();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher1 = serviceProvider.GetService<IDispatcher>();
        var dispatcher2 = serviceProvider.GetService<IDispatcher>();
        
        Assert.Same(dispatcher1, dispatcher2);
    }

    [Fact]
    public void AddCommandHandler_RegistersCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandHandler<TestCommand, TestCommandHandler>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<ICommandHandler<TestCommand>>();
        
        Assert.NotNull(handler);
        Assert.IsType<TestCommandHandler>(handler);
    }

    [Fact]
    public void AddCommandHandler_WithDefaultLifetime_RegistersAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandHandler<TestCommand, TestCommandHandler>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddCommandHandler_WithSpecifiedLifetime_RegistersWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandHandler<TestCommand, TestCommandHandler>(ServiceLifetime.Singleton);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddCommandHandler_WithResult_RegistersCommandHandlerWithResult()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandHandler<TestCommandWithResult, string, TestCommandWithResultHandler>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<ICommandHandler<TestCommandWithResult, string>>();
        
        Assert.NotNull(handler);
        Assert.IsType<TestCommandWithResultHandler>(handler);
    }

    [Fact]
    public void AddQueryHandler_RegistersQueryHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddQueryHandler<TestQuery, string, TestQueryHandler>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IQueryHandler<TestQuery, string>>();
        
        Assert.NotNull(handler);
        Assert.IsType<TestQueryHandler>(handler);
    }

    [Fact]
    public void AddQueryHandler_WithDefaultLifetime_RegistersAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddQueryHandler<TestQuery, string, TestQueryHandler>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddQueryHandler_WithSpecifiedLifetime_RegistersWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddQueryHandler<TestQuery, string, TestQueryHandler>(ServiceLifetime.Transient);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void ChainedRegistrations_AllowsFluentConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services
            .AddCQS()
            .AddCommandHandler<TestCommand, TestCommandHandler>()
            .AddCommandHandler<TestCommandWithResult, string, TestCommandWithResultHandler>()
            .AddQueryHandler<TestQuery, string, TestQueryHandler>();

        // Assert
        Assert.Same(services, result); // Fluent interface returns same instance
        
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.NotNull(serviceProvider.GetService<IDispatcher>());
        Assert.NotNull(serviceProvider.GetService<ICommandHandler<TestCommand>>());
        Assert.NotNull(serviceProvider.GetService<ICommandHandler<TestCommandWithResult, string>>());
        Assert.NotNull(serviceProvider.GetService<IQueryHandler<TestQuery, string>>());
    }

    [Fact]
    public async Task FullIntegration_CanDispatchAfterRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddCQS()
            .AddCommandHandler<TestCommand, TestCommandHandler>()
            .AddCommandHandler<TestCommandWithResult, string, TestCommandWithResultHandler>()
            .AddQueryHandler<TestQuery, string, TestQueryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();

        // Act & Assert - Commands
        var command = new TestCommand("test");
        await dispatcher.DispatchCommandAsync(command);

        // Act & Assert - Commands with results
        var commandWithResult = new TestCommandWithResult("test");
        var result = await dispatcher.DispatchCommandAsync<TestCommandWithResult, string>(commandWithResult);
        Assert.Equal("Processed: test", result);

        // Act & Assert - Queries
        var query = new TestQuery("filter");
        var queryResult = await dispatcher.DispatchQueryAsync<TestQuery, string>(query);
        Assert.Equal("Query result for: filter", queryResult);
    }
}
