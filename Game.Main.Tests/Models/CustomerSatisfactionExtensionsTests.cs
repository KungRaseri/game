#nullable enable

using FluentAssertions;
using Game.Main.Models;
using Xunit;

namespace Game.Main.Tests.Models;

public class CustomerSatisfactionExtensionsTests
{
    [Theory]
    [InlineData(CustomerSatisfaction.Angry, "Very Disappointed")]
    [InlineData(CustomerSatisfaction.Disappointed, "Disappointed")]
    [InlineData(CustomerSatisfaction.Neutral, "Neutral")]
    [InlineData(CustomerSatisfaction.Satisfied, "Happy")]
    [InlineData(CustomerSatisfaction.Delighted, "Delighted")]
    public void GetDescription_ReturnsCorrectDescription(CustomerSatisfaction satisfaction, string expectedDescription)
    {
        // Act
        var description = satisfaction.GetDescription();

        // Assert
        description.Should().Be(expectedDescription);
    }

    [Fact]
    public void GetDescription_WithInvalidValue_ReturnsUnknown()
    {
        // Arrange
        var invalidSatisfaction = (CustomerSatisfaction)999;

        // Act
        var description = invalidSatisfaction.GetDescription();

        // Assert
        description.Should().Be("Unknown");
    }

    [Theory]
    [InlineData(CustomerSatisfaction.Angry, 1)]
    [InlineData(CustomerSatisfaction.Disappointed, 2)]
    [InlineData(CustomerSatisfaction.Neutral, 3)]
    [InlineData(CustomerSatisfaction.Satisfied, 4)]
    [InlineData(CustomerSatisfaction.Delighted, 5)]
    public void GetScore_ReturnsCorrectNumericValue(CustomerSatisfaction satisfaction, int expectedScore)
    {
        // Act
        var score = satisfaction.GetScore();

        // Assert
        score.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData(CustomerSatisfaction.Angry, false)]
    [InlineData(CustomerSatisfaction.Disappointed, false)]
    [InlineData(CustomerSatisfaction.Neutral, false)]
    [InlineData(CustomerSatisfaction.Satisfied, true)]
    [InlineData(CustomerSatisfaction.Delighted, true)]
    public void IsPositive_ReturnsCorrectResult(CustomerSatisfaction satisfaction, bool expectedResult)
    {
        // Act
        var isPositive = satisfaction.IsPositive();

        // Assert
        isPositive.Should().Be(expectedResult);
    }
}
