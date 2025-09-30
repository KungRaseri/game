using Game.Main.Controllers;
using Game.Main.Systems;
using Game.Main.Utils;
using System;

namespace Game.Main.Managers;

/// <summary>
/// Main game manager that coordinates all systems for Milestone 1
/// Ready to be converted to Godot Node when UI integration is implemented
/// </summary>
public class GameManager : IDisposable
{
	private readonly CombatSystem _combatSystem;
	private readonly AdventurerController _adventurerController;
	private bool _disposed = false;

	public AdventurerController AdventurerController => _adventurerController;
	public CombatSystem CombatSystem => _combatSystem;

	public GameManager()
	{
		GameLogger.Info("Initializing GameManager");
		
		try
		{
			_combatSystem = new CombatSystem();
			_adventurerController = new AdventurerController(_combatSystem);
			
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
			_adventurerController.Update(fixedDeltaTime);
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
			_adventurerController?.Dispose();
			_disposed = true;
			GameLogger.Info("GameManager disposal complete");
		}
		catch (Exception ex)
		{
			GameLogger.Error(ex, "Error during GameManager disposal");
		}
	}
}
