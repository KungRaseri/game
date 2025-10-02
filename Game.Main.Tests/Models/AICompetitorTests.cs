#nullable enable

using FluentAssertions;
using Game.Main.Models;
using System;
using Xunit;

namespace Game.Main.Tests.Models;

public class AICompetitorTests
{
    [Fact]
    public void SellsItem_WhenItemExists_ReturnsTrue()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified
        };
        competitor.ItemPrices[(ItemType.Weapon, QualityTier.Common)] = 50m;

        // Act
        var result = competitor.SellsItem(ItemType.Weapon, QualityTier.Common);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SellsItem_WhenItemDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified
        };

        // Act
        var result = competitor.SellsItem(ItemType.Weapon, QualityTier.Common);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPriceFor_WhenItemExists_ReturnsPrice()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified
        };
        competitor.ItemPrices[(ItemType.Armor, QualityTier.Rare)] = 120m;

        // Act
        var price = competitor.GetPriceFor(ItemType.Armor, QualityTier.Rare);

        // Assert
        price.Should().Be(120m);
    }

    [Fact]
    public void GetPriceFor_WhenItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified
        };

        // Act
        var price = competitor.GetPriceFor(ItemType.Material, QualityTier.Epic);

        // Assert
        price.Should().BeNull();
    }

    [Fact]
    public void UpdateStrategy_UpdatesStrategyAndLastUpdated()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified,
            Strategy = CompetitorStrategy.LowPrice
        };
        var originalLastUpdated = competitor.LastUpdated;

        // Wait a small amount to ensure time difference
        System.Threading.Thread.Sleep(1);

        // Act
        competitor.UpdateStrategy(CompetitorStrategy.Premium);

        // Assert
        competitor.Strategy.Should().Be(CompetitorStrategy.Premium);
        competitor.LastUpdated.Should().BeAfter(originalLastUpdated);
    }

    [Fact]
    public void RecordAction_AddsActionToHistory()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified
        };
        var action = new CompetitorAction
        {
            CompetitorId = "comp1",
            ActionType = CompetitorActionType.PriceReduction,
            Description = "Reduced weapon prices by 10%"
        };

        // Act
        competitor.RecordAction(action);

        // Assert
        competitor.ActionHistory.Should().Contain(action);
        competitor.ActionHistory.Should().HaveCount(1);
    }

    [Fact]
    public void RecordAction_UpdatesLastUpdated()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified
        };
        var originalLastUpdated = competitor.LastUpdated;
        var action = new CompetitorAction
        {
            CompetitorId = "comp1",
            ActionType = CompetitorActionType.InventoryRestock,
            Description = "Restocked armor inventory"
        };

        // Wait a small amount to ensure time difference
        System.Threading.Thread.Sleep(1);

        // Act
        competitor.RecordAction(action);

        // Assert
        competitor.LastUpdated.Should().BeAfter(originalLastUpdated);
    }

    [Fact]
    public void RecordAction_LimitsHistoryTo50Items()
    {
        // Arrange
        var competitor = new AICompetitor
        {
            CompetitorId = "comp1",
            Name = "Test Shop",
            InventoryStrategy = InventoryStrategy.Diversified
        };

        // Act - Add 60 actions
        for (int i = 0; i < 60; i++)
        {
            var action = new CompetitorAction
            {
                CompetitorId = "comp1",
                ActionType = CompetitorActionType.PriceReduction,
                Description = $"Action {i}"
            };
            competitor.RecordAction(action);
        }

        // Assert
        competitor.ActionHistory.Should().HaveCount(50);
        competitor.ActionHistory[0].Description.Should().Be("Action 10"); // First 10 removed
        competitor.ActionHistory[49].Description.Should().Be("Action 59"); // Last one added
    }

    [Fact]
    public void AICompetitor_CanBeCreatedWithRequiredProperties()
    {
        // Act
        var competitor = new AICompetitor
        {
            CompetitorId = "shop123",
            Name = "Elite Armory",
            InventoryStrategy = InventoryStrategy.HighEnd,
            Strategy = CompetitorStrategy.Premium,
            Aggressiveness = 0.8,
            PerformanceScore = 0.75,
            PricingModifier = 1.25
        };

        // Assert
        competitor.CompetitorId.Should().Be("shop123");
        competitor.Name.Should().Be("Elite Armory");
        competitor.InventoryStrategy.Should().Be(InventoryStrategy.HighEnd);
        competitor.Strategy.Should().Be(CompetitorStrategy.Premium);
        competitor.Aggressiveness.Should().Be(0.8);
        competitor.PerformanceScore.Should().Be(0.75);
        competitor.PricingModifier.Should().Be(1.25);
        competitor.ItemPrices.Should().NotBeNull().And.BeEmpty();
        competitor.ActionHistory.Should().NotBeNull().And.BeEmpty();
        competitor.LastUpdated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
}

public class CompetitionAnalysisTests
{
    [Fact]
    public void CompetitionAnalysis_CanBeCreated()
    {
        // Act
        var analysis = new CompetitionAnalysis
        {
            TotalCompetitors = 5,
            AverageCompetitorPerformance = 0.6,
            MarketShare = 0.3,
            CompetitivePressure = 0.7
        };

        // Assert
        analysis.TotalCompetitors.Should().Be(5);
        analysis.AverageCompetitorPerformance.Should().Be(0.6);
        analysis.MarketShare.Should().Be(0.3);
        analysis.CompetitivePressure.Should().Be(0.7);
        analysis.Threats.Should().NotBeNull().And.BeEmpty();
        analysis.Opportunities.Should().NotBeNull().And.BeEmpty();
        analysis.RecommendedActions.Should().NotBeNull().And.BeEmpty();
        analysis.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0.2, 0.08, "Low competitive pressure, Niche player (8.0% market share)")]
    [InlineData(0.5, 0.15, "Moderate competitive pressure, Growing presence (15.0% market share)")]
    [InlineData(0.7, 0.4, "High competitive pressure, Strong competitor (40.0% market share)")]
    [InlineData(0.9, 0.6, "Intense competitive pressure, Market leader (60.0% market share)")]
    [InlineData(0.6, 0.8, "Moderate competitive pressure, Dominant position (80.0% market share)")]
    public void GetCompetitiveLandscapeDescription_ReturnsCorrectDescription(double pressure, double marketShare, string expected)
    {
        // Arrange
        var analysis = new CompetitionAnalysis
        {
            CompetitivePressure = pressure,
            MarketShare = marketShare
        };

        // Act
        var description = analysis.GetCompetitiveLandscapeDescription();

        // Assert
        description.Should().Be(expected);
    }

    [Fact]
    public void CompetitionAnalysis_CanAddThreatsAndOpportunities()
    {
        // Arrange
        var analysis = new CompetitionAnalysis();

        // Act
        analysis.Threats.Add("New competitor with aggressive pricing");
        analysis.Opportunities.Add("Competitor exit creates market gap");
        analysis.RecommendedActions.Add("Reduce prices on weapons category");

        // Assert
        analysis.Threats.Should().Contain("New competitor with aggressive pricing");
        analysis.Opportunities.Should().Contain("Competitor exit creates market gap");
        analysis.RecommendedActions.Should().Contain("Reduce prices on weapons category");
    }
}

public class CompetitorReactionTests
{
    [Fact]
    public void CompetitorReaction_CanBeCreated()
    {
        // Act
        var reaction = new CompetitorReaction
        {
            CompetitorId = "comp1",
            ItemType = ItemType.Weapon,
            QualityTier = QualityTier.Common,
            ReactionType = CompetitorReactionType.UnderCut,
            PriceMultiplier = 0.9,
            Reason = "Player reduced prices"
        };

        // Assert
        reaction.CompetitorId.Should().Be("comp1");
        reaction.ItemType.Should().Be(ItemType.Weapon);
        reaction.QualityTier.Should().Be(QualityTier.Common);
        reaction.ReactionType.Should().Be(CompetitorReactionType.UnderCut);
        reaction.PriceMultiplier.Should().Be(0.9);
        reaction.Reason.Should().Be("Player reduced prices");
        reaction.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompetitorReaction_HasDefaultValues()
    {
        // Act
        var reaction = new CompetitorReaction
        {
            CompetitorId = "comp1"
        };

        // Assert
        reaction.PriceMultiplier.Should().Be(1.0);
        reaction.ReactionType.Should().Be(CompetitorReactionType.NoChange);
        reaction.ItemType.Should().Be(ItemType.Weapon); // Default enum value
        reaction.QualityTier.Should().Be(QualityTier.Common); // Default enum value
    }
}

public class CompetitorActionTests
{
    [Fact]
    public void CompetitorAction_CanBeCreated()
    {
        // Act
        var action = new CompetitorAction
        {
            CompetitorId = "comp1",
            ActionType = CompetitorActionType.PriceReduction,
            Description = "Reduced all weapon prices by 15%",
            ImpactMagnitude = 0.8
        };

        // Assert
        action.CompetitorId.Should().Be("comp1");
        action.ActionType.Should().Be(CompetitorActionType.PriceReduction);
        action.Description.Should().Be("Reduced all weapon prices by 15%");
        action.ImpactMagnitude.Should().Be(0.8);
        action.AffectedItems.Should().NotBeNull().And.BeEmpty();
        action.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompetitorAction_HasDefaultImpactMagnitude()
    {
        // Act
        var action = new CompetitorAction
        {
            CompetitorId = "comp1",
            ActionType = CompetitorActionType.InventoryRestock,
            Description = "Restocked inventory"
        };

        // Assert
        action.ImpactMagnitude.Should().Be(0.5);
    }

    [Fact]
    public void CompetitorAction_CanHaveAffectedItems()
    {
        // Act
        var action = new CompetitorAction
        {
            CompetitorId = "comp1",
            ActionType = CompetitorActionType.QualityUpgrade,
            Description = "Improved weapon quality"
        };
        action.AffectedItems.Add((ItemType.Weapon, QualityTier.Common));
        action.AffectedItems.Add((ItemType.Weapon, QualityTier.Uncommon));

        // Assert
        action.AffectedItems.Should().HaveCount(2);
        action.AffectedItems.Should().Contain((ItemType.Weapon, QualityTier.Common));
        action.AffectedItems.Should().Contain((ItemType.Weapon, QualityTier.Uncommon));
    }
}
