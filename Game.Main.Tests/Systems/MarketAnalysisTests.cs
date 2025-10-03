#nullable enable

using FluentAssertions;
using Xunit;
using Game.Main.Systems;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Tests for the MarketAnalysis class.
/// </summary>
public class MarketAnalysisTests
{
    [Fact]
    public void MarketAnalysis_CanBeCreated_WithDefaultValues()
    {
        // Act
        var analysis = new MarketAnalysis();

        // Assert
        analysis.ItemType.Should().Be(ItemType.Weapon); // Default enum value
        analysis.Quality.Should().Be(QualityTier.Common); // Default enum value
        analysis.DemandLevel.Should().Be(0.0);
        analysis.SupplyLevel.Should().Be(0.0);
        analysis.PriceMultiplier.Should().Be(0.0);
        analysis.MarketTrend.Should().BeEmpty();
        analysis.RecentSales.Should().Be(0);
        analysis.AverageTimeToSell.Should().Be(0.0);
        analysis.RecommendedStrategy.Should().BeEmpty();
        analysis.CompetitorPosition.Should().BeEmpty();
    }

    [Fact]
    public void MarketAnalysis_CanBeCreated_WithInitializers()
    {
        // Act
        var analysis = new MarketAnalysis
        {
            ItemType = ItemType.Armor,
            Quality = QualityTier.Epic,
            DemandLevel = 0.8,
            SupplyLevel = 0.3,
            PriceMultiplier = 1.5,
            MarketTrend = "Rising",
            RecentSales = 15,
            AverageTimeToSell = 3.5,
            RecommendedStrategy = "Increase prices",
            CompetitorPosition = "Below market average"
        };

        // Assert
        analysis.ItemType.Should().Be(ItemType.Armor);
        analysis.Quality.Should().Be(QualityTier.Epic);
        analysis.DemandLevel.Should().Be(0.8);
        analysis.SupplyLevel.Should().Be(0.3);
        analysis.PriceMultiplier.Should().Be(1.5);
        analysis.MarketTrend.Should().Be("Rising");
        analysis.RecentSales.Should().Be(15);
        analysis.AverageTimeToSell.Should().Be(3.5);
        analysis.RecommendedStrategy.Should().Be("Increase prices");
        analysis.CompetitorPosition.Should().Be("Below market average");
    }

    [Fact]
    public void MarketAnalysis_HighDemandLowSupply_ScenarioTest()
    {
        // Arrange & Act
        var analysis = new MarketAnalysis
        {
            ItemType = ItemType.Weapon,
            Quality = QualityTier.Rare,
            DemandLevel = 0.9,
            SupplyLevel = 0.2,
            PriceMultiplier = 1.8,
            MarketTrend = "Bullish",
            RecentSales = 25,
            AverageTimeToSell = 1.2,
            RecommendedStrategy = "Increase inventory and prices",
            CompetitorPosition = "Market leader"
        };

        // Assert
        analysis.DemandLevel.Should().Be(0.9);
        analysis.SupplyLevel.Should().Be(0.2);
        analysis.PriceMultiplier.Should().BeGreaterThan(1.0);
        analysis.AverageTimeToSell.Should().BeLessThan(2.0); // Fast selling
    }

    [Fact]
    public void MarketAnalysis_LowDemandHighSupply_ScenarioTest()
    {
        // Arrange & Act
        var analysis = new MarketAnalysis
        {
            ItemType = ItemType.Material,
            Quality = QualityTier.Common,
            DemandLevel = 0.3,
            SupplyLevel = 0.9,
            PriceMultiplier = 0.7,
            MarketTrend = "Bearish",
            RecentSales = 3,
            AverageTimeToSell = 15.5,
            RecommendedStrategy = "Reduce prices and inventory",
            CompetitorPosition = "Oversupplied market"
        };

        // Assert
        analysis.DemandLevel.Should().Be(0.3);
        analysis.SupplyLevel.Should().Be(0.9);
        analysis.PriceMultiplier.Should().BeLessThan(1.0);
        analysis.AverageTimeToSell.Should().BeGreaterThan(10.0); // Slow selling
        analysis.RecentSales.Should().BeLessThan(5);
    }

    [Fact]
    public void MarketAnalysis_AllItemTypes_CanBeAnalyzed()
    {
        // Arrange
        var itemTypes = new[] { ItemType.Weapon, ItemType.Armor, ItemType.Material, ItemType.Consumable };

        foreach (var itemType in itemTypes)
        {
            // Act
            var analysis = new MarketAnalysis
            {
                ItemType = itemType,
                Quality = QualityTier.Uncommon,
                DemandLevel = 0.5,
                SupplyLevel = 0.5,
                PriceMultiplier = 1.0,
                MarketTrend = "Stable",
                RecentSales = 10,
                AverageTimeToSell = 5.0,
                RecommendedStrategy = $"Maintain current {itemType} strategy",
                CompetitorPosition = "Competitive"
            };

            // Assert
            analysis.ItemType.Should().Be(itemType);
            analysis.RecommendedStrategy.Should().Contain(itemType.ToString());
        }
    }

    [Fact]
    public void MarketAnalysis_AllQualityTiers_CanBeAnalyzed()
    {
        // Arrange
        var qualityTiers = new[] { QualityTier.Common, QualityTier.Uncommon, QualityTier.Rare, QualityTier.Epic, QualityTier.Legendary };

        foreach (var quality in qualityTiers)
        {
            // Act
            var analysis = new MarketAnalysis
            {
                ItemType = ItemType.Weapon,
                Quality = quality,
                DemandLevel = 0.6,
                SupplyLevel = 0.4,
                PriceMultiplier = quality switch
                {
                    QualityTier.Common => 0.8,
                    QualityTier.Uncommon => 1.0,
                    QualityTier.Rare => 1.5,
                    QualityTier.Epic => 2.0,
                    QualityTier.Legendary => 3.0,
                    _ => 1.0
                },
                MarketTrend = "Variable",
                RecentSales = 8,
                AverageTimeToSell = 6.0,
                RecommendedStrategy = $"Optimize {quality} tier pricing",
                CompetitorPosition = "Quality-dependent positioning"
            };

            // Assert
            analysis.Quality.Should().Be(quality);
            analysis.RecommendedStrategy.Should().Contain(quality.ToString());
        }
    }

    [Fact]
    public void MarketAnalysis_NegativeValues_CanBeRepresented()
    {
        // Act
        var analysis = new MarketAnalysis
        {
            ItemType = ItemType.Weapon,
            Quality = QualityTier.Common,
            DemandLevel = -0.1, // Could represent declining demand
            SupplyLevel = 0.5,
            PriceMultiplier = 0.85, // Below market rate
            MarketTrend = "Declining",
            RecentSales = 0,
            AverageTimeToSell = 30.0, // Very slow selling
            RecommendedStrategy = "Discontinue or heavily discount",
            CompetitorPosition = "Underperforming"
        };

        // Assert
        analysis.DemandLevel.Should().Be(-0.1);
        analysis.PriceMultiplier.Should().BeLessThan(1.0);
        analysis.RecentSales.Should().Be(0);
        analysis.MarketTrend.Should().Be("Declining");
    }

    [Fact]
    public void MarketAnalysis_ExtremelyHighDemand_CanBeRepresented()
    {
        // Act
        var analysis = new MarketAnalysis
        {
            ItemType = ItemType.Armor,
            Quality = QualityTier.Legendary,
            DemandLevel = 2.5, // Extremely high demand (above normal range)
            SupplyLevel = 0.1, // Very low supply
            PriceMultiplier = 5.0, // Premium pricing
            MarketTrend = "Explosive growth",
            RecentSales = 100,
            AverageTimeToSell = 0.1, // Almost instant selling
            RecommendedStrategy = "Maximize prices and find more suppliers",
            CompetitorPosition = "Market domination"
        };

        // Assert
        analysis.DemandLevel.Should().Be(2.5);
        analysis.SupplyLevel.Should().Be(0.1);
        analysis.PriceMultiplier.Should().Be(5.0);
        analysis.AverageTimeToSell.Should().BeLessThan(1.0);
        analysis.RecentSales.Should().Be(100);
    }

    [Fact]
    public void MarketAnalysis_MarketTrendDescriptions_CanBeVaried()
    {
        // Arrange
        var trendDescriptions = new[]
        {
            "Rising", "Falling", "Stable", "Volatile", "Bullish", "Bearish",
            "Growing", "Shrinking", "Stagnant", "Recovering", "Declining", "Booming"
        };

        foreach (var trend in trendDescriptions)
        {
            // Act
            var analysis = new MarketAnalysis
            {
                ItemType = ItemType.Weapon,
                Quality = QualityTier.Common,
                MarketTrend = trend
            };

            // Assert
            analysis.MarketTrend.Should().Be(trend);
        }
    }

    [Fact]
    public void MarketAnalysis_RecommendedStrategy_CanBeDetailed()
    {
        // Act
        var analysis = new MarketAnalysis
        {
            ItemType = ItemType.Consumable,
            Quality = QualityTier.Rare,
            DemandLevel = 0.7,
            SupplyLevel = 0.6,
            PriceMultiplier = 1.2,
            MarketTrend = "Moderately rising",
            RecentSales = 12,
            AverageTimeToSell = 4.5,
            RecommendedStrategy = "Maintain current stock levels. Consider small price increase of 5-10%. Monitor competitor pricing weekly.",
            CompetitorPosition = "Slightly above average pricing"
        };

        // Assert
        analysis.RecommendedStrategy.Should().NotBeEmpty();
        analysis.RecommendedStrategy.Should().Contain("Maintain");
        analysis.RecommendedStrategy.Should().Contain("price increase");
        analysis.RecommendedStrategy.Should().Contain("Monitor");
    }

    [Fact]
    public void MarketAnalysis_CompetitorPosition_CanBeDescriptive()
    {
        // Arrange
        var competitorPositions = new[]
        {
            "Market leader",
            "Above market average",
            "Below market average",
            "Competitive",
            "Overpriced",
            "Underpriced",
            "Niche player",
            "Mass market",
            "Premium positioning",
            "Budget positioning"
        };

        foreach (var position in competitorPositions)
        {
            // Act
            var analysis = new MarketAnalysis
            {
                CompetitorPosition = position
            };

            // Assert
            analysis.CompetitorPosition.Should().Be(position);
        }
    }
}
