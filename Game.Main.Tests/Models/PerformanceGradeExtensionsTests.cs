#nullable enable

using FluentAssertions;
using Game.Main.Models;
using Xunit;

namespace Game.Main.Tests.Models;

public class PerformanceGradeExtensionsTests
{
    [Theory]
    [InlineData(PerformanceGrade.Poor, "Needs Immediate Attention")]
    [InlineData(PerformanceGrade.BelowAverage, "Room for Improvement")]
    [InlineData(PerformanceGrade.Average, "Meeting Expectations")]
    [InlineData(PerformanceGrade.Good, "Performing Well")]
    [InlineData(PerformanceGrade.VeryGood, "Strong Performance")]
    [InlineData(PerformanceGrade.Excellent, "Outstanding Results")]
    public void GetDescription_ReturnsCorrectDescription(PerformanceGrade grade, string expectedDescription)
    {
        // Act
        var description = grade.GetDescription();

        // Assert
        description.Should().Be(expectedDescription);
    }

    [Fact]
    public void GetDescription_WithInvalidValue_ReturnsUnknown()
    {
        // Arrange
        var invalidGrade = (PerformanceGrade)999;

        // Act
        var description = invalidGrade.GetDescription();

        // Assert
        description.Should().Be("Unknown");
    }

    [Fact]
    public void GetColor_Poor_ReturnsRed()
    {
        // Act
        var color = PerformanceGrade.Poor.GetColor();

        // Assert
        color.Should().Be(Godot.Colors.Red);
    }

    [Fact]
    public void GetColor_BelowAverage_ReturnsOrange()
    {
        // Act
        var color = PerformanceGrade.BelowAverage.GetColor();

        // Assert
        color.Should().Be(Godot.Colors.Orange);
    }

    [Fact]
    public void GetColor_Average_ReturnsYellow()
    {
        // Act
        var color = PerformanceGrade.Average.GetColor();

        // Assert
        color.Should().Be(Godot.Colors.Yellow);
    }

    [Fact]
    public void GetColor_Good_ReturnsLightGreen()
    {
        // Act
        var color = PerformanceGrade.Good.GetColor();

        // Assert
        color.Should().Be(Godot.Colors.LightGreen);
    }

    [Fact]
    public void GetColor_VeryGood_ReturnsGreen()
    {
        // Act
        var color = PerformanceGrade.VeryGood.GetColor();

        // Assert
        color.Should().Be(Godot.Colors.Green);
    }

    [Fact]
    public void GetColor_Excellent_ReturnsGold()
    {
        // Act
        var color = PerformanceGrade.Excellent.GetColor();

        // Assert
        color.Should().Be(Godot.Colors.Gold);
    }

    [Fact]
    public void GetColor_WithInvalidValue_ReturnsGray()
    {
        // Arrange
        var invalidGrade = (PerformanceGrade)999;

        // Act
        var color = invalidGrade.GetColor();

        // Assert
        color.Should().Be(Godot.Colors.Gray);
    }
}
