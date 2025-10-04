#nullable enable

using FluentAssertions;
using Game.Main.Systems;
using Game.Shop.Systems;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Tests for shop interaction context tracking system.
/// </summary>
public class ShopInteractionContextTests
{
    [Fact]
    public void ShopInteractionContext_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var context = new ShopInteractionContext();

        // Assert
        context.InteractionQualityScore.Should().Be(0.5f);
        context.ShopReputationScore.Should().Be(0.5f);
        context.ShopAmbianceScore.Should().Be(0.5f);
        context.OtherCustomersSatisfaction.Should().Be(0.5f);
        context.AlternativeItemsAvailable.Should().BeTrue();
        context.TotalInteractions.Should().Be(0);
        context.DiscountOffered.Should().BeFalse();
        context.NegotiationAttempted.Should().BeFalse();
    }

    [Fact]
    public void ShopInteractionContext_CanSetInteractionQualityScore()
    {
        // Arrange
        var context = new ShopInteractionContext();
        var newScore = 0.8f;

        // Act
        context.InteractionQualityScore = newScore;

        // Assert
        context.InteractionQualityScore.Should().Be(newScore);
    }

    [Fact]
    public void ShopInteractionContext_CanSetShopReputationScore()
    {
        // Arrange
        var context = new ShopInteractionContext();
        var newScore = 0.9f;

        // Act
        context.ShopReputationScore = newScore;

        // Assert
        context.ShopReputationScore.Should().Be(newScore);
    }

    [Fact]
    public void ShopInteractionContext_CanSetShopAmbianceScore()
    {
        // Arrange
        var context = new ShopInteractionContext();
        var newScore = 0.7f;

        // Act
        context.ShopAmbianceScore = newScore;

        // Assert
        context.ShopAmbianceScore.Should().Be(newScore);
    }

    [Fact]
    public void ShopInteractionContext_CanSetOtherCustomersSatisfaction()
    {
        // Arrange
        var context = new ShopInteractionContext();
        var newScore = 0.6f;

        // Act
        context.OtherCustomersSatisfaction = newScore;

        // Assert
        context.OtherCustomersSatisfaction.Should().Be(newScore);
    }

    [Fact]
    public void ShopInteractionContext_CanSetAlternativeItemsAvailable()
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act
        context.AlternativeItemsAvailable = false;

        // Assert
        context.AlternativeItemsAvailable.Should().BeFalse();
    }

    [Fact]
    public void ShopInteractionContext_CanIncrementTotalInteractions()
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act
        context.TotalInteractions++;
        context.TotalInteractions++;

        // Assert
        context.TotalInteractions.Should().Be(2);
    }

    [Fact]
    public void ShopInteractionContext_CanSetDiscountOffered()
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act
        context.DiscountOffered = true;

        // Assert
        context.DiscountOffered.Should().BeTrue();
    }

    [Fact]
    public void ShopInteractionContext_CanSetNegotiationAttempted()
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act
        context.NegotiationAttempted = true;

        // Assert
        context.NegotiationAttempted.Should().BeTrue();
    }

    [Fact]
    public void ShopInteractionContext_AllPropertiesCanBeModified()
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act
        context.InteractionQualityScore = 0.95f;
        context.ShopReputationScore = 0.85f;
        context.ShopAmbianceScore = 0.75f;
        context.OtherCustomersSatisfaction = 0.65f;
        context.AlternativeItemsAvailable = false;
        context.TotalInteractions = 5;
        context.DiscountOffered = true;
        context.NegotiationAttempted = true;

        // Assert
        context.InteractionQualityScore.Should().Be(0.95f);
        context.ShopReputationScore.Should().Be(0.85f);
        context.ShopAmbianceScore.Should().Be(0.75f);
        context.OtherCustomersSatisfaction.Should().Be(0.65f);
        context.AlternativeItemsAvailable.Should().BeFalse();
        context.TotalInteractions.Should().Be(5);
        context.DiscountOffered.Should().BeTrue();
        context.NegotiationAttempted.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.25f)]
    [InlineData(0.5f)]
    [InlineData(0.75f)]
    [InlineData(1.0f)]
    public void ShopInteractionContext_AcceptsValidScoreRanges(float score)
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act & Assert - Should not throw exceptions
        context.InteractionQualityScore = score;
        context.ShopReputationScore = score;
        context.ShopAmbianceScore = score;
        context.OtherCustomersSatisfaction = score;

        context.InteractionQualityScore.Should().Be(score);
        context.ShopReputationScore.Should().Be(score);
        context.ShopAmbianceScore.Should().Be(score);
        context.OtherCustomersSatisfaction.Should().Be(score);
    }

    [Fact]
    public void ShopInteractionContext_CanTrackNegativeInteractions()
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act
        context.InteractionQualityScore = 0.1f; // Poor interaction
        context.ShopAmbianceScore = 0.2f; // Poor ambiance
        context.DiscountOffered = false;
        context.NegotiationAttempted = false;

        // Assert
        context.InteractionQualityScore.Should().Be(0.1f);
        context.ShopAmbianceScore.Should().Be(0.2f);
        context.DiscountOffered.Should().BeFalse();
        context.NegotiationAttempted.Should().BeFalse();
    }

    [Fact]
    public void ShopInteractionContext_CanTrackPositiveInteractions()
    {
        // Arrange
        var context = new ShopInteractionContext();

        // Act
        context.InteractionQualityScore = 0.9f; // Excellent interaction
        context.ShopAmbianceScore = 0.95f; // Excellent ambiance
        context.DiscountOffered = true;
        context.NegotiationAttempted = true;

        // Assert
        context.InteractionQualityScore.Should().Be(0.9f);
        context.ShopAmbianceScore.Should().Be(0.95f);
        context.DiscountOffered.Should().BeTrue();
        context.NegotiationAttempted.Should().BeTrue();
    }

    [Fact]
    public void ShopInteractionContext_TotalInteractions_CanBeSetToLargeNumbers()
    {
        // Arrange
        var context = new ShopInteractionContext();
        var largeNumber = 1000;

        // Act
        context.TotalInteractions = largeNumber;

        // Assert
        context.TotalInteractions.Should().Be(largeNumber);
    }

    [Fact]
    public void ShopInteractionContext_IndependentPropertyModification()
    {
        // Arrange
        var context = new ShopInteractionContext();
        var originalReputationScore = context.ShopReputationScore;

        // Act - Modify one property
        context.InteractionQualityScore = 0.8f;

        // Assert - Other properties remain unchanged
        context.ShopReputationScore.Should().Be(originalReputationScore);
        context.ShopAmbianceScore.Should().Be(0.5f);
        context.OtherCustomersSatisfaction.Should().Be(0.5f);
        context.AlternativeItemsAvailable.Should().BeTrue();
        context.TotalInteractions.Should().Be(0);
        context.DiscountOffered.Should().BeFalse();
        context.NegotiationAttempted.Should().BeFalse();
    }
}
