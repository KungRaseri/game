using Game.Adventure.Models;
using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to get the current adventurer's stats.
/// Returns null if no adventurer is currently active.
/// </summary>
public record GetCurrentAdventurerQuery() : IQuery<CombatEntityStats?>;