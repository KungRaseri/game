using FluentAssertions;
using Game.Main.Models;

namespace Game.Main.Tests.Models;

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