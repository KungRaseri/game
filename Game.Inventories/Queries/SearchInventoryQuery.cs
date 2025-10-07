#nullable enable

using Game.Core.CQS;
using Game.Inventories.Systems;

namespace Game.Inventories.Queries;

/// <summary>
/// Query to search inventory with criteria.
/// </summary>
public record SearchInventoryQuery : IQuery<InventorySearchResult>
{
    /// <summary>
    /// Search criteria for filtering inventory items.
    /// </summary>
    public InventorySearchCriteria Criteria { get; init; } = new();
}
