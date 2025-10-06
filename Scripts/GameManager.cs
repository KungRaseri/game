using Game.Core.CQS;
using Game.Adventure.Systems;
using Game.Adventure.Commands;
using Game.Adventure.Queries;
using Game.Adventure.Models;
using Game.Core.Utils;

namespace Game.Scripts;

/// <summary>
/// Main game manager that coordinates all systems for Milestone 1
/// Updated to use CQS pattern instead of direct controller access
/// Ready to be converted to Godot Node when UI integration is implemented
/// </summary>
public class GameManager : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly IDispatcher _dispatcher;
    private bool _disposed = false;

    public CombatSystem CombatSystem => _combatSystem;
    public IDispatcher Dispatcher => _dispatcher;

    public GameManager(IDispatcher dispatcher)
    {
        GameLogger.Info("Initializing GameManager");

        try
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            
            // Get the CombatSystem from DI instead of creating a new instance
            // This ensures we use the same instance that the CQS handlers use
            _combatSystem = Game.DI.DependencyInjectionNode.GetService<CombatSystem>();

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
            // Since we're using the same CombatSystem instance from DI,
            // we can update it directly for better performance in the game loop
            _combatSystem.Update(fixedDeltaTime);
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
            // Since we're using the same CombatSystem instance from DI,
            // we can reset it directly for initialization
            _combatSystem.Reset();
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