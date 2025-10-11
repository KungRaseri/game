#nullable enable

using Game.Core.CQS;
using Game.Progression.Models;

namespace Game.Progression.Queries;

/// <summary>
/// Query to get the current game phase.
/// </summary>
public record GetCurrentGamePhaseQuery : IQuery<GamePhase>
{
}
