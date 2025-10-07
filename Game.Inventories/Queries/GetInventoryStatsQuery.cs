#nullable enable

using Game.Core.CQS;
using Game.Inventories.Models;

namespace Game.Inventories.Queries;

/// <summary>
/// Query to get current inventory statistics.
/// </summary>
public record GetInventoryStatsQuery : IQuery<InventoryStats>
{
}
