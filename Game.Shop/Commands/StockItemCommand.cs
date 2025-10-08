using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Shop.Commands;

/// <summary>
/// Command to stock an item in a shop display slot.
/// </summary>
public record StockItemCommand : ICommand<bool>
{
    /// <summary>
    /// The item to stock in the shop.
    /// </summary>
    public required Item Item { get; init; }

    /// <summary>
    /// The display slot to use (0-5).
    /// </summary>
    public required int SlotId { get; init; }

    /// <summary>
    /// The sale price for the item.
    /// </summary>
    public required decimal Price { get; init; }
}
