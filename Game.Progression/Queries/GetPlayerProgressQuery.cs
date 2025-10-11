#nullable enable

using Game.Core.CQS;
using Game.Progression.Models;

namespace Game.Progression.Queries;

/// <summary>
/// Query to get the current player progress.
/// </summary>
public record GetPlayerProgressQuery : IQuery<PlayerProgress>
{
}
