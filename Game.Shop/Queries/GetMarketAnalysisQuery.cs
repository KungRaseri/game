using Game.Core.CQS;
using Game.Items.Models;
using Game.Shop.Systems;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get market analysis for a specific item type and quality.
/// </summary>
public record GetMarketAnalysisQuery : IQuery<MarketAnalysis>
{
    /// <summary>
    /// The type of item to analyze.
    /// </summary>
    public required ItemType ItemType { get; init; }

    /// <summary>
    /// The quality tier to analyze.
    /// </summary>
    public required QualityTier Quality { get; init; }
}
