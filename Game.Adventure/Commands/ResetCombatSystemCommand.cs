using Game.Core.CQS;

namespace Game.Adventure.Commands;

/// <summary>
/// Command to reset the combat system to idle state.
/// Useful for testing or when starting a new game session.
/// </summary>
public record ResetCombatSystemCommand() : ICommand;