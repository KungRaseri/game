#nullable enable

namespace Game.Core.Models;

/// <summary>
/// Represents market data for a specific item type including demand, supply, and pricing history.
/// </summary>
public class MarketData
{
    public ItemType ItemType { get; init; }
    public QualityTier QualityTier { get; init; }
    public double DemandLevel { get; set; } = 1.0; // 1.0 = normal, >1.0 = high demand, <1.0 = low demand
    public double SupplyLevel { get; set; } = 1.0; // 1.0 = normal, >1.0 = oversupply, <1.0 = shortage
    public MarketCondition CurrentCondition { get; set; } = MarketCondition.Normal;
    public SeasonalFactor CurrentSeason { get; set; } = SeasonalFactor.Spring;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public List<PriceHistoryEntry> PriceHistory { get; init; } = new();
    public int RecentSalesCount { get; set; } = 0; // Sales in the last 24 hours
    public double AverageTimeToSell { get; set; } = 12.0; // Hours to sell on average
    public double CompetitorPriceMultiplier { get; set; } = 1.0; // Relative to our prices
    
    /// <summary>
    /// Calculate the current market price multiplier based on demand, supply, and conditions.
    /// </summary>
    public double GetPriceMultiplier()
    {
        var baseMultiplier = DemandLevel / SupplyLevel;
        
        // Apply market condition adjustments
        var conditionMultiplier = CurrentCondition switch
        {
            MarketCondition.HighDemand => 1.3,
            MarketCondition.LowDemand => 0.7,
            MarketCondition.Oversupply => 0.5,
            MarketCondition.Shortage => 2.0,
            MarketCondition.SeasonalHigh => 1.5,
            MarketCondition.SeasonalLow => 0.6,
            MarketCondition.EconomicBoom => 1.4,
            MarketCondition.EconomicRecession => 0.8,
            _ => 1.0
        };
        
        // Apply seasonal adjustments
        var seasonalMultiplier = GetSeasonalMultiplier();
        
        // Apply competitor pricing pressure
        var competitorAdjustment = Math.Max(0.5, Math.Min(1.5, CompetitorPriceMultiplier));
        
        return baseMultiplier * conditionMultiplier * seasonalMultiplier * competitorAdjustment;
    }
    
    /// <summary>
    /// Get seasonal multiplier based on item type and current season.
    /// </summary>
    private double GetSeasonalMultiplier()
    {
        return (CurrentSeason, ItemType) switch
        {
            (SeasonalFactor.Summer, ItemType.Weapon) => 1.2, // More adventures
            (SeasonalFactor.Summer, ItemType.Armor) => 1.2,
            (SeasonalFactor.Winter, ItemType.Weapon) => 0.8, // Fewer adventures
            (SeasonalFactor.Winter, ItemType.Armor) => 0.8,
            (SeasonalFactor.Festival, ItemType.Consumable) => 1.4, // Festival preparations
            (SeasonalFactor.Wartime, ItemType.Weapon) => 1.8, // High demand for combat gear
            (SeasonalFactor.Wartime, ItemType.Armor) => 1.8,
            (SeasonalFactor.Peacetime, ItemType.Weapon) => 0.7, // Low demand for combat gear
            (SeasonalFactor.Peacetime, ItemType.Armor) => 0.7,
            _ => 1.0
        };
    }
    
    /// <summary>
    /// Update market data based on a recent sale.
    /// </summary>
    public void RecordSale(decimal salePrice, decimal originalPrice, CustomerSatisfaction satisfaction)
    {
        // Add to price history
        PriceHistory.Add(new PriceHistoryEntry(
            Date: DateTime.Now,
            Price: salePrice,
            OriginalPrice: originalPrice,
            PriceRatio: originalPrice > 0 ? (double)(salePrice / originalPrice) : 1.0,
            CustomerSatisfaction: satisfaction
        ));
        
        // Keep only recent history (last 30 entries)
        if (PriceHistory.Count > 30)
        {
            PriceHistory.RemoveAt(0);
        }
        
        RecentSalesCount++;
        LastUpdated = DateTime.Now;
        
        // Adjust demand based on sale success and customer satisfaction
        if (satisfaction >= CustomerSatisfaction.Satisfied)
        {
            DemandLevel = Math.Min(2.0, DemandLevel + 0.05); // Increase demand slightly
        }
        else if (satisfaction == CustomerSatisfaction.Disappointed)
        {
            DemandLevel = Math.Max(0.3, DemandLevel - 0.03); // Decrease demand slightly
        }
    }
    
    /// <summary>
    /// Update market data when an item sits unsold for a period.
    /// </summary>
    public void RecordTimePassed(TimeSpan timePassed)
    {
        var hoursPassed = timePassed.TotalHours;
        
        // If items are sitting unsold, reduce demand
        if (hoursPassed > AverageTimeToSell * 1.5)
        {
            DemandLevel = Math.Max(0.3, DemandLevel - 0.02);
        }
        
        // Reset recent sales count if a day has passed
        if (hoursPassed > 24)
        {
            RecentSalesCount = 0;
        }
        
        LastUpdated = DateTime.Now;
    }
    
    /// <summary>
    /// Get market trend analysis.
    /// </summary>
    public string GetMarketTrend()
    {
        if (PriceHistory.Count < 3) return "Insufficient data";
        
        var recentPrices = PriceHistory.TakeLast(5).Select(p => p.PriceRatio).ToList();
        var averageRecent = recentPrices.Average();
        
        return averageRecent switch
        {
            > 1.2 => "Strongly Rising",
            > 1.1 => "Rising",
            > 0.9 => "Stable",
            > 0.8 => "Declining",
            _ => "Strongly Declining"
        };
    }
}