namespace Game.Shop.Models;

/// <summary>
/// Historical price entry for market analysis.
/// </summary>
public record PriceHistoryEntry(
    DateTime Date,
    decimal Price,
    decimal OriginalPrice,
    double PriceRatio,
    CustomerSatisfaction CustomerSatisfaction
);