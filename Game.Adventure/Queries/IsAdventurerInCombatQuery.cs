using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to check if the adventurer is currently in combat.
/// </summary>
public record IsAdventurerInCombatQuery() : IQuery<bool>;