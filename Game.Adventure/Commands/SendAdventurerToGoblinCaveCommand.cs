#nullable enable

using Game.Core.CQS;

namespace Game.Adventure.Commands;

/// <summary>
/// Command to send an adventurer on an expedition to the Goblin Cave.
/// Contains 3 goblins that the adventurer must defeat.
/// </summary>
public record SendAdventurerToGoblinCaveCommand() : ICommand;