using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to check if the adventurer is available for a new expedition.
/// Returns true if the adventurer is in Idle state.
/// </summary>
public record IsAdventurerAvailableQuery() : IQuery<bool>;