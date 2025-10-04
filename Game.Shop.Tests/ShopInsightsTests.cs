#nullable enable

using FluentAssertions;

namespace Game.Shop.Tests;

public class ShopInsightsTests
{
    [Fact]
    public void ShopInsights_CanBeInstantiated()
    {
        // Act
        var insights = new ShopInsights();

        // Assert
        insights.Should().NotBeNull();
        insights.Recommendations.Should().NotBeNull().And.BeEmpty();
        insights.Achievements.Should().NotBeNull().And.BeEmpty();
        insights.Concerns.Should().NotBeNull().And.BeEmpty();
        insights.NextSteps.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ShopInsights_CanAddRecommendations()
    {
        // Arrange
        var insights = new ShopInsights();
        var recommendation = "Consider reducing prices on slow-moving inventory";

        // Act
        insights.Recommendations.Add(recommendation);

        // Assert
        insights.Recommendations.Should().Contain(recommendation);
        insights.Recommendations.Should().HaveCount(1);
    }

    [Fact]
    public void ShopInsights_CanAddAchievements()
    {
        // Arrange
        var insights = new ShopInsights();
        var achievement = "Sales increased by 15% this month";

        // Act
        insights.Achievements.Add(achievement);

        // Assert
        insights.Achievements.Should().Contain(achievement);
        insights.Achievements.Should().HaveCount(1);
    }

    [Fact]
    public void ShopInsights_CanAddConcerns()
    {
        // Arrange
        var insights = new ShopInsights();
        var concern = "Customer satisfaction declining in weapons category";

        // Act
        insights.Concerns.Add(concern);

        // Assert
        insights.Concerns.Should().Contain(concern);
        insights.Concerns.Should().HaveCount(1);
    }

    [Fact]
    public void ShopInsights_CanAddNextSteps()
    {
        // Arrange
        var insights = new ShopInsights();
        var nextStep = "Implement loyalty program for returning customers";

        // Act
        insights.NextSteps.Add(nextStep);

        // Assert
        insights.NextSteps.Should().Contain(nextStep);
        insights.NextSteps.Should().HaveCount(1);
    }

    [Fact]
    public void ShopInsights_CanAddMultipleItemsToEachList()
    {
        // Arrange
        var insights = new ShopInsights();

        // Act
        insights.Recommendations.Add("Recommendation 1");
        insights.Recommendations.Add("Recommendation 2");
        insights.Achievements.Add("Achievement 1");
        insights.Achievements.Add("Achievement 2");
        insights.Concerns.Add("Concern 1");
        insights.NextSteps.Add("Next Step 1");

        // Assert
        insights.Recommendations.Should().HaveCount(2);
        insights.Achievements.Should().HaveCount(2);
        insights.Concerns.Should().HaveCount(1);
        insights.NextSteps.Should().HaveCount(1);
    }
}
