namespace Game.Shop.Models;

/// <summary>
/// Types of reactions competitors can have to market changes.
/// </summary>
public enum CompetitorReactionType
{
    /// <summary>No reaction to the market change.</summary>
    NoChange,

    /// <summary>Reduce prices to undercut competition.</summary>
    UnderCut,

    /// <summary>Match competitor pricing.</summary>
    MatchPrice,

    /// <summary>Increase prices following market trends.</summary>
    PriceIncrease,

    /// <summary>Launch a promotional campaign.</summary>
    Promotion,

    /// <summary>Offer bulk discounts.</summary>
    BulkDiscount,

    /// <summary>Focus on quality improvements.</summary>
    QualityFocus,

    /// <summary>Expand inventory in response to demand.</summary>
    InventoryExpansion
}