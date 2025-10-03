using Game.Item.Models;

namespace Game.Main.Tests.Models;

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