#nullable enable

using Game.Items.Models;
using Game.Shop.Models;

namespace Game.Shop;

/// <summary>
/// Represents a completed sale transaction in the shop.
/// Contains all relevant information for analytics and record keeping.
/// </summary>
public record SaleTransaction(
    /// <summary>Unique identifier for this transaction.</summary>
    string TransactionId,
    /// <summary>The item that was sold.</summary>
    Item ItemSold,
    /// <summary>The actual sale price paid by the customer.</summary>
    decimal SalePrice,
    /// <summary>Estimated cost to create this item (materials + labor).</summary>
    decimal EstimatedCost,
    /// <summary>Profit margin as a percentage (e.g., 0.5 = 50% margin).</summary>
    decimal ProfitMargin,
    /// <summary>Identifier of the customer who made the purchase.</summary>
    string CustomerId,
    /// <summary>When the transaction was completed.</summary>
    DateTime TransactionTime,
    /// <summary>Customer satisfaction level with this transaction.</summary>
    CustomerSatisfaction CustomerSatisfaction
)
{
    /// <summary>
    /// Calculate the actual profit amount in gold.
    /// </summary>
    public decimal ProfitAmount => SalePrice - EstimatedCost;

    /// <summary>
    /// Get a human-readable summary of this transaction.
    /// </summary>
    public string GetSummary()
    {
        return $"{ItemSold.Name} sold for {SalePrice}g (profit: {ProfitAmount}g, {ProfitMargin:P1})";
    }

    /// <summary>
    /// Determine if this was a profitable transaction.
    /// </summary>
    public bool WasProfitable => ProfitAmount > 0;

    /// <summary>
    /// Get the transaction date (without time).
    /// </summary>
    public DateTime TransactionDate => TransactionTime.Date;
}