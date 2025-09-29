using Game.Main.Controllers;
using Game.Main.Systems;
using Godot;

namespace Game.Main.Managers
{
	/// <summary>
	/// Main game manager that coordinates all systems for Milestone 1
	/// </summary>
	public class GameManager
	{
		private readonly CombatSystem _combatSystem;
		private readonly AdventurerController _adventurerController;

		public AdventurerController AdventurerController => _adventurerController;
		public CombatSystem CombatSystem => _combatSystem;

		public GameManager()
		{
			_combatSystem = new CombatSystem();
			_adventurerController = new AdventurerController(_combatSystem);
		}

		/// <summary>
		/// Updates all game systems (should be called from _Process)
		/// </summary>
		public void Update()
		{
			_adventurerController.Update();
		}

		/// <summary>
		/// Initializes the game to starting state
		/// </summary>
		public void Initialize()
		{
			// Reset all systems to initial state
			GD.Print("Game initialized to starting state");
			_combatSystem.Reset();
		}

		public void Dispose()
		{
			_adventurerController?.Dispose();
		}
	}
}
