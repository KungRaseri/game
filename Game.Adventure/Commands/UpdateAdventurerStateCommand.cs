using Game.Core.CQS;

namespace Game.Adventure.Commands;

/// <summary>
/// Command to update the adventurer's combat state.
/// Should be called regularly to process combat, movement, and regeneration.
/// </summary>
public record UpdateAdventurerStateCommand(float DeltaTime = 1.0f) : ICommand;