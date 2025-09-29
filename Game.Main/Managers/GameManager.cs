using Game.Main.Controllers;
using Game.Main.Systems;
using Game.Main.Utils;
using System;

namespace Game.Main.Managers
{
	/// <summary>
	/// Main game manager that coordinates all systems for Milestone 1
	/// </summary>
	public class GameManager : IDisposable
	{
		private readonly CombatSystem _combatSystem;
		private readonly AdventurerController _adventurerController;
		private readonly GameLogger _logger;
		private bool _disposed = false;

		public AdventurerController AdventurerController => _adventurerController;
		public CombatSystem CombatSystem => _combatSystem;
		public GameLogger Logger => _logger;

		public GameManager()
		{
			_logger = new GameLogger();
			_logger.System("Initializing GameManager");

			try
			{
				_combatSystem = new CombatSystem();
				_adventurerController = new AdventurerController(_combatSystem);

				_logger.System("GameManager initialization complete");
			}
			catch (Exception ex)
			{
				_logger.Error("Failed to initialize GameManager", ex);
				throw;
			}
		}

		/// <summary>
		/// Updates all game systems (should be called from _Process)
		/// </summary>
		public void Update()
		{
			if (_disposed)
			{
				_logger.Warn("Update called on disposed GameManager");
				return;
			}

			try
			{
				_adventurerController.Update();
			}
			catch (Exception ex)
			{
				_logger.Error("Error during game update", ex);
			}
		}

		/// <summary>
		/// Initializes the game to starting state
		/// </summary>
		public void Initialize()
		{
			if (_disposed)
			{
				_logger.Warn("Initialize called on disposed GameManager");
				return;
			}

			_logger.System("Initializing game to starting state");

			try
			{
				// Reset all systems to initial state
				_combatSystem.Reset();
				_logger.Info("Game successfully initialized to starting state");
			}
			catch (Exception ex)
			{
				_logger.Error("Failed to initialize game", ex);
				throw;
			}
		}

		public void Dispose()
		{
			if (_disposed) return;

			_logger.System("Disposing GameManager");

			try
			{
				_adventurerController?.Dispose();
				_disposed = true;
				_logger.System("GameManager disposal complete");
			}
			catch (Exception ex)
			{
				_logger.Error("Error during GameManager disposal", ex);
			}
		}
	}
}
