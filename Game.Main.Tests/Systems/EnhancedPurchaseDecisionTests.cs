#nullable enable

using FluentAssertions;
using Game.Main.Systems;
using Xunit;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Tests for enhanced purchase decision analysis system.
/// </summary>
public class EnhancedPurchaseDecisionTests
{
    [Fact]
    public void EnhancedPurchaseDecision_CanBeCreatedWithRequiredProperties()
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Buying,
            Confidence = 0.8f,
            PrimaryReason = "Good value for money",
            SecondaryFactors = new List<string> { "High quality", "Fair price" },
            NegotiationWillingness = 0.3f,
            AlternativeInterest = 0.1f,
            EmotionalResponse = CustomerEmotionalResponse.Satisfied,
            SuggestedAction = "Complete purchase immediately"
        };

        // Assert
        decision.Decision.Should().Be(PurchaseDecision.Buying);
        decision.Confidence.Should().Be(0.8f);
        decision.PrimaryReason.Should().Be("Good value for money");
        decision.SecondaryFactors.Should().Contain("High quality");
        decision.SecondaryFactors.Should().Contain("Fair price");
        decision.NegotiationWillingness.Should().Be(0.3f);
        decision.AlternativeInterest.Should().Be(0.1f);
        decision.EmotionalResponse.Should().Be(CustomerEmotionalResponse.Satisfied);
        decision.SuggestedAction.Should().Be("Complete purchase immediately");
    }

    [Fact]
    public void EnhancedPurchaseDecision_WithConsideringDecision_HasAppropriateValues()
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Considering,
            Confidence = 0.6f,
            PrimaryReason = "Unsure about value",
            SecondaryFactors = new List<string> { "Price seems high", "Quality uncertain" },
            NegotiationWillingness = 0.7f,
            AlternativeInterest = 0.5f,
            EmotionalResponse = CustomerEmotionalResponse.Conflicted,
            SuggestedAction = "Offer additional information or slight discount"
        };

        // Assert
        decision.Decision.Should().Be(PurchaseDecision.Considering);
        decision.Confidence.Should().Be(0.6f);
        decision.NegotiationWillingness.Should().Be(0.7f);
        decision.AlternativeInterest.Should().Be(0.5f);
        decision.EmotionalResponse.Should().Be(CustomerEmotionalResponse.Conflicted);
    }

    [Fact]
    public void EnhancedPurchaseDecision_WithNotBuyingDecision_HasAppropriateValues()
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.NotBuying,
            Confidence = 0.9f,
            PrimaryReason = "Price too high",
            SecondaryFactors = new List<string> { "Exceeds budget", "Better alternatives elsewhere" },
            NegotiationWillingness = 0.1f,
            AlternativeInterest = 0.8f,
            EmotionalResponse = CustomerEmotionalResponse.Frustrated,
            SuggestedAction = "Consider suggesting alternatives or future discounts"
        };

        // Assert
        decision.Decision.Should().Be(PurchaseDecision.NotBuying);
        decision.Confidence.Should().Be(0.9f);
        decision.EmotionalResponse.Should().Be(CustomerEmotionalResponse.Frustrated);
        decision.AlternativeInterest.Should().Be(0.8f);
    }

    [Fact]
    public void EnhancedPurchaseDecision_WithEmptySecondaryFactors_IsValid()
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Buying,
            Confidence = 0.7f,
            PrimaryReason = "Perfect fit",
            SecondaryFactors = new List<string>(),
            NegotiationWillingness = 0.0f,
            AlternativeInterest = 0.0f,
            EmotionalResponse = CustomerEmotionalResponse.Delighted,
            SuggestedAction = "Process sale immediately"
        };

        // Assert
        decision.SecondaryFactors.Should().BeEmpty();
        decision.Decision.Should().Be(PurchaseDecision.Buying);
        decision.EmotionalResponse.Should().Be(CustomerEmotionalResponse.Delighted);
    }

    [Fact]
    public void EnhancedPurchaseDecision_WithMultipleSecondaryFactors_StoresAllFactors()
    {
        // Arrange
        var factors = new List<string> 
        { 
            "Excellent craftsmanship", 
            "Matches current equipment", 
            "Reasonable price",
            "Good reputation of maker",
            "Immediate availability"
        };

        // Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Buying,
            Confidence = 0.95f,
            PrimaryReason = "Ideal item for needs",
            SecondaryFactors = factors,
            NegotiationWillingness = 0.2f,
            AlternativeInterest = 0.0f,
            EmotionalResponse = CustomerEmotionalResponse.Delighted,
            SuggestedAction = "Complete transaction with confidence"
        };

        // Assert
        decision.SecondaryFactors.Should().HaveCount(5);
        decision.SecondaryFactors.Should().ContainInOrder(factors);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.25f)]
    [InlineData(0.5f)]
    [InlineData(0.75f)]
    [InlineData(1.0f)]
    public void EnhancedPurchaseDecision_AcceptsValidConfidenceRanges(float confidence)
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Considering,
            Confidence = confidence,
            PrimaryReason = "Test reason",
            SecondaryFactors = new List<string>(),
            NegotiationWillingness = 0.5f,
            AlternativeInterest = 0.5f,
            EmotionalResponse = CustomerEmotionalResponse.Neutral,
            SuggestedAction = "Test action"
        };

        // Assert
        decision.Confidence.Should().Be(confidence);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.3f)]
    [InlineData(0.7f)]
    [InlineData(1.0f)]
    public void EnhancedPurchaseDecision_AcceptsValidNegotiationWillingness(float willingness)
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Considering,
            Confidence = 0.5f,
            PrimaryReason = "Test reason",
            SecondaryFactors = new List<string>(),
            NegotiationWillingness = willingness,
            AlternativeInterest = 0.5f,
            EmotionalResponse = CustomerEmotionalResponse.Neutral,
            SuggestedAction = "Test action"
        };

        // Assert
        decision.NegotiationWillingness.Should().Be(willingness);
    }

    [Theory]
    [InlineData(CustomerEmotionalResponse.Upset)]
    [InlineData(CustomerEmotionalResponse.Frustrated)]
    [InlineData(CustomerEmotionalResponse.Neutral)]
    [InlineData(CustomerEmotionalResponse.Satisfied)]
    [InlineData(CustomerEmotionalResponse.Delighted)]
    [InlineData(CustomerEmotionalResponse.Conflicted)]
    public void EnhancedPurchaseDecision_AcceptsAllEmotionalResponses(CustomerEmotionalResponse response)
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Considering,
            Confidence = 0.5f,
            PrimaryReason = "Test reason",
            SecondaryFactors = new List<string>(),
            NegotiationWillingness = 0.5f,
            AlternativeInterest = 0.5f,
            EmotionalResponse = response,
            SuggestedAction = "Test action"
        };

        // Assert
        decision.EmotionalResponse.Should().Be(response);
    }

    [Theory]
    [InlineData(PurchaseDecision.Buying)]
    [InlineData(PurchaseDecision.Considering)]
    [InlineData(PurchaseDecision.NotBuying)]
    public void EnhancedPurchaseDecision_AcceptsAllPurchaseDecisions(PurchaseDecision purchaseDecision)
    {
        // Arrange & Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = purchaseDecision,
            Confidence = 0.5f,
            PrimaryReason = "Test reason",
            SecondaryFactors = new List<string>(),
            NegotiationWillingness = 0.5f,
            AlternativeInterest = 0.5f,
            EmotionalResponse = CustomerEmotionalResponse.Neutral,
            SuggestedAction = "Test action"
        };

        // Assert
        decision.Decision.Should().Be(purchaseDecision);
    }

    [Fact]
    public void EnhancedPurchaseDecision_SuggestedAction_CanBeDetailedString()
    {
        // Arrange
        var detailedAction = "Highlight the superior craftsmanship, offer a 5% discount for immediate purchase, " +
                           "and mention the limited availability to create urgency.";

        // Act
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.Considering,
            Confidence = 0.6f,
            PrimaryReason = "Quality concerns",
            SecondaryFactors = new List<string> { "Price hesitation", "Comparison shopping" },
            NegotiationWillingness = 0.4f,
            AlternativeInterest = 0.3f,
            EmotionalResponse = CustomerEmotionalResponse.Conflicted,
            SuggestedAction = detailedAction
        };

        // Assert
        decision.SuggestedAction.Should().Be(detailedAction);
    }

    [Fact]
    public void EnhancedPurchaseDecision_AlternativeInterest_ReflectsCustomerBehavior()
    {
        // Arrange & Act - High alternative interest suggests customer wants to compare
        var decision = new EnhancedPurchaseDecision
        {
            Decision = PurchaseDecision.NotBuying,
            Confidence = 0.7f,
            PrimaryReason = "Want to see other options",
            SecondaryFactors = new List<string> { "Comparison shopping", "Not urgent need" },
            NegotiationWillingness = 0.6f,
            AlternativeInterest = 0.9f, // Very interested in alternatives
            EmotionalResponse = CustomerEmotionalResponse.Neutral,
            SuggestedAction = "Show similar items with different price points"
        };

        // Assert
        decision.AlternativeInterest.Should().Be(0.9f);
        decision.Decision.Should().Be(PurchaseDecision.NotBuying);
    }
}
