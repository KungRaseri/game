using FluentAssertions;
using Game.Core.CQS;
using Game.Items.Commands;
using Game.Items.Extensions;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Queries;
using Game.Items.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Items.Tests.Extensions;

/// <summary>
/// Tests for ItemsServiceCollectionExtensions to ensure proper service registration.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddItemsServices_ShouldRegisterAllCommandHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ICommandHandler<CreateWeaponCommand, Weapon>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<CreateArmorCommand, Armor>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<CreateMaterialCommand, Material>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<GenerateLootCommand, List<Drop>>>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<CalculateItemValueCommand, int>>().Should().NotBeNull();
    }

    [Fact]
    public void AddItemsServices_ShouldRegisterAllQueryHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IQueryHandler<GetWeaponConfigsQuery, IEnumerable<Data.WeaponConfig>>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetArmorConfigsQuery, IEnumerable<Data.ArmorConfig>>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetMaterialConfigsQuery, IEnumerable<Data.MaterialConfig>>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetLootStatisticsQuery, Dictionary<string, float>>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<GetQualityTierModifiersQuery, Models.QualityTierModifierResult>>().Should().NotBeNull();
        serviceProvider.GetService<IQueryHandler<CheckLootTableExistsQuery, bool>>().Should().NotBeNull();
    }

    [Fact]
    public void AddItemsServices_ShouldRegisterLootGenerator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var lootGenerator = serviceProvider.GetService<LootGenerator>();
        lootGenerator.Should().NotBeNull();
        lootGenerator.Should().BeOfType<LootGenerator>();
    }

    [Fact]
    public void AddItemsServices_ShouldRegisterLootGeneratorAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var lootGenerator1 = serviceProvider.GetService<LootGenerator>();
        var lootGenerator2 = serviceProvider.GetService<LootGenerator>();

        lootGenerator1.Should().BeSameAs(lootGenerator2);
    }

    [Fact]
    public void AddItemsServices_ShouldRegisterCommandHandlersAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var handler1a = scope1.ServiceProvider.GetService<ICommandHandler<CreateWeaponCommand, Weapon>>();
        var handler1b = scope1.ServiceProvider.GetService<ICommandHandler<CreateWeaponCommand, Weapon>>();
        var handler2 = scope2.ServiceProvider.GetService<ICommandHandler<CreateWeaponCommand, Weapon>>();

        handler1a.Should().BeSameAs(handler1b); // Same within scope
        handler1a.Should().NotBeSameAs(handler2); // Different across scopes
    }

    [Fact]
    public void AddItemsServices_ShouldRegisterQueryHandlersAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var handler1a = scope1.ServiceProvider.GetService<IQueryHandler<GetWeaponConfigsQuery, IEnumerable<Data.WeaponConfig>>>();
        var handler1b = scope1.ServiceProvider.GetService<IQueryHandler<GetWeaponConfigsQuery, IEnumerable<Data.WeaponConfig>>>();
        var handler2 = scope2.ServiceProvider.GetService<IQueryHandler<GetWeaponConfigsQuery, IEnumerable<Data.WeaponConfig>>>();

        handler1a.Should().BeSameAs(handler1b); // Same within scope
        handler1a.Should().NotBeSameAs(handler2); // Different across scopes
    }

    [Fact]
    public void AddItemsServices_ShouldRegisterCorrectHandlerImplementations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ICommandHandler<CreateWeaponCommand, Weapon>>().Should().BeOfType<CreateWeaponCommandHandler>();
        serviceProvider.GetService<ICommandHandler<CreateArmorCommand, Armor>>().Should().BeOfType<CreateArmorCommandHandler>();
        serviceProvider.GetService<ICommandHandler<CreateMaterialCommand, Material>>().Should().BeOfType<CreateMaterialCommandHandler>();
        serviceProvider.GetService<ICommandHandler<GenerateLootCommand, List<Drop>>>().Should().BeOfType<GenerateLootCommandHandler>();
        serviceProvider.GetService<ICommandHandler<CalculateItemValueCommand, int>>().Should().BeOfType<CalculateItemValueCommandHandler>();

        serviceProvider.GetService<IQueryHandler<GetWeaponConfigsQuery, IEnumerable<Data.WeaponConfig>>>().Should().BeOfType<GetWeaponConfigsQueryHandler>();
        serviceProvider.GetService<IQueryHandler<GetArmorConfigsQuery, IEnumerable<Data.ArmorConfig>>>().Should().BeOfType<GetArmorConfigsQueryHandler>();
        serviceProvider.GetService<IQueryHandler<GetMaterialConfigsQuery, IEnumerable<Data.MaterialConfig>>>().Should().BeOfType<GetMaterialConfigsQueryHandler>();
        serviceProvider.GetService<IQueryHandler<GetLootStatisticsQuery, Dictionary<string, float>>>().Should().BeOfType<GetLootStatisticsQueryHandler>();
        serviceProvider.GetService<IQueryHandler<GetQualityTierModifiersQuery, Models.QualityTierModifierResult>>().Should().BeOfType<GetQualityTierModifiersQueryHandler>();
        serviceProvider.GetService<IQueryHandler<CheckLootTableExistsQuery, bool>>().Should().BeOfType<CheckLootTableExistsQueryHandler>();
    }

    [Fact]
    public void AddItemsServices_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddItemsServices();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddItemsServices_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        services.AddItemsServices(); // Should not throw
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<LootGenerator>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<CreateWeaponCommand, Weapon>>().Should().NotBeNull();
    }

    [Fact]
    public void AddItemsServices_ShouldCreateWorkingLootGenerator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var lootGenerator = serviceProvider.GetService<LootGenerator>();
        lootGenerator.Should().NotBeNull();

        // Test that the loot generator can be used
        var statistics = lootGenerator!.GetDropStatistics("nonexistent");
        statistics.Should().NotBeNull();
        statistics.Should().BeEmpty(); // No loot table configured for this monster
    }

    [Fact]
    public void AddItemsServices_HandlersShouldHaveCorrectDependencies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Handlers that require LootGenerator should get it injected
        var lootCommandHandler = serviceProvider.GetService<ICommandHandler<GenerateLootCommand, List<Drop>>>();
        lootCommandHandler.Should().NotBeNull();
        lootCommandHandler.Should().BeOfType<GenerateLootCommandHandler>();

        var lootStatisticsHandler = serviceProvider.GetService<IQueryHandler<GetLootStatisticsQuery, Dictionary<string, float>>>();
        lootStatisticsHandler.Should().NotBeNull();
        lootStatisticsHandler.Should().BeOfType<GetLootStatisticsQueryHandler>();

        var checkLootTableHandler = serviceProvider.GetService<IQueryHandler<CheckLootTableExistsQuery, bool>>();
        checkLootTableHandler.Should().NotBeNull();
        checkLootTableHandler.Should().BeOfType<CheckLootTableExistsQueryHandler>();
    }

    [Fact]
    public void AddItemsServices_ShouldIntegrateWithExistingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestService>();

        // Act
        services.AddItemsServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<TestService>().Should().NotBeNull();
        serviceProvider.GetService<LootGenerator>().Should().NotBeNull();
        serviceProvider.GetService<ICommandHandler<CreateWeaponCommand, Weapon>>().Should().NotBeNull();
    }

    private class TestService
    {
        // Simple test service for integration testing
    }
}
