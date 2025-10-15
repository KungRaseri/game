#nullable enable

using Game.Core.CQS;
using Game.Core.Extensions;
using Game.Core.Utils;
using Game.UI.Extensions;
using Godot;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Scripts.Managers;

/// <summary>
/// Main game manager that coordinates between all game systems and handles initialization.
/// This is the central hub for dependency injection and system coordination.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }
    
    private IServiceProvider? _serviceProvider;
    private IDispatcher? _dispatcher;
    
    [Signal]
    public delegate void GameInitializedEventHandler();
    
    [Signal]
    public delegate void GameShutdownEventHandler();

    public override void _Ready()
    {
        // Set up singleton instance
        if (Instance != null)
        {
            GameLogger.Error("GameManager: Multiple instances detected! Freeing duplicate.");
            QueueFree();
            return;
        }
        
        Instance = this;
        
        // Set up logging
        GameLogger.SetBackend(new GodotLoggerBackend());
        GameLogger.Info("GameManager: Initializing");
        
        // Initialize dependency injection
        InitializeDependencyInjection();
        
        GameLogger.Info("GameManager: Initialization complete");
        EmitSignal(SignalName.GameInitialized);
    }
    
    public override void _ExitTree()
    {
        GameLogger.Info("GameManager: Shutting down");
        EmitSignal(SignalName.GameShutdown);
        
        // Clean up singleton
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    /// <summary>
    /// Initializes the dependency injection container with all game services.
    /// </summary>
    private void InitializeDependencyInjection()
    {
        try
        {
            var services = new ServiceCollection();
            
            // Add core CQS services
            services.AddCQS();
            
            // Add all module services
            services.AddUIModule();
            // TODO: Add other modules as they're created
            // services.AddAdventureModule();
            // services.AddCraftingModule();
            // services.AddEconomyModule();
            
            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();
            _dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
            
            GameLogger.Info("GameManager: Dependency injection container initialized");
        }
        catch (System.Exception ex)
        {
            GameLogger.Error(ex, "GameManager: Failed to initialize dependency injection");
        }
    }
    
    /// <summary>
    /// Gets a service from the dependency injection container.
    /// </summary>
    public T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }
    
    /// <summary>
    /// Gets a required service from the dependency injection container.
    /// </summary>
    public T GetRequiredService<T>() where T : class
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("Service provider not initialized");
        }
        
        return _serviceProvider.GetRequiredService<T>();
    }
    
    /// <summary>
    /// Gets the CQS dispatcher for sending commands and queries.
    /// </summary>
    public IDispatcher? GetDispatcher()
    {
        return _dispatcher;
    }
    
    /// <summary>
    /// Dispatches a command through the CQS system.
    /// </summary>
    public async Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand
    {
        if (_dispatcher == null)
        {
            GameLogger.Warning("GameManager: No dispatcher available for command dispatch");
            return;
        }
        
        try
        {
            await _dispatcher.DispatchCommandAsync(command);
        }
        catch (System.Exception ex)
        {
            GameLogger.Error(ex, $"GameManager: Failed to dispatch command {typeof(TCommand).Name}");
            throw;
        }
    }
    
    /// <summary>
    /// Dispatches a query through the CQS system.
    /// </summary>
    public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query) 
        where TQuery : IQuery<TResult>
    {
        if (_dispatcher == null)
        {
            GameLogger.Warning("GameManager: No dispatcher available for query dispatch");
            throw new InvalidOperationException("Dispatcher not available");
        }
        
        try
        {
            return await _dispatcher.DispatchQueryAsync<TQuery, TResult>(query);
        }
        catch (System.Exception ex)
        {
            GameLogger.Error(ex, $"GameManager: Failed to dispatch query {query.GetType().Name}");
            throw;
        }
    }
}

/// <summary>
/// Autoload script that sets up the GameManager as a singleton.
/// This should be configured in project settings as an autoload.
/// </summary>
public partial class GameManagerAutoload : Node
{
    public override void _Ready()
    {
        // Create and add the GameManager to the scene tree
        var gameManager = new GameManager();
        GetTree().Root.AddChild(gameManager);
        
        GameLogger.Info("GameManagerAutoload: GameManager instance created and added to scene tree");
    }
}
