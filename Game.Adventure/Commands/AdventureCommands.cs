#nullable enable

using Game.Core.CQS;

namespace Game.Adventure.Commands;

/// <summary>
/// Command to send an adventurer on an expedition to the Goblin Cave.
/// Contains 3 goblins that the adventurer must defeat.
/// </summary>
public record SendAdventurerToGoblinCaveCommand() : ICommand;

/// <summary>
/// Command to force an adventurer to retreat from their current expedition.
/// Can be used when the adventurer is in danger or the player wants to abort.
/// </summary>
public record ForceAdventurerRetreatCommand() : ICommand;

/// <summary>
/// Command to update the adventurer's combat state.
/// Should be called regularly to process combat, movement, and regeneration.
/// </summary>
public record UpdateAdventurerStateCommand(float DeltaTime = 1.0f) : ICommand;

/// <summary>
/// Command to reset the combat system to idle state.
/// Useful for testing or when starting a new game session.
/// </summary>
public record ResetCombatSystemCommand() : ICommand;
