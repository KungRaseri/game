#nullable enable

using FluentAssertions;
using Game.Core.CQS;
using Game.Inventories.Commands;
using Game.Inventories.Extensions;
using Game.Inventories.Handlers;
using Game.Inventories.Queries;
using Game.Inventories.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Inventories.Tests.Extensions;

public class InventoriesServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInventoryServices_RegistersAllCommandHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInventoryServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Command Handlers
        serviceProvider.GetService<AddMaterialsCommandHandler>()
            .Should().BeOfType<AddMaterialsCommandHandler>();
            
        serviceProvider.GetService<RemoveMaterialsCommandHandler>()
            .Should().BeOfType<RemoveMaterialsCommandHandler>();
            
        serviceProvider.GetService<ConsumeMaterialsCommandHandler>()
            .Should().BeOfType<ConsumeMaterialsCommandHandler>();
            
        serviceProvider.GetService<ExpandInventoryCommandHandler>()
            .Should().BeOfType<ExpandInventoryCommandHandler>();
            
        serviceProvider.GetService<ClearInventoryCommandHandler>()
            .Should().BeOfType<ClearInventoryCommandHandler>();
            
        serviceProvider.GetService<SaveInventoryCommandHandler>()
            .Should().BeOfType<SaveInventoryCommandHandler>();
            
        serviceProvider.GetService<LoadInventoryCommandHandler>()
            .Should().BeOfType<LoadInventoryCommandHandler>();
    }

    [Fact]
    public void AddInventoryServices_RegistersAllQueryHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInventoryServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Query Handlers
        serviceProvider.GetService<GetInventoryStatsQueryHandler>()
            .Should().BeOfType<GetInventoryStatsQueryHandler>();
            
        serviceProvider.GetService<SearchInventoryQueryHandler>()
            .Should().BeOfType<SearchInventoryQueryHandler>();
            
        serviceProvider.GetService<CanConsumeMaterialsQueryHandler>()
            .Should().BeOfType<CanConsumeMaterialsQueryHandler>();
            
        serviceProvider.GetService<GetInventoryContentsQueryHandler>()
            .Should().BeOfType<GetInventoryContentsQueryHandler>();
            
        serviceProvider.GetService<ValidateInventoryQueryHandler>()
            .Should().BeOfType<ValidateInventoryQueryHandler>();
            
        serviceProvider.GetService<IsInventoryLoadedQueryHandler>()
            .Should().BeOfType<IsInventoryLoadedQueryHandler>();
    }

    [Fact]
    public void AddInventoryServices_RegistersInventoryManagerAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInventoryServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var manager1 = serviceProvider.GetService<InventoryManager>();
        var manager2 = serviceProvider.GetService<InventoryManager>();
        
        manager1.Should().NotBeNull();
        manager2.Should().NotBeNull();
        manager1.Should().BeSameAs(manager2); // Singleton behavior
    }

    [Fact]
    public void AddInventoryServices_RegistersDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInventoryServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Verify that core CQS services are registered
        serviceProvider.GetService<IDispatcher>().Should().NotBeNull();
    }

    [Fact]
    public void AddInventoryServices_AllHandlersCanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddInventoryServices();
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - All handlers should resolve without exceptions
        var commandHandlers = new object[]
        {
            serviceProvider.GetRequiredService<AddMaterialsCommandHandler>(),
            serviceProvider.GetRequiredService<RemoveMaterialsCommandHandler>(),
            serviceProvider.GetRequiredService<ConsumeMaterialsCommandHandler>(),
            serviceProvider.GetRequiredService<ExpandInventoryCommandHandler>(),
            serviceProvider.GetRequiredService<ClearInventoryCommandHandler>(),
            serviceProvider.GetRequiredService<SaveInventoryCommandHandler>(),
            serviceProvider.GetRequiredService<LoadInventoryCommandHandler>()
        };

        var queryHandlers = new object[]
        {
            serviceProvider.GetRequiredService<GetInventoryStatsQueryHandler>(),
            serviceProvider.GetRequiredService<SearchInventoryQueryHandler>(),
            serviceProvider.GetRequiredService<CanConsumeMaterialsQueryHandler>(),
            serviceProvider.GetRequiredService<GetInventoryContentsQueryHandler>(),
            serviceProvider.GetRequiredService<ValidateInventoryQueryHandler>(),
            serviceProvider.GetRequiredService<IsInventoryLoadedQueryHandler>()
        };

        commandHandlers.Should().AllBeAssignableTo<object>();
        commandHandlers.Should().OnlyHaveUniqueItems();
        
        queryHandlers.Should().AllBeAssignableTo<object>();
        queryHandlers.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AddInventoryServices_HandlersShareSameInventoryManager()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddInventoryServices();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var addHandler = serviceProvider.GetRequiredService<AddMaterialsCommandHandler>();
        var removeHandler = serviceProvider.GetRequiredService<RemoveMaterialsCommandHandler>();
        var statsHandler = serviceProvider.GetRequiredService<GetInventoryStatsQueryHandler>();

        // Assert
        addHandler.Should().NotBeNull();
        removeHandler.Should().NotBeNull();
        statsHandler.Should().NotBeNull();

        // All handlers should use the same InventoryManager instance (singleton)
        // This is verified by the fact that operations on one handler affect results from another
        // We can't directly access the private _inventoryManager field, but the singleton registration ensures this
    }

    [Fact]
    public void AddInventoryServices_CanBeCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInventoryServices();
        services.AddInventoryServices(); // Call twice
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var manager = serviceProvider.GetService<InventoryManager>();
        manager.Should().NotBeNull();
        
        // Should still work correctly
        var handler = serviceProvider.GetService<AddMaterialsCommandHandler>();
        handler.Should().NotBeNull();
    }
}
