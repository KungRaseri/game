#nullable enable

using Game.Core.Data.Interfaces;
using Game.Core.Data.Services;
using Game.Items.Data.Models;
using Game.Items.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Items.Tests.Integration;

/// <summary>
/// Simple integration test to verify JSON data loading works
/// </summary>
public class JsonDataLoadingTests
{
    [Fact]
    public async Task ItemDataService_LoadMaterials_ReturnsConfigurationsFromJson()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient(typeof(IDataLoader<>), typeof(JsonDataLoader<>));
        services.AddSingleton<HotReloadService>(); // Add hot-reload service for dependency injection
        services.AddScoped<ItemDataService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var itemDataService = serviceProvider.GetRequiredService<ItemDataService>();

        // Act
        var materials = await itemDataService.GetAllMaterialConfigsAsync();

        // Assert
        Assert.NotEmpty(materials);
        Assert.Contains(materials, m => m.ItemId == "iron_ore");
        Assert.Contains(materials, m => m.ItemId == "material_oak_wood");
    }

    [Fact]
    public async Task ItemCreationService_CreateWeapon_CreatesWeaponFromJsonConfig()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient(typeof(IDataLoader<>), typeof(JsonDataLoader<>));
        services.AddSingleton<HotReloadService>(); // Add hot-reload service for dependency injection
        services.AddScoped<ItemDataService>();
        services.AddScoped<ItemCreationService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var itemCreationService = serviceProvider.GetRequiredService<ItemCreationService>();

        // Act
        var weapon = await itemCreationService.CreateWeaponAsync("weapon_iron_sword");

        // Assert
        Assert.Equal("Iron Sword", weapon.Name);
        Assert.Equal("weapon_iron_sword_common", weapon.ItemId);
        Assert.True(weapon.DamageBonus > 0);
    }

    [Fact]
    public async Task ItemDataService_GetMaterialConfig_ReturnsSpecificMaterial()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient(typeof(IDataLoader<>), typeof(JsonDataLoader<>));
        services.AddSingleton<HotReloadService>(); // Add hot-reload service for dependency injection
        services.AddScoped<ItemDataService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var itemDataService = serviceProvider.GetRequiredService<ItemDataService>();

        // Act
        var ironOre = await itemDataService.GetMaterialConfigAsync("iron_ore");

        // Assert
        Assert.NotNull(ironOre);
        Assert.Equal("Iron Ore", ironOre.Name);
        Assert.Equal(2, ironOre.BaseValue);
    }
}
