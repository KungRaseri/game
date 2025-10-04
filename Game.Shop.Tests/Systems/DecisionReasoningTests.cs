#nullable enable

using FluentAssertions;
using Game.Shop.Systems;

namespace Game.Shop.Tests.Systems;

/// <summary>
/// Tests for decision reasoning data class.
/// </summary>
public class DecisionReasoningTests
{
    [Fact]
    public void DecisionReasoning_CanBeCreatedWithRequiredProperties()
    {
        // Arrange & Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Price is too high",
            SecondaryFactors = new List<string> { "Quality seems good", "Brand reputation" }
        };

        // Assert
        reasoning.PrimaryReason.Should().Be("Price is too high");
        reasoning.SecondaryFactors.Should().Contain("Quality seems good");
        reasoning.SecondaryFactors.Should().Contain("Brand reputation");
        reasoning.SecondaryFactors.Should().HaveCount(2);
    }

    [Fact]
    public void DecisionReasoning_WithEmptySecondaryFactors_IsValid()
    {
        // Arrange & Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Perfect match for needs",
            SecondaryFactors = new List<string>()
        };

        // Assert
        reasoning.PrimaryReason.Should().Be("Perfect match for needs");
        reasoning.SecondaryFactors.Should().BeEmpty();
    }

    [Fact]
    public void DecisionReasoning_WithMultipleSecondaryFactors_StoresAllFactors()
    {
        // Arrange
        var factors = new List<string>
        {
            "Excellent craftsmanship",
            "Fair pricing",
            "Good warranty",
            "Positive reviews",
            "Immediate availability"
        };

        // Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Overall excellent value",
            SecondaryFactors = factors
        };

        // Assert
        reasoning.SecondaryFactors.Should().HaveCount(5);
        reasoning.SecondaryFactors.Should().ContainInOrder(factors);
    }

    [Fact]
    public void DecisionReasoning_PrimaryReason_CanBeModified()
    {
        // Arrange
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Initial reason",
            SecondaryFactors = new List<string>()
        };

        // Act
        reasoning.PrimaryReason = "Updated reason";

        // Assert
        reasoning.PrimaryReason.Should().Be("Updated reason");
    }

    [Fact]
    public void DecisionReasoning_SecondaryFactors_CanBeModified()
    {
        // Arrange
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Main reason",
            SecondaryFactors = new List<string> { "Factor 1" }
        };

        // Act
        reasoning.SecondaryFactors.Add("Factor 2");
        reasoning.SecondaryFactors.Add("Factor 3");

        // Assert
        reasoning.SecondaryFactors.Should().HaveCount(3);
        reasoning.SecondaryFactors.Should().Contain("Factor 1");
        reasoning.SecondaryFactors.Should().Contain("Factor 2");
        reasoning.SecondaryFactors.Should().Contain("Factor 3");
    }

    [Fact]
    public void DecisionReasoning_SecondaryFactors_CanBeRemoved()
    {
        // Arrange
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Main reason",
            SecondaryFactors = new List<string> { "Factor 1", "Factor 2", "Factor 3" }
        };

        // Act
        reasoning.SecondaryFactors.Remove("Factor 2");

        // Assert
        reasoning.SecondaryFactors.Should().HaveCount(2);
        reasoning.SecondaryFactors.Should().Contain("Factor 1");
        reasoning.SecondaryFactors.Should().Contain("Factor 3");
        reasoning.SecondaryFactors.Should().NotContain("Factor 2");
    }

    [Fact]
    public void DecisionReasoning_SecondaryFactors_CanBeCleared()
    {
        // Arrange
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Main reason",
            SecondaryFactors = new List<string> { "Factor 1", "Factor 2" }
        };

        // Act
        reasoning.SecondaryFactors.Clear();

        // Assert
        reasoning.SecondaryFactors.Should().BeEmpty();
        reasoning.PrimaryReason.Should().Be("Main reason"); // Should not affect primary reason
    }

    [Theory]
    [InlineData("")]
    [InlineData("Simple reason")]
    [InlineData("Very detailed reason explaining the customer's thought process and concerns")]
    public void DecisionReasoning_AcceptsVariousReasonLengths(string reason)
    {
        // Arrange & Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = reason,
            SecondaryFactors = new List<string>()
        };

        // Assert
        reasoning.PrimaryReason.Should().Be(reason);
    }

    [Fact]
    public void DecisionReasoning_WithPositiveReasons_ReflectsPositiveDecision()
    {
        // Arrange & Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Excellent value for money",
            SecondaryFactors = new List<string>
            {
                "High quality materials",
                "Fair pricing",
                "Good reputation",
                "Exactly what I need"
            }
        };

        // Assert
        reasoning.PrimaryReason.Should().Contain("Excellent");
        reasoning.SecondaryFactors.Should().AllSatisfy(factor =>
            factor.Should().Match(f =>
                f.Contains("quality") || f.Contains("Fair") || f.Contains("Good") || f.Contains("need")));
    }

    [Fact]
    public void DecisionReasoning_WithNegativeReasons_ReflectsNegativeDecision()
    {
        // Arrange & Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Price is too expensive",
            SecondaryFactors = new List<string>
            {
                "Exceeds budget",
                "Better options available elsewhere",
                "Uncertain about quality"
            }
        };

        // Assert
        reasoning.PrimaryReason.Should().Contain("expensive");
        reasoning.SecondaryFactors.Should().Contain(factor => factor.Contains("budget"));
        reasoning.SecondaryFactors.Should().Contain(factor => factor.Contains("Better options"));
    }

    [Fact]
    public void DecisionReasoning_WithMixedReasons_ReflectsConflictedDecision()
    {
        // Arrange & Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Unsure about the purchase",
            SecondaryFactors = new List<string>
            {
                "Good quality but high price",
                "Like the style but concerned about durability",
                "Need the item but have budget constraints"
            }
        };

        // Assert
        reasoning.PrimaryReason.Should().Contain("Unsure");
        reasoning.SecondaryFactors.Should().AllSatisfy(factor =>
            factor.Should().Match(f => f.Contains("but") || f.Contains("concerned")));
    }

    [Fact]
    public void DecisionReasoning_SecondaryFactors_AllowsDuplicates()
    {
        // Arrange & Act
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Multiple similar concerns",
            SecondaryFactors = new List<string>
            {
                "Price is high",
                "Price is high",
                "Different concern"
            }
        };

        // Assert
        reasoning.SecondaryFactors.Should().HaveCount(3);
        reasoning.SecondaryFactors.Should().Contain("Price is high");
        reasoning.SecondaryFactors.Where(f => f == "Price is high").Should().HaveCount(2);
    }

    [Fact]
    public void DecisionReasoning_PropertiesAreIndependent()
    {
        // Arrange
        var reasoning = new DecisionReasoning
        {
            PrimaryReason = "Original reason",
            SecondaryFactors = new List<string> { "Original factor" }
        };

        // Act
        reasoning.PrimaryReason = "New reason";

        // Assert
        reasoning.PrimaryReason.Should().Be("New reason");
        reasoning.SecondaryFactors.Should().Contain("Original factor"); // Should remain unchanged
    }
}