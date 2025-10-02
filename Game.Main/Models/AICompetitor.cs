#nullable enable

using System;
using System.Collections.Generic;

namespace Game.Main.Models;

/// <summary>
/// Represents an AI competitor shop with its own pricing strategy and market behavior.
/// </summary>
public class AICompetitor
{
    /// <summary>Unique identifier for this competitor.</summary>
    public required string CompetitorId { get; init; }
    
    /// <summary>Display name of the competitor shop.</summary>
    public required string Name { get; init; }
    
    /// <summary>Overall business strategy of this competitor.</summary>
    public CompetitorStrategy Strategy { get; set; }
    
    /// <summary>How aggressively this competitor reacts to market changes (0.0-1.0).</summary>
    public double Aggressiveness { get; set; }
    
    /// <summary>Current performance score affecting their market success (0.0-1.0).</summary>
    public double PerformanceScore { get; set; }
    
    /// <summary>How this competitor manages their inventory.</summary>
    public required InventoryStrategy InventoryStrategy { get; init; }
    
    /// <summary>Pricing modifier applied to base prices (0.5-2.0).</summary>
    public double PricingModifier { get; set; }
    
    /// <summary>Current prices for items this competitor sells.</summary>
    public Dictionary<(ItemType, QualityTier), decimal> ItemPrices { get; init; } = new();
    
    /// <summary>Historical actions taken by this competitor.</summary>
    public List<CompetitorAction> ActionHistory { get; init; } = new();
    
    /// <summary>When this competitor was last updated.</summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Checks if this competitor sells a specific item type and quality.
    /// </summary>
    public bool SellsItem(ItemType itemType, QualityTier quality)
    {
        return ItemPrices.ContainsKey((itemType, quality));
    }
    
    /// <summary>
    /// Gets the competitor's price for a specific item, if they sell it.
    /// </summary>
    public decimal? GetPriceFor(ItemType itemType, QualityTier quality)
    {
        return ItemPrices.TryGetValue((itemType, quality), out var price) ? price : null;
    }
    
    /// <summary>
    /// Updates the competitor's strategy based on market performance.
    /// </summary>
    public void UpdateStrategy(CompetitorStrategy newStrategy)
    {
        Strategy = newStrategy;
        LastUpdated = DateTime.Now;
    }
    
    /// <summary>
    /// Records a new action taken by this competitor.
    /// </summary>
    public void RecordAction(CompetitorAction action)
    {
        ActionHistory.Add(action);
        LastUpdated = DateTime.Now;
        
        // Limit history size to prevent memory growth
        if (ActionHistory.Count > 50)
        {
            ActionHistory.RemoveAt(0);
        }
    }
}

/// <summary>
/// Business strategies that AI competitors can employ.
/// </summary>
public enum CompetitorStrategy
{
    /// <summary>Focus on lowest prices to attract price-sensitive customers.</summary>
    LowPrice,
    
    /// <summary>Emphasize quality over price, targeting quality-conscious customers.</summary>
    QualityFocused,
    
    /// <summary>Position as premium brand with high prices and luxury appeal.</summary>
    Premium,
    
    /// <summary>Offer bulk discounts and volume pricing.</summary>
    VolumeDiscount,
    
    /// <summary>Specialize in specific item types or qualities.</summary>
    Specialized
}

/// <summary>
/// How competitors manage their inventory and stocking decisions.
/// </summary>
public enum InventoryStrategy
{
    /// <summary>Keep diverse inventory across all item types.</summary>
    Diversified,
    
    /// <summary>Focus on fast-moving, popular items.</summary>
    FastMoving,
    
    /// <summary>Specialize in high-quality, rare items.</summary>
    HighEnd,
    
    /// <summary>Stock based on seasonal demand patterns.</summary>
    Seasonal,
    
    /// <summary>Reactive stocking based on competitor actions.</summary>
    Reactive
}

/// <summary>
/// Comprehensive analysis of the competitive market landscape.
/// </summary>
public class CompetitionAnalysis
{
    /// <summary>Total number of active competitors in the market.</summary>
    public int TotalCompetitors { get; init; }
    
    /// <summary>Average performance score of all competitors.</summary>
    public double AverageCompetitorPerformance { get; init; }
    
    /// <summary>Player's estimated market share (0.0-1.0).</summary>
    public double MarketShare { get; init; }
    
    /// <summary>Overall competitive pressure in the market (0.0-1.0).</summary>
    public double CompetitivePressure { get; init; }
    
    /// <summary>Identified competitive threats requiring attention.</summary>
    public List<string> Threats { get; init; } = new();
    
    /// <summary>Market opportunities to exploit.</summary>
    public List<string> Opportunities { get; init; } = new();
    
    /// <summary>Recommended actions based on competitive analysis.</summary>
    public List<string> RecommendedActions { get; init; } = new();
    
    /// <summary>When this analysis was generated.</summary>
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
    
    /// <summary>
    /// Gets a summary description of the competitive landscape.
    /// </summary>
    public string GetCompetitiveLandscapeDescription()
    {
        var pressure = CompetitivePressure switch
        {
            <= 0.3 => "Low",
            <= 0.6 => "Moderate", 
            <= 0.8 => "High",
            _ => "Intense"
        };
        
        var marketPosition = MarketShare switch
        {
            <= 0.1 => "Niche player",
            <= 0.25 => "Growing presence",
            <= 0.5 => "Strong competitor",
            <= 0.75 => "Market leader",
            _ => "Dominant position"
        };
        
        return $"{pressure} competitive pressure, {marketPosition} ({MarketShare:P1} market share)";
    }
}

/// <summary>
/// Represents a competitor's reaction to player actions or market changes.
/// </summary>
public class CompetitorReaction
{
    /// <summary>Which competitor is reacting.</summary>
    public required string CompetitorId { get; init; }
    
    /// <summary>Item type being affected by the reaction.</summary>
    public ItemType ItemType { get; init; }
    
    /// <summary>Quality tier being affected.</summary>
    public QualityTier QualityTier { get; init; }
    
    /// <summary>Type of reaction being taken.</summary>
    public CompetitorReactionType ReactionType { get; set; }
    
    /// <summary>Price multiplier applied as part of the reaction.</summary>
    public double PriceMultiplier { get; set; } = 1.0;
    
    /// <summary>When this reaction occurred.</summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;
    
    /// <summary>Description of why this reaction occurred.</summary>
    public string? Reason { get; init; }
}

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

/// <summary>
/// Represents an action taken by an AI competitor.
/// </summary>
public class CompetitorAction
{
    /// <summary>Which competitor took this action.</summary>
    public required string CompetitorId { get; init; }
    
    /// <summary>Type of action taken.</summary>
    public required CompetitorActionType ActionType { get; init; }
    
    /// <summary>When the action was taken.</summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;
    
    /// <summary>Description of the action.</summary>
    public required string Description { get; init; }
    
    /// <summary>Items affected by this action, if applicable.</summary>
    public List<(ItemType, QualityTier)> AffectedItems { get; init; } = new();
    
    /// <summary>Magnitude of the action's impact (0.0-1.0).</summary>
    public double ImpactMagnitude { get; init; } = 0.5;
}

/// <summary>
/// Types of actions AI competitors can take.
/// </summary>
public enum CompetitorActionType
{
    /// <summary>Restocking inventory with popular items.</summary>
    InventoryRestock,
    
    /// <summary>Reducing prices on select items.</summary>
    PriceReduction,
    
    /// <summary>Starting promotional campaigns.</summary>
    Promotion,
    
    /// <summary>Improving item quality or service.</summary>
    QualityUpgrade,
    
    /// <summary>Expanding into new product categories.</summary>
    MarketExpansion,
    
    /// <summary>Enhancing customer service and experience.</summary>
    CustomerService,
    
    /// <summary>Adjusting business strategy.</summary>
    StrategyChange,
    
    /// <summary>Entering new market segments.</summary>
    MarketEntry,
    
    /// <summary>Exiting unprofitable markets.</summary>
    MarketExit
}
