#nullable enable

using System;
using Game.Main.Models;

namespace Game.Main.Models;

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

/// <summary>
/// Customer satisfaction levels for transactions and shop experience.
/// </summary>
public enum LegacyCustomerSatisfaction
{
    /// <summary>Customer was very unhappy with the transaction.</summary>
    VeryUnsatisfied = 1,
    
    /// <summary>Customer was somewhat disappointed.</summary>
    Unsatisfied = 2,
    
    /// <summary>Customer had a neutral experience.</summary>
    Neutral = 3,
    
    /// <summary>Customer was pleased with the transaction.</summary>
    Satisfied = 4,
    
    /// <summary>Customer was very happy and likely to return.</summary>
    VerySatisfied = 5
}

/// <summary>
/// Extension methods for CustomerSatisfaction enum.
/// </summary>
public static class CustomerSatisfactionExtensions
{
    /// <summary>
    /// Get a descriptive string for the satisfaction level.
    /// </summary>
    public static string GetDescription(this CustomerSatisfaction satisfaction)
    {
        return satisfaction switch
        {
            CustomerSatisfaction.Angry => "Very Disappointed",
            CustomerSatisfaction.Disappointed => "Disappointed", 
            CustomerSatisfaction.Neutral => "Neutral",
            CustomerSatisfaction.Satisfied => "Happy",
            CustomerSatisfaction.Delighted => "Delighted",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get a numeric score for analytics (1-5 scale).
    /// </summary>
    public static int GetScore(this CustomerSatisfaction satisfaction)
    {
        return (int)satisfaction;
    }
    
    /// <summary>
    /// Determine if this represents a positive experience.
    /// </summary>
    public static bool IsPositive(this CustomerSatisfaction satisfaction)
    {
        return satisfaction >= CustomerSatisfaction.Satisfied;
    }
}
