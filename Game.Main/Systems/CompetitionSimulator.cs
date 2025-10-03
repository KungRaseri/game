#nullable enable

using Game.Main.Utils;

namespace Game.Main.Systems;

/// <summary>
/// Simulates AI competitors in the market, adjusting their prices and inventory 
/// based on our shop's performance and market conditions.
/// </summary>
public class CompetitionSimulator
{
    private readonly List<AICompetitor> _competitors = new();
    private readonly Dictionary<(ItemType, QualityTier), decimal> _lastPlayerPrices = new();
    private readonly Random _random = new();
    
    // Configuration
    public double CompetitorReactionSensitivity { get; set; } = 0.3;
    public double MaxCompetitorPriceChange { get; set; } = 0.25; // 25%
    public int MaxCompetitors { get; set; } = 5;
    public bool EnableAggressiveCompetition { get; set; } = true;
    
    public IReadOnlyList<AICompetitor> Competitors => _competitors;
    
    public CompetitionSimulator()
    {
        InitializeCompetitors();
    }
    
    /// <summary>
    /// Updates competitor pricing and strategies based on market analysis.
    /// </summary>
    public CompetitionAnalysis AnalyzeCompetition(ShopPerformanceMetrics playerMetrics,
        Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        UpdateCompetitorStrategies(playerMetrics);
        
        var analysis = new CompetitionAnalysis
        {
            TotalCompetitors = _competitors.Count,
            AverageCompetitorPerformance = _competitors.Average(c => c.PerformanceScore),
            MarketShare = CalculatePlayerMarketShare(playerMetrics),
            CompetitivePressure = CalculateCompetitivePressure(marketData),
            Threats = IdentifyCompetitiveThreats(),
            Opportunities = IdentifyMarketOpportunities(marketData),
            RecommendedActions = GenerateRecommendations(playerMetrics, marketData)
        };
        
        GameLogger.Debug($"Competition analysis: {analysis.TotalCompetitors} competitors, " +
                        $"Player market share: {analysis.MarketShare:P1}, " +
                        $"Competitive pressure: {analysis.CompetitivePressure:F2}");
        
        return analysis;
    }
    
    /// <summary>
    /// Simulates competitor reactions to player pricing changes.
    /// </summary>
    public void ReactToPlayerPricing(ItemType itemType, QualityTier quality, decimal newPrice, decimal oldPrice)
    {
        var priceChange = (double)(newPrice - oldPrice) / (double)oldPrice;
        _lastPlayerPrices[(itemType, quality)] = newPrice;
        
        foreach (var competitor in _competitors.Where(c => c.SellsItem(itemType, quality)))
        {
            var reaction = CalculateCompetitorReaction(competitor, priceChange, itemType, quality);
            ApplyCompetitorReaction(competitor, reaction, itemType, quality);
        }
        
        GameLogger.Info($"Competitors reacted to player price change: {itemType} {quality} " +
                       $"{oldPrice:C} -> {newPrice:C} ({priceChange:P1})");
    }
    
    /// <summary>
    /// Periodically updates competitor behavior and market dynamics.
    /// </summary>
    public void UpdateMarketDynamics(Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        foreach (var competitor in _competitors)
        {
            // Update competitor performance based on market conditions
            UpdateCompetitorPerformance(competitor, marketData);
            
            // Simulate competitor actions (inventory changes, promotions, etc.)
            SimulateCompetitorActions(competitor, marketData);
            
            // Update pricing strategies
            UpdateCompetitorPricing(competitor, marketData);
        }
        
        // Occasionally add or remove competitors based on market conditions
        ManageCompetitorPopulation(marketData);
    }
    
    private void InitializeCompetitors()
    {
        var competitorTypes = new[]
        {
            new { Name = "The Iron Forge", Strategy = CompetitorStrategy.QualityFocused, Aggressiveness = 0.3 },
            new { Name = "Bargain Blades", Strategy = CompetitorStrategy.LowPrice, Aggressiveness = 0.8 },
            new { Name = "Elite Armory", Strategy = CompetitorStrategy.Premium, Aggressiveness = 0.2 },
            new { Name = "Quick & Cheap", Strategy = CompetitorStrategy.VolumeDiscount, Aggressiveness = 0.6 },
            new { Name = "Master Craftsman", Strategy = CompetitorStrategy.Specialized, Aggressiveness = 0.4 }
        };
        
        foreach (var template in competitorTypes.Take(MaxCompetitors))
        {
            var competitor = new AICompetitor
            {
                CompetitorId = Guid.NewGuid().ToString(),
                Name = template.Name,
                Strategy = template.Strategy,
                Aggressiveness = template.Aggressiveness,
                PerformanceScore = _random.NextDouble() * 0.4 + 0.3, // 0.3-0.7
                InventoryStrategy = GetRandomInventoryStrategy(),
                PricingModifier = _random.NextDouble() * 0.4 + 0.8 // 0.8-1.2
            };
            
            InitializeCompetitorInventory(competitor);
            _competitors.Add(competitor);
        }
        
        GameLogger.Info($"Initialized {_competitors.Count} AI competitors");
    }
    
    private void UpdateCompetitorStrategies(ShopPerformanceMetrics playerMetrics)
    {
        var playerPerformanceGrade = playerMetrics.GetPerformanceGrade();
        
        foreach (var competitor in _competitors)
        {
            // Adjust aggressiveness based on player performance
            if (playerPerformanceGrade >= PerformanceGrade.VeryGood)
            {
                competitor.Aggressiveness = Math.Min(1.0, competitor.Aggressiveness + 0.1);
            }
            else if (playerPerformanceGrade <= PerformanceGrade.BelowAverage)
            {
                competitor.Aggressiveness = Math.Max(0.1, competitor.Aggressiveness - 0.05);
            }
        }
    }
    
    private CompetitorReaction CalculateCompetitorReaction(AICompetitor competitor, 
        double playerPriceChange, ItemType itemType, QualityTier quality)
    {
        var reaction = new CompetitorReaction
        {
            CompetitorId = competitor.CompetitorId,
            ItemType = itemType,
            QualityTier = quality,
            ReactionType = CompetitorReactionType.NoChange
        };
        
        // Calculate reaction probability based on competitor strategy and aggressiveness
        var reactionProbability = competitor.Aggressiveness * CompetitorReactionSensitivity;
        
        if (_random.NextDouble() > reactionProbability)
        {
            return reaction; // No reaction
        }
        
        // Determine reaction type based on strategy and price change
        switch (competitor.Strategy)
        {
            case CompetitorStrategy.LowPrice:
                if (playerPriceChange > 0.05) // Player increased price by >5%
                {
                    reaction.ReactionType = CompetitorReactionType.UnderCut;
                    reaction.PriceMultiplier = 0.95; // Undercut by 5%
                }
                else if (playerPriceChange < -0.1) // Player decreased price by >10%
                {
                    reaction.ReactionType = CompetitorReactionType.MatchPrice;
                    reaction.PriceMultiplier = 0.98; // Match but slightly lower
                }
                break;
                
            case CompetitorStrategy.QualityFocused:
                if (playerPriceChange < -0.15) // Significant price drop
                {
                    reaction.ReactionType = CompetitorReactionType.Promotion;
                    reaction.PriceMultiplier = 0.9; // Temporary promotion
                }
                break;
                
            case CompetitorStrategy.Premium:
                if (playerPriceChange > 0.2) // Large price increase
                {
                    reaction.ReactionType = CompetitorReactionType.PriceIncrease;
                    reaction.PriceMultiplier = 1.05; // Follow the trend
                }
                break;
                
            case CompetitorStrategy.VolumeDiscount:
                reaction.ReactionType = CompetitorReactionType.BulkDiscount;
                reaction.PriceMultiplier = 0.92; // Volume pricing
                break;
                
            case CompetitorStrategy.Specialized:
                // Specialized competitors react less to general price changes
                if (Math.Abs(playerPriceChange) > 0.25)
                {
                    reaction.ReactionType = CompetitorReactionType.MatchPrice;
                    reaction.PriceMultiplier = 1.0 + playerPriceChange * 0.5;
                }
                break;
        }
        
        return reaction;
    }
    
    private void ApplyCompetitorReaction(AICompetitor competitor, CompetitorReaction reaction, 
        ItemType itemType, QualityTier quality)
    {
        if (reaction.ReactionType == CompetitorReactionType.NoChange)
            return;
            
        // Update competitor's pricing for this item
        var key = (itemType, quality);
        if (!competitor.ItemPrices.ContainsKey(key))
        {
            competitor.ItemPrices[key] = 100m; // Default base price
        }
        
        var oldPrice = competitor.ItemPrices[key];
        var newPrice = oldPrice * (decimal)reaction.PriceMultiplier;
        
        // Apply bounds to prevent extreme pricing
        var basePrice = GetBasePriceForItem(itemType, quality);
        newPrice = Math.Max(basePrice * 0.5m, Math.Min(basePrice * 3.0m, newPrice));
        
        competitor.ItemPrices[key] = newPrice;
        
        // Update competitor performance based on reaction success
        UpdateCompetitorReactionSuccess(competitor, reaction, oldPrice, newPrice);
        
        GameLogger.Debug($"Competitor {competitor.Name} reacted: {reaction.ReactionType} " +
                        $"for {itemType} {quality} {oldPrice:C} -> {newPrice:C}");
    }
    
    private void SimulateCompetitorActions(AICompetitor competitor, 
        Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        // Simulate periodic competitor actions
        if (_random.NextDouble() < 0.1) // 10% chance per update
        {
            var action = GenerateRandomCompetitorAction(competitor, marketData);
            ExecuteCompetitorAction(competitor, action);
        }
    }
    
    private double CalculatePlayerMarketShare(ShopPerformanceMetrics playerMetrics)
    {
        var totalMarketRevenue = playerMetrics.TotalRevenue + 
            _competitors.Sum(c => (decimal)(c.PerformanceScore * 10000)); // Simulated competitor revenue
        
        return totalMarketRevenue > 0 ? (double)(playerMetrics.TotalRevenue / totalMarketRevenue) : 0.0;
    }
    
    private double CalculateCompetitivePressure(Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        var avgCompetitorAggressiveness = _competitors.Average(c => c.Aggressiveness);
        var marketSaturation = Math.Min(1.0, _competitors.Count / (double)MaxCompetitors);
        
        return (avgCompetitorAggressiveness + marketSaturation) / 2.0;
    }
    
    private List<string> IdentifyCompetitiveThreats()
    {
        var threats = new List<string>();
        
        var topCompetitors = _competitors
            .OrderByDescending(c => c.PerformanceScore)
            .Take(2);
            
        foreach (var competitor in topCompetitors)
        {
            if (competitor.PerformanceScore > 0.7)
            {
                threats.Add($"{competitor.Name} is performing strongly with {competitor.Strategy} strategy");
            }
            
            if (competitor.Aggressiveness > 0.8)
            {
                threats.Add($"{competitor.Name} is being highly aggressive in pricing");
            }
        }
        
        return threats;
    }
    
    private List<string> IdentifyMarketOpportunities(Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        var opportunities = new List<string>();
        
        foreach (var (key, data) in marketData)
        {
            if (data.DemandLevel > 1.3 && data.SupplyLevel < 0.8)
            {
                opportunities.Add($"High demand, low supply for {key.Item1} ({key.Item2}) - pricing opportunity");
            }
            
            var competitorsInCategory = _competitors.Count(c => c.SellsItem(key.Item1, key.Item2));
            if (competitorsInCategory < 2)
            {
                opportunities.Add($"Low competition in {key.Item1} ({key.Item2}) category");
            }
        }
        
        return opportunities;
    }
    
    private List<string> GenerateRecommendations(ShopPerformanceMetrics playerMetrics,
        Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        var recommendations = new List<string>();
        var marketShare = CalculatePlayerMarketShare(playerMetrics);
        
        if (marketShare < 0.2)
        {
            recommendations.Add("Consider aggressive pricing to gain market share");
        }
        else if (marketShare > 0.6)
        {
            recommendations.Add("Strong market position - consider premium pricing strategy");
        }
        
        var avgCompetitorAggression = _competitors.Average(c => c.Aggressiveness);
        if (avgCompetitorAggression > 0.7)
        {
            recommendations.Add("High competitive pressure - focus on differentiation and customer service");
        }
        
        return recommendations;
    }
    
    // Helper methods for competitor management
    private InventoryStrategy GetRandomInventoryStrategy()
    {
        var strategies = Enum.GetValues<InventoryStrategy>();
        return strategies[_random.Next(strategies.Length)];
    }
    
    private void InitializeCompetitorInventory(AICompetitor competitor)
    {
        var itemTypes = Enum.GetValues<ItemType>();
        var qualities = Enum.GetValues<QualityTier>();
        
        foreach (var itemType in itemTypes)
        {
            if (_random.NextDouble() < 0.6) // 60% chance to sell this item type
            {
                foreach (var quality in qualities)
                {
                    if (_random.NextDouble() < GetQualityProbability(quality))
                    {
                        var basePrice = GetBasePriceForItem(itemType, quality);
                        competitor.ItemPrices[(itemType, quality)] = basePrice * (decimal)competitor.PricingModifier;
                    }
                }
            }
        }
    }
    
    private double GetQualityProbability(QualityTier quality) => quality switch
    {
        QualityTier.Common => 0.8,
        QualityTier.Uncommon => 0.6,
        QualityTier.Rare => 0.4,
        QualityTier.Epic => 0.2,
        QualityTier.Legendary => 0.1,
        _ => 0.5
    };
    
    private decimal GetBasePriceForItem(ItemType itemType, QualityTier quality)
    {
        var basePrice = itemType switch
        {
            ItemType.Weapon => 50m,
            ItemType.Armor => 75m,
            ItemType.Consumable => 15m,
            ItemType.Material => 10m,
            _ => 25m
        };
        
        var qualityMultiplier = quality switch
        {
            QualityTier.Common => 1.0m,
            QualityTier.Uncommon => 2.0m,
            QualityTier.Rare => 4.0m,
            QualityTier.Epic => 8.0m,
            QualityTier.Legendary => 16.0m,
            _ => 1.0m
        };
        
        return basePrice * qualityMultiplier;
    }
    
    private void UpdateCompetitorPerformance(AICompetitor competitor, 
        Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        // Update performance based on strategy effectiveness and market conditions
        var strategyEffectiveness = CalculateStrategyEffectiveness(competitor, marketData);
        competitor.PerformanceScore = Math.Max(0.1, Math.Min(1.0, 
            competitor.PerformanceScore * 0.95 + strategyEffectiveness * 0.05));
    }
    
    private double CalculateStrategyEffectiveness(AICompetitor competitor,
        Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        // Simple strategy effectiveness calculation
        return competitor.Strategy switch
        {
            CompetitorStrategy.LowPrice => 0.7 + _random.NextDouble() * 0.2,
            CompetitorStrategy.QualityFocused => 0.6 + _random.NextDouble() * 0.3,
            CompetitorStrategy.Premium => 0.5 + _random.NextDouble() * 0.4,
            CompetitorStrategy.VolumeDiscount => 0.6 + _random.NextDouble() * 0.3,
            CompetitorStrategy.Specialized => 0.7 + _random.NextDouble() * 0.2,
            _ => 0.5 + _random.NextDouble() * 0.3
        };
    }
    
    private void UpdateCompetitorPricing(AICompetitor competitor,
        Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        foreach (var (key, price) in competitor.ItemPrices.ToList())
        {
            if (marketData.TryGetValue(key, out var market))
            {
                var marketMultiplier = market.GetPriceMultiplier();
                var newPrice = price * (decimal)(1.0 + (marketMultiplier - 1.0) * 0.1); // Gradual adjustment
                competitor.ItemPrices[key] = newPrice;
            }
        }
    }
    
    private CompetitorAction GenerateRandomCompetitorAction(AICompetitor competitor,
        Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        var actionTypes = Enum.GetValues<CompetitorActionType>();
        var actionType = actionTypes[_random.Next(actionTypes.Length)];
        
        return new CompetitorAction
        {
            CompetitorId = competitor.CompetitorId,
            ActionType = actionType,
            Timestamp = DateTime.Now,
            Description = GenerateActionDescription(actionType, competitor)
        };
    }
    
    private string GenerateActionDescription(CompetitorActionType actionType, AICompetitor competitor)
    {
        return actionType switch
        {
            CompetitorActionType.InventoryRestock => $"{competitor.Name} restocked popular items",
            CompetitorActionType.PriceReduction => $"{competitor.Name} reduced prices on select items",
            CompetitorActionType.Promotion => $"{competitor.Name} started a limited-time promotion",
            CompetitorActionType.QualityUpgrade => $"{competitor.Name} improved item quality",
            CompetitorActionType.MarketExpansion => $"{competitor.Name} expanded into new item categories",
            CompetitorActionType.CustomerService => $"{competitor.Name} improved customer service",
            _ => $"{competitor.Name} made strategic adjustments"
        };
    }
    
    private void ExecuteCompetitorAction(AICompetitor competitor, CompetitorAction action)
    {
        switch (action.ActionType)
        {
            case CompetitorActionType.PriceReduction:
                // Reduce some prices
                var itemsToReduce = competitor.ItemPrices.Keys.Take(_random.Next(1, 4));
                foreach (var item in itemsToReduce)
                {
                    competitor.ItemPrices[item] *= 0.9m; // 10% reduction
                }
                break;
                
            case CompetitorActionType.QualityUpgrade:
                competitor.PerformanceScore = Math.Min(1.0, competitor.PerformanceScore + 0.05);
                break;
                
            case CompetitorActionType.CustomerService:
                competitor.Aggressiveness = Math.Max(0.1, competitor.Aggressiveness - 0.05);
                break;
        }
        
        GameLogger.Info($"Competitor action executed: {action.Description}");
    }
    
    private void UpdateCompetitorReactionSuccess(AICompetitor competitor, CompetitorReaction reaction,
        decimal oldPrice, decimal newPrice)
    {
        // Update competitor performance based on reaction success
        var reactionSuccess = _random.NextDouble() * competitor.Aggressiveness;
        if (reactionSuccess > 0.5)
        {
            competitor.PerformanceScore = Math.Min(1.0, competitor.PerformanceScore + 0.02);
        }
        else
        {
            competitor.PerformanceScore = Math.Max(0.1, competitor.PerformanceScore - 0.01);
        }
    }
    
    private void ManageCompetitorPopulation(Dictionary<(ItemType, QualityTier), MarketData> marketData)
    {
        // Occasionally add or remove competitors based on market conditions
        if (_random.NextDouble() < 0.01) // 1% chance per update
        {
            var averageMarketPerformance = marketData.Values.Average(m => m.DemandLevel);
            
            if (averageMarketPerformance > 1.2 && _competitors.Count < MaxCompetitors)
            {
                // Good market - chance for new competitor
                AddNewCompetitor();
            }
            else if (averageMarketPerformance < 0.8 && _competitors.Count > 2)
            {
                // Poor market - weakest competitor might leave
                RemoveWeakestCompetitor();
            }
        }
    }
    
    private void AddNewCompetitor()
    {
        var competitorNames = new[] { "New Venture", "Market Entrant", "Startup Shop", "Fresh Store", "Rising Star" };
        var name = competitorNames[_random.Next(competitorNames.Length)];
        
        var newCompetitor = new AICompetitor
        {
            CompetitorId = Guid.NewGuid().ToString(),
            Name = name,
            Strategy = (CompetitorStrategy)_random.Next(Enum.GetValues<CompetitorStrategy>().Length),
            Aggressiveness = _random.NextDouble() * 0.6 + 0.2, // 0.2-0.8
            PerformanceScore = _random.NextDouble() * 0.3 + 0.2, // 0.2-0.5 (new, lower performance)
            InventoryStrategy = GetRandomInventoryStrategy(),
            PricingModifier = _random.NextDouble() * 0.4 + 0.8 // 0.8-1.2
        };
        
        InitializeCompetitorInventory(newCompetitor);
        _competitors.Add(newCompetitor);
        
        GameLogger.Info($"New competitor entered market: {name}");
    }
    
    private void RemoveWeakestCompetitor()
    {
        var weakest = _competitors.OrderBy(c => c.PerformanceScore).FirstOrDefault();
        if (weakest != null)
        {
            _competitors.Remove(weakest);
            GameLogger.Info($"Competitor left market: {weakest.Name}");
        }
    }
}
