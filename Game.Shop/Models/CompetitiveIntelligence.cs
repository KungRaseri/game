#nullable enable

using Game.Item.Models;

namespace Game.Shop.Models;

/// <summary>
/// Provides competitive intelligence data for a specific item category.
/// Used for pricing analysis and market positioning decisions.
/// </summary>
public class CompetitiveIntelligence
{
    /// <summary>
    /// The item type being analyzed.
    /// </summary>
    public ItemType ItemType { get; init; }
    
    /// <summary>
    /// The quality tier being analyzed.
    /// </summary>
    public QualityTier QualityTier { get; init; }
    
    /// <summary>
    /// Our current price for this item.
    /// </summary>
    public decimal OurPrice { get; init; }
    
    /// <summary>
    /// List of competitor prices for the same item category.
    /// </summary>
    public List<decimal> CompetitorPrices { get; init; } = new();
    
    /// <summary>
    /// Names of the competitors offering this item.
    /// </summary>
    public List<string> CompetitorNames { get; init; } = new();
    
    /// <summary>
    /// Average price among all competitors.
    /// </summary>
    public decimal AverageCompetitorPrice { get; init; }
    
    /// <summary>
    /// Lowest competitor price in the market.
    /// </summary>
    public decimal LowestCompetitorPrice { get; init; }
    
    /// <summary>
    /// Highest competitor price in the market.
    /// </summary>
    public decimal HighestCompetitorPrice { get; init; }
    
    /// <summary>
    /// Our price advantage/disadvantage compared to average competitor price.
    /// Positive values indicate we're cheaper than average.
    /// Negative values indicate we're more expensive than average.
    /// </summary>
    public decimal PriceAdvantage { get; init; }
    
    /// <summary>
    /// Our market position relative to competitors.
    /// </summary>
    public MarketPosition MarketPosition { get; init; }
    
    /// <summary>
    /// Number of competitors offering this item category.
    /// </summary>
    public int CompetitorCount => CompetitorPrices.Count;
    
    /// <summary>
    /// Whether we have competitive pricing (within 10% of average).
    /// </summary>
    public bool IsCompetitivelyPriced => Math.Abs(PriceAdvantage) <= 0.1m;
    
    /// <summary>
    /// Whether we're the lowest priced option.
    /// </summary>
    public bool IsLowestPriced => CompetitorCount > 0 && OurPrice <= LowestCompetitorPrice;
    
    /// <summary>
    /// Whether we're the highest priced option.
    /// </summary>
    public bool IsHighestPriced => CompetitorCount > 0 && OurPrice >= HighestCompetitorPrice;
    
    /// <summary>
    /// Pricing recommendation based on competitive analysis.
    /// </summary>
    public string GetPricingRecommendation()
    {
        if (CompetitorCount == 0)
        {
            return "No competition detected - consider premium pricing";
        }
        
        return MarketPosition switch
        {
            MarketPosition.Premium => "Consider slight price reduction to increase volume",
            MarketPosition.AboveAverage => "Well positioned for quality-conscious customers",
            MarketPosition.Average => "Competitive positioning - monitor competitor changes",
            MarketPosition.BelowAverage => "Good value positioning - consider volume strategies",
            MarketPosition.Discount => "Aggressive pricing - ensure profitability",
            MarketPosition.Monopoly => "Market leader - optimize for maximum profit",
            _ => "Review pricing strategy"
        };
    }
    
    /// <summary>
    /// Get market share estimate based on pricing position.
    /// </summary>
    public double EstimatedMarketShare()
    {
        if (CompetitorCount == 0) return 1.0;
        
        return MarketPosition switch
        {
            MarketPosition.Premium => 0.15,
            MarketPosition.AboveAverage => 0.25,
            MarketPosition.Average => 0.30,
            MarketPosition.BelowAverage => 0.35,
            MarketPosition.Discount => 0.45,
            MarketPosition.Monopoly => 1.0,
            _ => 0.20
        };
    }
}