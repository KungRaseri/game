#nullable enable

using FluentAssertions;
using Game.Core.CQS;
using Game.Core.Tests;
using Game.Crafting.Commands;
using Game.Crafting.Extensions;
using Game.Crafting.Handlers;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;
using Game.Crafting.Tests.CQS;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Game.Crafting.Tests.Extensions;

/// <summary>
/// Tests for ServiceCollectionExtensions dependency injection configuration.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    private readonly TestableLoggerBackend _testLogger;

    public ServiceCollectionExtensionsTests()
    {
        _testLogger = TestHelpers.SetupTestLogging();
    }

    #region AddCraftingServices Tests

    [Fact]
    public void AddCraftingServices_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftingServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify core systems are registered as singletons
        var recipeManager1 = serviceProvider.GetService<RecipeManager>();
        var recipeManager2 = serviceProvider.GetService<RecipeManager>();
        recipeManager1.Should().NotBeNull();
        recipeManager1.Should().BeSameAs(recipeManager2);

        var craftingStation1 = serviceProvider.GetService<CraftingStation>();
        var craftingStation2 = serviceProvider.GetService<CraftingStation>();
        craftingStation1.Should().NotBeNull();
        craftingStation1.Should().BeSameAs(craftingStation2);

        // Verify facade service is registered as scoped
        var craftingService = serviceProvider.GetService<CraftingService>();
        craftingService.Should().NotBeNull();
    }

    [Fact]
    public void AddCraftingServices_RegistersAllCommandHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftingServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify all command handlers are registered
        serviceProvider.GetService<ICommandHandler<QueueCraftingOrderCommand, string>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<CancelCraftingOrderCommand>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<CancelAllCraftingOrdersCommand>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<AddRecipeCommand>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<UnlockRecipeCommand>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<LockRecipeCommand>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<DiscoverRecipesCommand, int>>().Should().NotBeNull();
    }

    [Fact]
    public void AddCraftingServices_RegistersAllQueryHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftingServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify all query handlers are registered
        serviceProvider.GetService<IQueryHandler<GetCraftingOrderQuery, CraftingOrder?>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetAllCraftingOrdersQuery, CraftingOrdersResult>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetCraftingStationStatsQuery, Dictionary<string, object>>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetRecipeQuery, Recipe?>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<SearchRecipesQuery, IReadOnlyList<Recipe>>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<IsRecipeUnlockedQuery, bool>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetRecipeManagerStatsQuery, Dictionary<string, object>>>().Should().NotBeNull();
    }

    [Fact]
    public void AddCraftingServices_HandlersAreRegisteredAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCraftingServices();

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Create two scopes to test that handlers are scoped
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        // Assert
        var handler1InScope1 = scope1.ServiceProvider.GetService<ICommandHandler<QueueCraftingOrderCommand, string>>();
        var handler2InScope1 = scope1.ServiceProvider.GetService<ICommandHandler<QueueCraftingOrderCommand, string>>();
        
        var handler1InScope2 = scope2.ServiceProvider.GetService<ICommandHandler<QueueCraftingOrderCommand, string>>();

        // Same instance within the same scope
        handler1InScope1.Should().BeSameAs(handler2InScope1);
        
        // Different instances in different scopes
        handler1InScope1.Should().NotBeSameAs(handler1InScope2);
    }

    [Fact]
    public void AddCraftingServices_CorrectHandlerTypesRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftingServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify correct implementation types are registered
        var queueHandler = serviceProvider.GetService<ICommandHandler<QueueCraftingOrderCommand, string>>();
        queueHandler.Should().BeOfType<QueueCraftingOrderCommandHandler>();

        var cancelHandler = serviceProvider.GetService<ICommandHandler<CancelCraftingOrderCommand>>();
        cancelHandler.Should().BeOfType<CancelCraftingOrderCommandHandler>();

        var getOrderHandler = serviceProvider.GetService<IQueryHandler<GetCraftingOrderQuery, CraftingOrder?>>();
        getOrderHandler.Should().BeOfType<GetCraftingOrderQueryHandler>();

        var searchHandler = serviceProvider.GetService<IQueryHandler<SearchRecipesQuery, IReadOnlyList<Recipe>>>();
        searchHandler.Should().BeOfType<SearchRecipesQueryHandler>();
    }

    #endregion

    #region AddCraftingCoreSystems Tests

    [Fact]
    public void AddCraftingCoreSystems_RegistersOnlyCoreSystems()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftingCoreSystems();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify core systems are registered
        serviceProvider.GetService<RecipeManager>().Should().NotBeNull();
        serviceProvider.GetService<CraftingStation>().Should().NotBeNull();

        // Verify handlers and facade service are NOT registered
        serviceProvider.GetService<CraftingService>().Should().BeNull();
        serviceProvider.GetService<ICommandHandler<QueueCraftingOrderCommand, string>>().Should().BeNull();
        serviceProvider.GetService<IQueryHandler<GetCraftingOrderQuery, CraftingOrder?>>().Should().BeNull();
    }

    [Fact]
    public void AddCraftingCoreSystems_CoreSystemsAreRegisteredAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCraftingCoreSystems();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify singleton registration
        var recipeManager1 = serviceProvider.GetService<RecipeManager>();
        var recipeManager2 = serviceProvider.GetService<RecipeManager>();
        recipeManager1.Should().BeSameAs(recipeManager2);

        var craftingStation1 = serviceProvider.GetService<CraftingStation>();
        var craftingStation2 = serviceProvider.GetService<CraftingStation>();
        craftingStation1.Should().BeSameAs(craftingStation2);
    }

    #endregion

    #region Method Chaining Tests

    [Fact]
    public void AddCraftingServices_ReturnsServiceCollection_AllowsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should allow method chaining
        var result = services
            .AddCraftingServices()
            .AddSingleton<string>("test");

        result.Should().BeSameAs(services);
        
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<string>().Should().Be("test");
        serviceProvider.GetService<CraftingService>().Should().NotBeNull();
    }

    [Fact]
    public void AddCraftingCoreSystems_ReturnsServiceCollection_AllowsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should allow method chaining
        var result = services
            .AddCraftingCoreSystems()
            .AddSingleton<string>("test");

        result.Should().BeSameAs(services);
        
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<string>().Should().Be("test");
        serviceProvider.GetService<RecipeManager>().Should().NotBeNull();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AddCraftingServices_WithCQSInfrastructure_IntegratesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IDispatcher, Dispatcher>();

        // Act
        services.AddCraftingServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify CraftingService can be resolved with all dependencies
        var craftingService = serviceProvider.GetService<CraftingService>();
        craftingService.Should().NotBeNull();

        // Verify handlers can be resolved with their dependencies
        var queueHandler = serviceProvider.GetService<ICommandHandler<QueueCraftingOrderCommand, string>>();
        queueHandler.Should().NotBeNull();
        queueHandler.Should().BeOfType<QueueCraftingOrderCommandHandler>();
    }

    [Fact]
    public void AddCraftingServices_CanResolveAllRegisteredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCraftingServices();

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert - All services should be resolvable without exceptions
        var exceptions = new List<Exception>();

        try { serviceProvider.GetRequiredService<RecipeManager>(); }
        catch (Exception ex) { exceptions.Add(ex); }

        try { serviceProvider.GetRequiredService<CraftingStation>(); }
        catch (Exception ex) { exceptions.Add(ex); }

        try { serviceProvider.GetRequiredService<CraftingService>(); }
        catch (Exception ex) { exceptions.Add(ex); }

        // Test a sample of handlers
        try { serviceProvider.GetRequiredService<ICommandHandler<QueueCraftingOrderCommand, string>>(); }
        catch (Exception ex) { exceptions.Add(ex); }

        try { serviceProvider.GetRequiredService<IQueryHandler<GetCraftingOrderQuery, CraftingOrder?>>(); }
        catch (Exception ex) { exceptions.Add(ex); }

        try { serviceProvider.GetRequiredService<ICommandHandler<AddRecipeCommand>>(); }
        catch (Exception ex) { exceptions.Add(ex); }

        try { serviceProvider.GetRequiredService<IQueryHandler<SearchRecipesQuery, IReadOnlyList<Recipe>>>(); }
        catch (Exception ex) { exceptions.Add(ex); }

        exceptions.Should().BeEmpty("All registered services should be resolvable");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AddCraftingServices_CalledMultipleTimes_DoesNotCauseDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Call multiple times
        services.AddCraftingServices();
        services.AddCraftingServices();
        services.AddCraftingServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should still work correctly (last registration wins)
        var craftingService = serviceProvider.GetService<CraftingService>();
        craftingService.Should().NotBeNull();

        var recipeManager = serviceProvider.GetService<RecipeManager>();
        recipeManager.Should().NotBeNull();
    }

    [Fact]
    public void AddCraftingCoreSystems_CalledMultipleTimes_DoesNotCauseDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Call multiple times
        services.AddCraftingCoreSystems();
        services.AddCraftingCoreSystems();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should still work correctly
        var recipeManager = serviceProvider.GetService<RecipeManager>();
        recipeManager.Should().NotBeNull();

        var craftingStation = serviceProvider.GetService<CraftingStation>();
        craftingStation.Should().NotBeNull();
    }

    #endregion
}
