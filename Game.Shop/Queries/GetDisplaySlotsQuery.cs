using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get all display slots in the shop.
/// </summary>
public record GetDisplaySlotsQuery : IQuery<IEnumerable<ShopDisplaySlot>>
{
    /// <summary>
    /// Filter to only occupied slots.
    /// </summary>
    public bool OnlyOccupied { get; init; } = false;

    /// <summary>
    /// Filter to only available (empty) slots.
    /// </summary>
    public bool OnlyAvailable { get; init; } = false;
}
