using Game.Core.CQS;
using Game.Adventure.Systems;
using Game.Adventure.Commands;
using Game.Adventure.Queries;
using Game.Adventure.Models;
using Game.Core.Utils;
using Game.UI.Systems;

namespace Game.Scripts.Core;

/// <summary>
/// Main game manager that coordinates all systems for Milestone 1
/// Updated to use CQS pattern instead of direct controller access
/// Ready to be converted to Godot Node when UI integration is implemented
/// </summary>
public class GameManager : IDisposable
{
    private readonly AdventureSystem _adventureSystem;
    private readonly UISystem _uiSystem;
    private readonly IDispatcher _dispatcher;
    private bool _disposed = false;

    public AdventureSystem AdventureSystem => _adventureSystem;
    public UISystem UISystem => _uiSystem;
    public IDispatcher Dispatcher => _dispatcher;

    public GameManager(IDispatcher dispatcher)
    {
        GameLogger.Info("Initializing GameManager");

        try
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            // Get the AdventureSystem from DI instead of creating a new instance
            // This ensures we use the same instance that the CQS handlers use
            _adventureSystem = Game.Scripts.DI.DependencyInjectionNode.GetService<AdventureSystem>();

            // Get the UISystem from DI
            _uiSystem = Game.Scripts.DI.DependencyInjectionNode.GetService<UISystem>();

            GameLogger.Info("GameManager initialization complete");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to initialize GameManager");
            throw;
        }
    }

    /// <summary>
    /// Updates all game systems (should be called from _Process)
    /// </summary>
    public void Update()
    {
        Update(1.0f); // Default to 1 second for backward compatibility
    }

    /// <summary>
    /// Updates all game systems with fixed time step
    /// </summary>
    public void Update(float fixedDeltaTime)
    {
        if (_disposed)
        {
            GameLogger.Warning("Update called on disposed GameManager");
            return;
        }

        try
        {
            _adventureSystem.CombatSystem.Update(fixedDeltaTime);
            _uiSystem.Update(); // Update UI system for toast cleanup
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error during game update");
        }
    }

    /// <summary>
    /// Initializes the game to starting state
    /// </summary>
    public void Initialize()
    {
        if (_disposed)
        {
            GameLogger.Warning("Initialize called on disposed GameManager");
            return;
        }

        GameLogger.Info("Initializing game to starting state");

        try
        {
            _adventureSystem.CombatSystem.Reset();
            GameLogger.Info("Game successfully initialized to starting state");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to initialize game");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        GameLogger.Info("Disposing GameManager");

        try
        {
            // Dispose UI system
            _uiSystem?.Dispose();
            
            // The CombatSystem is managed by DI, no need to dispose it here
            _disposed = true;
            GameLogger.Info("GameManager disposal complete");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error during GameManager disposal");
        }
    }
}