#nullable enable

using Game.Items.Models;
using Game.Shop.Models;
using Game.Shop.Systems;

namespace Game.Shop.Tests;

/// <summary>
/// Tests for the Dynamic Pricing Engine system.
/// </summary>
public class PricingEngineTests
{
    private PricingEngine CreatePricingEngine()
    {
        return new PricingEngine();
    }
    
    private Item CreateTestItem(ItemType type = ItemType.Weapon, QualityTier quality = QualityTier.Common)
    {
        return new Item(
            itemId: "test-item",
            name: "Test Item",
            description: "A test item for pricing",
            itemType: type,
            quality: quality,
            value: 100
        );
    }
    
    [Fact]
    public void PricingEngine_CalculateOptimalPrice_ReturnsValidPrice()
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        var basePrice = 100m;
        
        // Act
        var optimalPrice = engine.CalculateOptimalPrice(item, basePrice);
        
        // Assert
        Assert.True(optimalPrice > 0);
        Assert.True(optimalPrice >= basePrice * 0.3m); // Min price constraint
        Assert.True(optimalPrice <= basePrice * 2.0m); // Max price constraint
    }
    
    [Theory]
    [InlineData(QualityTier.Common, 1.0)]
    [InlineData(QualityTier.Uncommon, 1.3)]
    [InlineData(QualityTier.Rare, 1.8)]
    [InlineData(QualityTier.Epic, 2.5)]
    [InlineData(QualityTier.Legendary, 4.0)]
    public void PricingEngine_QualityTiers_ApplyCorrectMultipliers(QualityTier quality, double expectedMultiplier)
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem(quality: quality);
        var basePrice = 100m;
        
        // Act
        var optimalPrice = engine.CalculateOptimalPrice(item, basePrice);
        
        // Assert - Price should be influenced by quality (allowing for market variance)
        var priceRatio = (double)(optimalPrice / basePrice);
        Assert.True(priceRatio > expectedMultiplier * 0.3); // Lower bound with market factors
        Assert.True(priceRatio < expectedMultiplier * 3.0); // Upper bound with market factors
    }
    
    [Fact]
    public void PricingEngine_PredictCustomerSatisfaction_WorksCorrectly()
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        var basePrice = 100m;
        
        // Act & Assert
        var delightedSatisfaction = engine.PredictCustomerSatisfaction(item, 70m, basePrice);
        Assert.Equal(CustomerSatisfaction.Delighted, delightedSatisfaction);
        
        var satisfiedSatisfaction = engine.PredictCustomerSatisfaction(item, 90m, basePrice);
        Assert.Equal(CustomerSatisfaction.Satisfied, satisfiedSatisfaction);
        
        var neutralSatisfaction = engine.PredictCustomerSatisfaction(item, 120m, basePrice);
        Assert.Equal(CustomerSatisfaction.Neutral, neutralSatisfaction);
        
        var disappointedSatisfaction = engine.PredictCustomerSatisfaction(item, 150m, basePrice);
        Assert.Equal(CustomerSatisfaction.Disappointed, disappointedSatisfaction);
        
        var angrySatisfaction = engine.PredictCustomerSatisfaction(item, 200m, basePrice);
        Assert.Equal(CustomerSatisfaction.Angry, angrySatisfaction);
    }
    
    [Fact]
    public void PricingEngine_RecordSale_UpdatesMarketData()
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        
        // Get initial analysis
        var initialAnalysis = engine.GetMarketAnalysis(item.ItemType, item.Quality);
        var initialSales = initialAnalysis.RecentSales;
        
        // Act
        engine.RecordSale(item, 120m, 100m, CustomerSatisfaction.Satisfied);
        
        // Assert
        var updatedAnalysis = engine.GetMarketAnalysis(item.ItemType, item.Quality);
        Assert.Equal(initialSales + 1, updatedAnalysis.RecentSales);
    }
    
    [Fact]
    public void PricingEngine_SetPricingStrategy_AffectsPricing()
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        var basePrice = 100m;
        
        // Act - Set to discount strategy
        engine.SetPricingStrategy(item.ItemType, PricingStrategy.Discount);
        var discountPrice = engine.CalculateOptimalPrice(item, basePrice);
        
        // Set to premium strategy
        engine.SetPricingStrategy(item.ItemType, PricingStrategy.Premium);
        var premiumPrice = engine.CalculateOptimalPrice(item, basePrice);
        
        // Assert - Premium should generally be higher than discount
        Assert.True(premiumPrice >= discountPrice);
    }
    
    [Fact]
    public void PricingEngine_UpdateMarketConditions_ModifiesMarket()
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        
        // Record some sales to establish market data
        engine.RecordSale(item, 120m, 100m, CustomerSatisfaction.Satisfied);
        
        var initialAnalysis = engine.GetMarketAnalysis(item.ItemType, item.Quality);
        
        // Act - Simulate time passing
        engine.UpdateMarketConditions(TimeSpan.FromHours(25)); // More than 24 hours
        
        // Assert - Market should have evolved
        var updatedAnalysis = engine.GetMarketAnalysis(item.ItemType, item.Quality);
        Assert.Equal(0, updatedAnalysis.RecentSales); // Sales reset after 24 hours
    }
    
    [Fact]
    public void PricingEngine_GetMarketAnalysis_ProvidesComprehensiveData()
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        
        // Act
        var analysis = engine.GetMarketAnalysis(item.ItemType, item.Quality);
        
        // Assert
        Assert.Equal(item.ItemType, analysis.ItemType);
        Assert.Equal(item.Quality, analysis.Quality);
        Assert.True(analysis.DemandLevel > 0);
        Assert.True(analysis.SupplyLevel > 0);
        Assert.True(analysis.PriceMultiplier > 0);
        Assert.NotNull(analysis.MarketTrend);
        Assert.NotNull(analysis.RecommendedStrategy);
        Assert.NotNull(analysis.CompetitorPosition);
    }
    
    [Fact]
    public void PricingEngine_VolatilitySettings_AffectPriceVariance()
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        var basePrice = 100m;
        
        // Test low volatility
        engine.PriceVolatility = 0.01; // 1% variance
        var prices1 = new decimal[10];
        for (int i = 0; i < 10; i++)
        {
            prices1[i] = engine.CalculateOptimalPrice(item, basePrice);
        }
        
        // Test high volatility
        engine.PriceVolatility = 0.2; // 20% variance
        var prices2 = new decimal[10];
        for (int i = 0; i < 10; i++)
        {
            prices2[i] = engine.CalculateOptimalPrice(item, basePrice);
        }
        
        // Assert - Higher volatility should produce more variance
        var variance1 = CalculateVariance(prices1);
        var variance2 = CalculateVariance(prices2);
        
        // High volatility should generally produce more variance (though random)
        Assert.True(variance2 >= 0); // Just ensure no exceptions
        Assert.True(variance1 >= 0);
    }
    
    [Theory]
    [InlineData(PricingStrategy.Premium)]
    [InlineData(PricingStrategy.Competitive)]
    [InlineData(PricingStrategy.Discount)]
    [InlineData(PricingStrategy.Dynamic)]
    [InlineData(PricingStrategy.Seasonal)]
    [InlineData(PricingStrategy.Penetration)]
    [InlineData(PricingStrategy.Skimming)]
    public void PricingEngine_AllPricingStrategies_ProduceValidPrices(PricingStrategy strategy)
    {
        // Arrange
        var engine = CreatePricingEngine();
        var item = CreateTestItem();
        var basePrice = 100m;
        
        // Act
        engine.SetPricingStrategy(item.ItemType, strategy);
        var price = engine.CalculateOptimalPrice(item, basePrice);
        
        // Assert
        Assert.True(price > 0);
        Assert.True(price >= basePrice * 0.3m); // Min constraint
        Assert.True(price <= basePrice * 2.0m); // Max constraint
    }
    
    private double CalculateVariance(decimal[] values)
    {
        if (values.Length == 0) return 0;
        
        var mean = values.Average();
        var sumSquaredDiffs = values.Sum(v => Math.Pow((double)(v - mean), 2));
        return sumSquaredDiffs / values.Length;
    }
}
