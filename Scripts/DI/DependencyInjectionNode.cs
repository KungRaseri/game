#nullable enable

using Game.Adventure.Extensions;
using Game.UI.Extensions;
using Game.Gathering.Extensions;
using Game.Shop.Extensions;
using Game.Inventories.Extensions;
using Game.Crafting.Extensions;
using Game.Core.Extensions;
using Game.Core.Utils;
using Godot;
using Microsoft.Extensions.DependencyInjection;

namespace Game.DI;

/// <summary>
/// Custom dependency injection manager for Godot that integrates with our CQS pattern.
/// This provides a simpler, more reliable alternative to the Godot.DependencyInjection package.
/// Add this node to Project → Project Settings → Autoload.
/// </summary>
public partial class DependencyInjectionNode : Node
{
    private static ServiceProvider? _serviceProvider;
    private static IServiceCollection? _services;

    /// <summary>
    /// Gets the current service provider. Throws if DI hasn't been initialized.
    /// </summary>
    public static ServiceProvider ServiceProvider =>
        _serviceProvider ?? throw new InvalidOperationException("Dependency injection not initialized. Ensure DependencyInjectionNode is in autoload.");

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    public static T GetService<T>() where T : notnull =>
        ServiceProvider.GetRequiredService<T>();

    /// <summary>
    /// Tries to get a service of the specified type.
    /// </summary>
    public static T? GetOptionalService<T>() where T : class =>
        ServiceProvider.GetService<T>();

    public override void _Ready()
    {
        GD.Print("Initializing custom dependency injection...");
        GameLogger.SetBackend(new GodotLoggerBackend());

        _services = new ServiceCollection();
        ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();

        GD.Print("Dependency injection initialized successfully!");
    }

    public override void _ExitTree()
    {
        _serviceProvider?.Dispose();
        _serviceProvider = null;
        _services = null;
    }

    /// <summary>
    /// Configure all services for the application.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        GameLogger.Debug("[DI] Starting service configuration...");

        // Add CQS infrastructure
        services.AddCQS();

        // Add Adventure module services
        GameLogger.Debug("[DI] Registering Adventure module...");
        services.AddAdventureModule();

        GameLogger.Debug("[DI] Registering UI module...");
        services.AddUIModule();

        GameLogger.Debug("[DI] Registering Gathering module...");
        services.AddGatheringModule();

        GameLogger.Debug("[DI] Registering Shop module (includes ShopKeeper state management)...");
        services.AddShopServices();

        GameLogger.Debug("[DI] Registering Inventories module...");
        services.AddInventoryServices();

        GameLogger.Debug("[DI] Registering Crafting module...");
        services.AddCraftingServices();

        // GameLogger.Debug("[DI] Registering Progression module...");
        // services.AddProgressionServices(); // TODO: Enable when Game.Progression is properly integrated

        // Add other game services here as needed
        // services.AddScoped<IAdventurerService, AdventurerService>();
        // services.AddScoped<IShopService, ShopService>();
        // etc.

        GameLogger.Debug($"[DI] Service configuration completed - Registered {services.Count} services");
        GD.Print($"Registered {services.Count} services");
    }
}
