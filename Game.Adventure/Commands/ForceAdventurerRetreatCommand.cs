using Game.Core.CQS;

namespace Game.Adventure.Commands;

/// <summary>
/// Command to force an adventurer to retreat from their current expedition.
/// Can be used when the adventurer is in danger or the player wants to abort.
/// </summary>
public record ForceAdventurerRetreatCommand() : ICommand;