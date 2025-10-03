using Game.Item.Models;

namespace Game.Main.Tests.Models;

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