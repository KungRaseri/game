#nullable enable

using Game.Items.Models;

namespace Game.Main.Systems;

/// <summary>
/// Dynamic pricing engine that adjusts item prices based on market conditions,
/// demand, supply, customer behavior, and competitive analysis.
/// </summary>
public class PricingEngine
{
    private readonly Dictionary<(ItemType, QualityTier), MarketData> _marketData = new();
    private readonly Dictionary<ItemType, PricingStrategy> _itemStrategies = new();
    private readonly Random _random = new();
    
    // Configuration parameters
    public double MaxPriceIncrease { get; set; } = 2.0; // 200% of base price
    public double MinPriceDecrease { get; set; } = 0.3; // 30% of base price
    public double PriceVolatility { get; set; } = 0.1; // Random price variance
    public bool EnableCompetitorSimulation { get; set; } = true;
    public double CustomerSensitivityFactor { get; set; } = 1.0;
    
    public PricingEngine()
    {
        InitializeDefaultStrategies();
    }
    
    /// <summary>
    /// Calculate the optimal price for an item based on market conditions.
    /// </summary>
    public decimal CalculateOptimalPrice(Items item, decimal basePrice)
    {
        var marketData = GetOrCreateMarketData(item.ItemType, item.Quality);
        var strategy = GetPricingStrategy(item.ItemType);
        
        // Get base market multiplier
        var marketMultiplier = marketData.GetPriceMultiplier();
        
        // Apply strategy-specific adjustments
        var strategyMultiplier = GetStrategyMultiplier(strategy, marketData);
        
        // Apply quality premium
        var qualityMultiplier = GetQualityMultiplier(item.Quality);
        
        // Add some controlled randomness for market volatility
        var volatilityFactor = 1.0 + (_random.NextDouble() - 0.5) * PriceVolatility;
        
        // Calculate final price
        var finalMultiplier = marketMultiplier * strategyMultiplier * qualityMultiplier * volatilityFactor;
        
        // Apply min/max constraints
        finalMultiplier = Math.Max(MinPriceDecrease, Math.Min(MaxPriceIncrease, finalMultiplier));
        
        var optimalPrice = basePrice * (decimal)finalMultiplier;
        
        // Round to reasonable increments
        return RoundToReasonablePrice(optimalPrice);
    }
    
    /// <summary>
    /// Get pricing strategy multiplier based on current market conditions.
    /// </summary>
    private double GetStrategyMultiplier(PricingStrategy strategy, MarketData marketData)
    {
        return strategy switch
        {
            PricingStrategy.Premium => 1.3 + (marketData.DemandLevel - 1.0) * 0.2,
            PricingStrategy.Competitive => 1.0 + (marketData.CompetitorPriceMultiplier - 1.0) * 0.8,
            PricingStrategy.Discount => 0.8 - (marketData.SupplyLevel - 1.0) * 0.1,
            PricingStrategy.Dynamic => 1.0, // Already handled by market multiplier
            PricingStrategy.Seasonal => marketData.CurrentSeason switch
            {
                SeasonalFactor.Festival => 1.2,
                SeasonalFactor.Wartime => 1.1,
                SeasonalFactor.Winter => 0.9,
                _ => 1.0
            },
            PricingStrategy.Penetration => 0.7,
            PricingStrategy.Skimming => 1.8 - Math.Max(0, marketData.RecentSalesCount * 0.05),
            _ => 1.0
        };
    }
    
    /// <summary>
    /// Get quality tier pricing multiplier.
    /// </summary>
    private double GetQualityMultiplier(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.Common => 1.0,
            QualityTier.Uncommon => 1.3,
            QualityTier.Rare => 1.8,
            QualityTier.Epic => 2.5,
            QualityTier.Legendary => 4.0,
            _ => 1.0
        };
    }
    
    /// <summary>
    /// Predict customer satisfaction based on pricing.
    /// </summary>
    public CustomerSatisfaction PredictCustomerSatisfaction(Items item, decimal proposedPrice, decimal basePrice)
    {
        var marketData = GetOrCreateMarketData(item.ItemType, item.Quality);
        var priceRatio = proposedPrice / basePrice;
        
        // Adjust satisfaction based on customer sensitivity and market conditions
        var adjustedRatio = (double)priceRatio * CustomerSensitivityFactor;
        
        // Consider market conditions - customers expect higher prices in high demand
        if (marketData.CurrentCondition == MarketCondition.HighDemand)
        {
            adjustedRatio *= 0.9; // More tolerant of high prices
        }
        else if (marketData.CurrentCondition == MarketCondition.LowDemand)
        {
            adjustedRatio *= 1.2; // Less tolerant of high prices
        }
        
        return adjustedRatio switch
        {
            <= 0.8 => CustomerSatisfaction.Delighted,
            <= 1.0 => CustomerSatisfaction.Satisfied,
            <= 1.3 => CustomerSatisfaction.Neutral,
            <= 1.7 => CustomerSatisfaction.Disappointed,
            _ => CustomerSatisfaction.Angry
        };
    }
    
    /// <summary>
    /// Record a sale and update market data.
    /// </summary>
    public void RecordSale(Items item, decimal salePrice, CustomerSatisfaction satisfaction)
    {
        RecordSale(item, salePrice, salePrice, satisfaction);
    }
    
    /// <summary>
    /// Record a sale and update market data.
    /// </summary>
    public void RecordSale(Items item, decimal salePrice, decimal originalPrice, CustomerSatisfaction satisfaction)
    {
        var marketData = GetOrCreateMarketData(item.ItemType, item.Quality);
        marketData.RecordSale(salePrice, originalPrice, satisfaction);
        
        // Update competitor pricing simulation
        if (EnableCompetitorSimulation)
        {
            UpdateCompetitorPricing(marketData, salePrice, originalPrice);
        }
    }
    
    /// <summary>
    /// Update market conditions periodically.
    /// </summary>
    public void UpdateMarketConditions(TimeSpan timePassed)
    {
        foreach (var marketData in _marketData.Values)
        {
            marketData.RecordTimePassed(timePassed);
            
            // Simulate market evolution
            SimulateMarketEvolution(marketData, timePassed);
            
            // Update competitor behavior
            if (EnableCompetitorSimulation)
            {
                SimulateCompetitorBehavior(marketData);
            }
        }
    }
    
    /// <summary>
    /// Set pricing strategy for a specific item type.
    /// </summary>
    public void SetPricingStrategy(ItemType itemType, PricingStrategy strategy)
    {
        _itemStrategies[itemType] = strategy;
    }
    
    /// <summary>
    /// Get market analysis for an item type.
    /// </summary>
    public MarketAnalysis GetMarketAnalysis(ItemType itemType, QualityTier quality)
    {
        var marketData = GetOrCreateMarketData(itemType, quality);
        
        return new MarketAnalysis
        {
            ItemType = itemType,
            Quality = quality,
            DemandLevel = marketData.DemandLevel,
            SupplyLevel = marketData.SupplyLevel,
            PriceMultiplier = marketData.GetPriceMultiplier(),
            MarketTrend = marketData.GetMarketTrend(),
            RecentSales = marketData.RecentSalesCount,
            AverageTimeToSell = marketData.AverageTimeToSell,
            RecommendedStrategy = GetRecommendedStrategy(marketData),
            CompetitorPosition = GetCompetitorAnalysis(marketData)
        };
    }
    
    /// <summary>
    /// Get or create market data for item type and quality.
    /// </summary>
    private MarketData GetOrCreateMarketData(ItemType itemType, QualityTier quality)
    {
        var key = (itemType, quality);
        if (!_marketData.ContainsKey(key))
        {
            _marketData[key] = new MarketData
            {
                ItemType = itemType,
                QualityTier = quality
            };
        }
        return _marketData[key];
    }
    
    /// <summary>
    /// Get pricing strategy for item type.
    /// </summary>
    private PricingStrategy GetPricingStrategy(ItemType itemType)
    {
        return _itemStrategies.GetValueOrDefault(itemType, PricingStrategy.Dynamic);
    }
    
    /// <summary>
    /// Round price to reasonable increments.
    /// </summary>
    private decimal RoundToReasonablePrice(decimal price)
    {
        return price switch
        {
            < 10 => Math.Round(price, 2),      // Round to cents
            < 100 => Math.Round(price, 1),     // Round to 10 cents
            < 1000 => Math.Round(price, 0),    // Round to dollars
            _ => Math.Round(price / 5, 0) * 5  // Round to $5 increments
        };
    }
    
    /// <summary>
    /// Initialize default pricing strategies for different item types.
    /// </summary>
    private void InitializeDefaultStrategies()
    {
        _itemStrategies[ItemType.Weapon] = PricingStrategy.Premium;
        _itemStrategies[ItemType.Armor] = PricingStrategy.Premium;
        _itemStrategies[ItemType.Consumable] = PricingStrategy.Competitive;
        _itemStrategies[ItemType.Material] = PricingStrategy.Discount;
    }
    
    /// <summary>
    /// Simulate market evolution over time.
    /// </summary>
    private void SimulateMarketEvolution(MarketData marketData, TimeSpan timePassed)
    {
        var hours = timePassed.TotalHours;
        
        // Gradual return to normal demand/supply levels
        var normalizeRate = 0.01 * hours;
        
        if (marketData.DemandLevel > 1.0)
        {
            marketData.DemandLevel = Math.Max(1.0, marketData.DemandLevel - normalizeRate);
        }
        else if (marketData.DemandLevel < 1.0)
        {
            marketData.DemandLevel = Math.Min(1.0, marketData.DemandLevel + normalizeRate);
        }
        
        if (marketData.SupplyLevel > 1.0)
        {
            marketData.SupplyLevel = Math.Max(1.0, marketData.SupplyLevel - normalizeRate);
        }
        else if (marketData.SupplyLevel < 1.0)
        {
            marketData.SupplyLevel = Math.Min(1.0, marketData.SupplyLevel + normalizeRate);
        }
    }
    
    /// <summary>
    /// Simulate competitor pricing behavior.
    /// </summary>
    private void SimulateCompetitorBehavior(MarketData marketData)
    {
        // Competitors react to market conditions
        var targetMultiplier = marketData.GetPriceMultiplier() * 0.95; // Slightly undercut market
        
        // Gradual adjustment towards target
        var adjustmentRate = 0.1;
        marketData.CompetitorPriceMultiplier += 
            (targetMultiplier - marketData.CompetitorPriceMultiplier) * adjustmentRate;
    }
    
    /// <summary>
    /// Update competitor pricing based on our sales.
    /// </summary>
    private void UpdateCompetitorPricing(MarketData marketData, decimal salePrice, decimal originalPrice)
    {
        var priceRatio = (double)(salePrice / originalPrice);
        
        // If we're selling at high prices successfully, competitors may raise prices
        if (priceRatio > 1.2)
        {
            marketData.CompetitorPriceMultiplier = Math.Min(1.5, 
                marketData.CompetitorPriceMultiplier + 0.02);
        }
        // If we're discounting heavily, competitors may follow
        else if (priceRatio < 0.8)
        {
            marketData.CompetitorPriceMultiplier = Math.Max(0.7, 
                marketData.CompetitorPriceMultiplier - 0.01);
        }
    }
    
    /// <summary>
    /// Get recommended strategy based on market conditions.
    /// </summary>
    private string GetRecommendedStrategy(MarketData marketData)
    {
        return (marketData.DemandLevel, marketData.SupplyLevel) switch
        {
            (> 1.5, < 0.8) => "Premium Pricing - High demand, low supply",
            (< 0.7, > 1.3) => "Discount Pricing - Low demand, high supply",
            (> 1.2, _) => "Competitive Pricing - High demand market",
            (_, > 1.5) => "Penetration Pricing - Oversupplied market",
            _ => "Dynamic Pricing - Balanced market"
        };
    }
    
    /// <summary>
    /// Analyze competitor position.
    /// </summary>
    private string GetCompetitorAnalysis(MarketData marketData)
    {
        return marketData.CompetitorPriceMultiplier switch
        {
            > 1.2 => "Competitors pricing above market",
            > 1.1 => "Competitors pricing slightly high",
            > 0.9 => "Competitive pricing parity",
            > 0.8 => "Competitors undercutting prices",
            _ => "Aggressive competitor pricing"
        };
    }
    
    /// <summary>
    /// Get all market data for competition analysis.
    /// </summary>
    public Dictionary<(ItemType, QualityTier), MarketData> GetAllMarketData()
    {
        return new Dictionary<(ItemType, QualityTier), MarketData>(_marketData);
    }
    
    /// <summary>
    /// Update competitor price information.
    /// </summary>
    public void UpdateCompetitorPrice(ItemType itemType, QualityTier quality, decimal price)
    {
        var marketData = GetOrCreateMarketData(itemType, quality);
        
        // Calculate price ratio compared to current market conditions
        var basePrice = 100m; // Assume base price for calculations
        
        // Create a reference item for price calculation
        var referenceItem = new Items(
            itemId: "ref_" + itemType.ToString().ToLower(),
            name: "Reference Item",
            description: "Reference item for pricing",
            itemType: itemType,
            quality: quality,
            value: (int)basePrice
        );
        
        var currentOptimalPrice = CalculateOptimalPrice(referenceItem, basePrice);
        
        if (currentOptimalPrice > 0)
        {
            var competitorMultiplier = (double)(price / currentOptimalPrice);
            marketData.CompetitorPriceMultiplier = competitorMultiplier;
        }
    }
}