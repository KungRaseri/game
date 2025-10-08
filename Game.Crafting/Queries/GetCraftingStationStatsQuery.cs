#nullable enable

using Game.Core.CQS;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to get crafting station statistics.
/// </summary>
public record GetCraftingStationStatsQuery : IQuery<Dictionary<string, object>>
{
}
