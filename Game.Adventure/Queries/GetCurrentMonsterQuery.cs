using Game.Adventure.Models;
using Game.Core.CQS;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to get the current monster being fought.
/// Returns null if not in combat or no monster is active.
/// </summary>
public record GetCurrentMonsterQuery() : IQuery<CombatEntityStats?>;