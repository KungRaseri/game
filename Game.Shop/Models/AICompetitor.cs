#nullable enable

using Game.Items.Models;

namespace Game.Shop.Models;

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