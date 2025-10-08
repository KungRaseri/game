#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to get a specific crafting order by ID.
/// </summary>
public record GetCraftingOrderQuery : IQuery<CraftingOrder?>
{
    /// <summary>
    /// The ID of the order to retrieve.
    /// </summary>
    public string OrderId { get; init; } = string.Empty;
}
