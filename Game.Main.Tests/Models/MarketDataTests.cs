#nullable enable

using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Game.Main.Tests.Models;

public class MarketDataTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Assert
        marketData.ItemType.Should().Be(ItemType.Weapon);
        marketData.QualityTier.Should().Be(QualityTier.Common);
        marketData.DemandLevel.Should().Be(1.0);
        marketData.SupplyLevel.Should().Be(1.0);
        marketData.CurrentCondition.Should().Be(MarketCondition.Normal);
        marketData.CurrentSeason.Should().Be(SeasonalFactor.Spring);
        marketData.RecentSalesCount.Should().Be(0);
        marketData.AverageTimeToSell.Should().Be(12.0);
        marketData.CompetitorPriceMultiplier.Should().Be(1.0);
        marketData.PriceHistory.Should().BeEmpty();
    }
    
    [Fact]
    public void GetPriceMultiplier_WithNormalConditions_ReturnsOne()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Spring,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeApproximately(1.0, 0.1);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithHighDemandLowSupply_ReturnsHighMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 2.0,
            SupplyLevel = 0.5,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Spring,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeGreaterThan(2.0);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithLowDemandHighSupply_ReturnsLowMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 0.5,
            SupplyLevel = 2.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Spring,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeLessThan(1.0);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithHighDemandCondition_IncreasesMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.HighDemand,
            CurrentSeason = SeasonalFactor.Spring,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeGreaterThan(1.0);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithOversupplyCondition_DecreasesMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Oversupply,
            CurrentSeason = SeasonalFactor.Spring,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeLessThan(1.0);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithSummerWeapons_IncreasesMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Summer,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeGreaterThan(1.0);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithWinterWeapons_DecreasesMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Winter,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeLessThan(1.0);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithWartimeWeapons_SignificantlyIncreasesMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Wartime,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeGreaterThan(1.5);
    }
    
    [Fact]
    public void GetPriceMultiplier_WithFestivalConsumables_IncreasesMultiplier()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Consumable,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Festival,
            CompetitorPriceMultiplier = 1.0
        };
        
        // Act
        var multiplier = marketData.GetPriceMultiplier();
        
        // Assert
        multiplier.Should().BeGreaterThan(1.0);
    }
    
    [Fact]
    public void RecordSale_AddsToHistory()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Act
        marketData.RecordSale(100m, 80m, CustomerSatisfaction.Satisfied);
        
        // Assert
        marketData.PriceHistory.Should().HaveCount(1);
        marketData.PriceHistory[0].Price.Should().Be(100m);
        marketData.PriceHistory[0].OriginalPrice.Should().Be(80m);
        marketData.PriceHistory[0].CustomerSatisfaction.Should().Be(CustomerSatisfaction.Satisfied);
        marketData.RecentSalesCount.Should().Be(1);
    }
    
    [Fact]
    public void RecordSale_WithSatisfiedCustomer_IncreasesDemand()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0
        };
        
        // Act
        marketData.RecordSale(100m, 80m, CustomerSatisfaction.Satisfied);
        
        // Assert
        marketData.DemandLevel.Should().BeGreaterThan(1.0);
    }
    
    [Fact]
    public void RecordSale_WithDisappointedCustomer_DecreasesDemand()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0
        };
        
        // Act
        marketData.RecordSale(100m, 80m, CustomerSatisfaction.Disappointed);
        
        // Assert
        marketData.DemandLevel.Should().BeLessThan(1.0);
    }
    
    [Fact]
    public void RecordSale_KeepsHistoryLimitOf30()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Act
        for (int i = 0; i < 35; i++)
        {
            marketData.RecordSale(100m + i, 80m, CustomerSatisfaction.Satisfied);
        }
        
        // Assert
        marketData.PriceHistory.Should().HaveCount(30);
        marketData.PriceHistory.Last().Price.Should().Be(134m); // Last recorded price
    }
    
    [Fact]
    public void RecordTimePassed_WithLongTime_DecreasesDemand()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            AverageTimeToSell = 10.0
        };
        
        // Act
        marketData.RecordTimePassed(TimeSpan.FromHours(20)); // More than 1.5 times average
        
        // Assert
        marketData.DemandLevel.Should().BeLessThan(1.0);
    }
    
    [Fact]
    public void RecordTimePassed_WithDayPassed_ResetsSalesCount()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            RecentSalesCount = 5
        };
        
        // Act
        marketData.RecordTimePassed(TimeSpan.FromHours(25));
        
        // Assert
        marketData.RecentSalesCount.Should().Be(0);
    }
    
    [Fact]
    public void GetMarketTrend_WithInsufficientData_ReturnsInsufficientData()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Act
        var trend = marketData.GetMarketTrend();
        
        // Assert
        trend.Should().Be("Insufficient data");
    }
    
    [Fact]
    public void GetMarketTrend_WithRisingPrices_ReturnsRising()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Add some price history with rising trend
        for (int i = 0; i < 5; i++)
        {
            marketData.RecordSale(100m + (i * 10), 80m, CustomerSatisfaction.Satisfied);
        }
        
        // Act
        var trend = marketData.GetMarketTrend();
        
        // Assert
        trend.Should().Be("Strongly Rising");
    }
    
    [Fact]
    public void GetMarketTrend_WithStablePrices_ReturnsStable()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Add some price history with stable trend
        for (int i = 0; i < 5; i++)
        {
            marketData.RecordSale(100m, 100m, CustomerSatisfaction.Satisfied);
        }
        
        // Act
        var trend = marketData.GetMarketTrend();
        
        // Assert
        trend.Should().Be("Stable");
    }
    
    [Fact]
    public void GetMarketTrend_WithDecliningPrices_ReturnsDeclining()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Add some price history with declining trend
        for (int i = 0; i < 5; i++)
        {
            marketData.RecordSale(100m - (i * 15), 120m, CustomerSatisfaction.Satisfied);
        }
        
        // Act
        var trend = marketData.GetMarketTrend();
        
        // Assert
        trend.Should().Be("Strongly Declining");
    }
    
    [Fact]
    public void LastUpdated_IsSetWhenCreated()
    {
        // Act
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common
        };
        
        // Assert
        marketData.LastUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
    
    [Fact]
    public void LastUpdated_IsUpdatedOnRecordSale()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            LastUpdated = DateTime.Now.AddHours(-1)
        };
        var oldTime = marketData.LastUpdated;
        
        // Act
        marketData.RecordSale(100m, 80m, CustomerSatisfaction.Satisfied);
        
        // Assert
        marketData.LastUpdated.Should().BeAfter(oldTime);
    }
    
    [Fact]
    public void LastUpdated_IsUpdatedOnRecordTimePassed()
    {
        // Arrange
        var marketData = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            LastUpdated = DateTime.Now.AddHours(-1)
        };
        var oldTime = marketData.LastUpdated;
        
        // Act
        marketData.RecordTimePassed(TimeSpan.FromHours(2));
        
        // Assert
        marketData.LastUpdated.Should().BeAfter(oldTime);
    }
    
    [Fact]
    public void CompetitorPriceMultiplier_AffectsPriceMultiplier()
    {
        // Arrange
        var marketData1 = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Spring,
            CompetitorPriceMultiplier = 0.8 // Competitors pricing lower
        };
        
        var marketData2 = new MarketData
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            DemandLevel = 1.0,
            SupplyLevel = 1.0,
            CurrentCondition = MarketCondition.Normal,
            CurrentSeason = SeasonalFactor.Spring,
            CompetitorPriceMultiplier = 1.2 // Competitors pricing higher
        };
        
        // Act
        var multiplier1 = marketData1.GetPriceMultiplier();
        var multiplier2 = marketData2.GetPriceMultiplier();
        
        // Assert
        multiplier1.Should().BeLessThan(multiplier2);
    }
}
