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