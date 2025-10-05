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
    private readonly AdventureSystem _adventureSystem;
    private readonly IDispatcher _dispatcher;
    private bool _disposed = false;

    public CombatSystem CombatSystem => _combatSystem;
    public AdventureSystem AdventureSystem => _adventureSystem;
    public IDispatcher Dispatcher => _dispatcher;

    public GameManager(IDispatcher dispatcher)
    {
        GameLogger.Info("Initializing GameManager");

        try
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _combatSystem = new CombatSystem();
            _adventureSystem = new AdventureSystem(_combatSystem);

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
            // Update the adventure system through the dispatch system if needed
            // The AdventureSystem itself doesn't need regular updates as it's event-driven
            // However, we can update the underlying CombatSystem if it needs periodic updates
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
            // Reset all systems to initial state
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
            _adventureSystem?.Dispose();
            _disposed = true;
            GameLogger.Info("GameManager disposal complete");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error during GameManager disposal");
        }
    }
}