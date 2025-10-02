#nullable enable

using FluentAssertions;
using Game.Main.Models;
using Xunit;

namespace Game.Main.Tests.Models;

public class CompetitiveIntelligenceTests
{
    [Fact]
    public void CompetitorCount_ReturnsCorrectCount()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            CompetitorPrices = new List<decimal> { 10m, 15m, 20m }
        };

        // Act & Assert
        intelligence.CompetitorCount.Should().Be(3);
    }

    [Fact]
    public void CompetitorCount_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence();

        // Act & Assert
        intelligence.CompetitorCount.Should().Be(0);
    }

    [Theory]
    [InlineData(12.0, 15.0, 0.08, true)]  // Within 10%
    [InlineData(12.0, 15.0, 0.05, true)] // Within 10%
    [InlineData(12.0, 15.0, 0.15, false)] // Outside 10%
    [InlineData(12.0, 15.0, -0.08, true)] // Within 10% negative
    [InlineData(12.0, 15.0, -0.12, false)] // Outside 10% negative
    public void IsCompetitivelyPriced_ReturnsCorrectResult(decimal ourPrice, decimal avgPrice, decimal priceAdvantage, bool expected)
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            OurPrice = ourPrice,
            AverageCompetitorPrice = avgPrice,
            PriceAdvantage = priceAdvantage
        };

        // Act & Assert
        intelligence.IsCompetitivelyPriced.Should().Be(expected);
    }

    [Fact]
    public void IsLowestPriced_WhenOurPriceIsLowest_ReturnsTrue()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            OurPrice = 10m,
            LowestCompetitorPrice = 12m,
            CompetitorPrices = new List<decimal> { 12m, 15m, 18m }
        };

        // Act & Assert
        intelligence.IsLowestPriced.Should().BeTrue();
    }

    [Fact]
    public void IsLowestPriced_WhenOurPriceIsNotLowest_ReturnsFalse()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            OurPrice = 15m,
            LowestCompetitorPrice = 12m,
            CompetitorPrices = new List<decimal> { 12m, 15m, 18m }
        };

        // Act & Assert
        intelligence.IsLowestPriced.Should().BeFalse();
    }

    [Fact]
    public void IsLowestPriced_WhenNoCompetitors_ReturnsFalse()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            OurPrice = 10m,
            CompetitorPrices = new List<decimal>()
        };

        // Act & Assert
        intelligence.IsLowestPriced.Should().BeFalse();
    }

    [Fact]
    public void IsHighestPriced_WhenOurPriceIsHighest_ReturnsTrue()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            OurPrice = 20m,
            HighestCompetitorPrice = 18m,
            CompetitorPrices = new List<decimal> { 12m, 15m, 18m }
        };

        // Act & Assert
        intelligence.IsHighestPriced.Should().BeTrue();
    }

    [Fact]
    public void IsHighestPriced_WhenOurPriceIsNotHighest_ReturnsFalse()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            OurPrice = 15m,
            HighestCompetitorPrice = 18m,
            CompetitorPrices = new List<decimal> { 12m, 15m, 18m }
        };

        // Act & Assert
        intelligence.IsHighestPriced.Should().BeFalse();
    }

    [Fact]
    public void IsHighestPriced_WhenNoCompetitors_ReturnsFalse()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            OurPrice = 20m,
            CompetitorPrices = new List<decimal>()
        };

        // Act & Assert
        intelligence.IsHighestPriced.Should().BeFalse();
    }

    [Fact]
    public void GetPricingRecommendation_WhenNoCompetition_ReturnsMonopolyMessage()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            CompetitorPrices = new List<decimal>()
        };

        // Act
        var recommendation = intelligence.GetPricingRecommendation();

        // Assert
        recommendation.Should().Be("No competition detected - consider premium pricing");
    }

    [Theory]
    [InlineData(MarketPosition.Premium, "Consider slight price reduction to increase volume")]
    [InlineData(MarketPosition.AboveAverage, "Well positioned for quality-conscious customers")]
    [InlineData(MarketPosition.Average, "Competitive positioning - monitor competitor changes")]
    [InlineData(MarketPosition.BelowAverage, "Good value positioning - consider volume strategies")]
    [InlineData(MarketPosition.Discount, "Aggressive pricing - ensure profitability")]
    [InlineData(MarketPosition.Monopoly, "Market leader - optimize for maximum profit")]
    public void GetPricingRecommendation_ReturnsCorrectMessage(MarketPosition position, string expectedMessage)
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            MarketPosition = position,
            CompetitorPrices = new List<decimal> { 10m, 15m } // Non-empty to avoid monopoly path
        };

        // Act
        var recommendation = intelligence.GetPricingRecommendation();

        // Assert
        recommendation.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(MarketPosition.Premium, 0.15)]
    [InlineData(MarketPosition.AboveAverage, 0.25)]
    [InlineData(MarketPosition.Average, 0.30)]
    [InlineData(MarketPosition.BelowAverage, 0.35)]
    [InlineData(MarketPosition.Discount, 0.45)]
    [InlineData(MarketPosition.Monopoly, 1.0)]
    public void EstimatedMarketShare_ReturnsCorrectShare(MarketPosition position, double expectedShare)
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            MarketPosition = position,
            CompetitorPrices = new List<decimal> { 10m, 15m } // Non-empty for non-monopoly cases
        };

        // Act
        var marketShare = intelligence.EstimatedMarketShare();

        // Assert
        marketShare.Should().Be(expectedShare);
    }

    [Fact]
    public void EstimatedMarketShare_WhenNoCompetitors_ReturnsFullShare()
    {
        // Arrange
        var intelligence = new CompetitiveIntelligence
        {
            CompetitorPrices = new List<decimal>()
        };

        // Act
        var marketShare = intelligence.EstimatedMarketShare();

        // Assert
        marketShare.Should().Be(1.0);
    }

    [Fact]
    public void CompetitiveIntelligence_CanBeInitializedWithProperties()
    {
        // Arrange & Act
        var intelligence = new CompetitiveIntelligence
        {
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Rare,
            OurPrice = 100m,
            CompetitorPrices = new List<decimal> { 90m, 110m, 95m },
            CompetitorNames = new List<string> { "Shop A", "Shop B", "Shop C" },
            AverageCompetitorPrice = 98.33m,
            LowestCompetitorPrice = 90m,
            HighestCompetitorPrice = 110m,
            PriceAdvantage = 1.67m,
            MarketPosition = MarketPosition.AboveAverage
        };

        // Assert
        intelligence.ItemType.Should().Be(ItemType.Weapon);
        intelligence.QualityTier.Should().Be(QualityTier.Rare);
        intelligence.OurPrice.Should().Be(100m);
        intelligence.CompetitorPrices.Should().HaveCount(3);
        intelligence.CompetitorNames.Should().HaveCount(3);
        intelligence.AverageCompetitorPrice.Should().Be(98.33m);
        intelligence.LowestCompetitorPrice.Should().Be(90m);
        intelligence.HighestCompetitorPrice.Should().Be(110m);
        intelligence.PriceAdvantage.Should().Be(1.67m);
        intelligence.MarketPosition.Should().Be(MarketPosition.AboveAverage);
    }
}
