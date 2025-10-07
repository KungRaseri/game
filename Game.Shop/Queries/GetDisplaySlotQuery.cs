using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get a specific display slot information.
/// </summary>
public record GetDisplaySlotQuery : IQuery<ShopDisplaySlot?>
{
    /// <summary>
    /// The display slot ID to retrieve.
    /// </summary>
    public required int SlotId { get; init; }
}
