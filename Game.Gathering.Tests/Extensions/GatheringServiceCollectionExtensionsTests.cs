#nullable enable

using Game.Core.CQS;
using Game.Gathering.Commands;
using Game.Gathering.Extensions;
using Game.Gathering.Handlers;
using Game.Gathering.Systems;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Game.Gathering.Tests.Extensions;

/// <summary>
/// Tests for the GatheringServiceCollectionExtensions.
/// </summary>
public class GatheringServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGatheringModule_RegistersGatheringSystem()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>());

        // Act
        services.AddGatheringModule();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var gatheringSystem = serviceProvider.GetService<GatheringSystem>();
        Assert.NotNull(gatheringSystem);
    }

    [Fact]
    public void AddGatheringModule_RegistersGatherMaterialsCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>());

        // Act
        services.AddGatheringModule();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();
        Assert.NotNull(handler);
        Assert.IsType<GatherMaterialsCommandHandler>(handler);
    }

    [Fact]
    public void AddGatheringModule_GatheringSystemIsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>());

        // Act
        services.AddGatheringModule();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();
        
        var system1 = scope1.ServiceProvider.GetService<GatheringSystem>();
        var system2 = scope2.ServiceProvider.GetService<GatheringSystem>();
        
        Assert.NotNull(system1);
        Assert.NotNull(system2);
        Assert.NotSame(system1, system2); // Different instances in different scopes
    }

    [Fact]
    public void AddGatheringModule_HandlerIsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>());

        // Act
        services.AddGatheringModule();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();
        
        var handler1 = scope1.ServiceProvider.GetService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();
        var handler2 = scope2.ServiceProvider.GetService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();
        
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.NotSame(handler1, handler2); // Different instances in different scopes
    }

    [Fact]
    public void AddGatheringModule_CanBeCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>());

        // Act
        services.AddGatheringModule();
        services.AddGatheringModule(); // Second call

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var gatheringSystem = serviceProvider.GetService<GatheringSystem>();
        var handler = serviceProvider.GetService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>();
        
        Assert.NotNull(gatheringSystem);
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddGatheringModule_WithExistingServices_DoesNotConflict()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>());
        services.AddScoped<string>(_ => "test"); // Add unrelated service

        // Act
        services.AddGatheringModule();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var gatheringSystem = serviceProvider.GetService<GatheringSystem>();
        var testString = serviceProvider.GetService<string>();
        
        Assert.NotNull(gatheringSystem);
        Assert.Equal("test", testString);
    }

    [Fact]
    public void AddGatheringModule_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGatheringModule();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddGatheringModule_AllowsFluentChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        services
            .AddGatheringModule()
            .AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>())
            .AddGatheringModule(); // Can chain and call again
    }

    [Fact]
    public void AddGatheringModule_RegisteredServicesCanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher>(_ => Mock.Of<IDispatcher>());
        services.AddGatheringModule();

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert - All registered services should be resolvable
        Assert.NotNull(serviceProvider.GetService<IGatheringSystem>());
        Assert.NotNull(serviceProvider.GetService<GatheringSystem>());
        Assert.NotNull(serviceProvider.GetService<ICommandHandler<GatherMaterialsCommand, GatherMaterialsResult>>());
        Assert.NotNull(serviceProvider.GetService<GatherMaterialsCommandHandler>());
    }

    [Fact]
    public void AddGatheringModule_WithMissingDependency_ThrowsOnResolution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGatheringModule(); // Missing IDispatcher dependency

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IGatheringSystem>());
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<GatheringSystem>());
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<GatherMaterialsCommandHandler>());
    }
}
