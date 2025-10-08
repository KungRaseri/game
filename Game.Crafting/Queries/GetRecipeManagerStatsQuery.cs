#nullable enable

using Game.Core.CQS;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to get recipe manager statistics.
/// </summary>
public record GetRecipeManagerStatsQuery : IQuery<Dictionary<string, object>>
{
}
