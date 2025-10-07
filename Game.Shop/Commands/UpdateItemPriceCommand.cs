using Game.Core.CQS;

namespace Game.Shop.Commands;

/// <summary>
/// Command to update the price of an item in a display slot.
/// </summary>
public record UpdateItemPriceCommand : ICommand<bool>
{
    /// <summary>
    /// The display slot containing the item to update.
    /// </summary>
    public required int SlotId { get; init; }

    /// <summary>
    /// The new price for the item.
    /// </summary>
    public required decimal NewPrice { get; init; }
}
