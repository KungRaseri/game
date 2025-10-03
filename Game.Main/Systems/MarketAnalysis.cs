using Game.Items.Models;

namespace Game.Main.Systems;

/// <summary>
/// Market analysis results for a specific item type and quality.
/// </summary>
public class MarketAnalysis
{
    public ItemType ItemType { get; init; }
    public QualityTier Quality { get; init; }
    public double DemandLevel { get; init; }
    public double SupplyLevel { get; init; }
    public double PriceMultiplier { get; init; }
    public string MarketTrend { get; init; } = string.Empty;
    public int RecentSales { get; init; }
    public double AverageTimeToSell { get; init; }
    public string RecommendedStrategy { get; init; } = string.Empty;
    public string CompetitorPosition { get; init; } = string.Empty;
}