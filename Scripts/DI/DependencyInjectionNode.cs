#nullable enable

using Game.Adventure.Extensions;
using Game.Core.CQS;
using Game.Core.Extensions;
using Game.Core.Utils;
using Game.DI.Examples;
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
        
        // Test the integration
        TestIntegration();
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

        // Register example services
        GameLogger.Debug("[DI] Registering game services...");
        services.AddScoped<IGameService, GameService>();
        
        // Register CQS handlers
        GameLogger.Debug("[DI] Registering CQS handlers...");
        services.AddCommandHandler<LogGameEventCommand, LogGameEventCommandHandler>();
        services.AddQueryHandler<GetGameStatusQuery, GameStatus, GetGameStatusQueryHandler>();

        // Add other game services here
        // services.AddScoped<IAdventurerService, AdventurerService>();
        // services.AddScoped<IShopService, ShopService>();
        // etc.
        
        GameLogger.Debug($"[DI] Service configuration completed - Registered {services.Count} services");
        GD.Print($"Registered {services.Count} services");
    }

    /// <summary>
    /// Test that the DI integration works correctly.
    /// </summary>
    private static async void TestIntegration()
    {
        try
        {
            var gameService = GetService<IGameService>();
            await gameService.LogEventAsync("DITest", "Dependency injection integration test successful!");
            
            var status = await gameService.GetStatusAsync();
            GD.Print($"CQS Integration test - Status: {status}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"DI Integration test failed: {ex.Message}");
        }
    }
}
