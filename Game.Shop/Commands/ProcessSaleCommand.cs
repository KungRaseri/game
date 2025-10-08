using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Commands;

/// <summary>
/// Command to process a sale transaction.
/// </summary>
public record ProcessSaleCommand : ICommand<SaleTransaction?>
{
    /// <summary>
    /// The display slot containing the purchased item.
    /// </summary>
    public required int SlotId { get; init; }

    /// <summary>
    /// The customer making the purchase.
    /// </summary>
    public required string CustomerId { get; init; }

    /// <summary>
    /// Customer satisfaction with the transaction.
    /// </summary>
    public required CustomerSatisfaction Satisfaction { get; init; }
}
