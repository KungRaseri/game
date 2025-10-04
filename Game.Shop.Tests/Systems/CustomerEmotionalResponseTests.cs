#nullable enable

using FluentAssertions;
using Game.Shop.Systems;

namespace Game.Shop.Tests.Systems;

/// <summary>
/// Tests for customer emotional response enumeration.
/// </summary>
public class CustomerEmotionalResponseTests
{
    [Fact]
    public void CustomerEmotionalResponse_HasExpectedValues()
    {
        // Act & Assert - Check all enum values exist
        var responses = Enum.GetValues<CustomerEmotionalResponse>();

        responses.Should().Contain(CustomerEmotionalResponse.Upset);
        responses.Should().Contain(CustomerEmotionalResponse.Frustrated);
        responses.Should().Contain(CustomerEmotionalResponse.Neutral);
        responses.Should().Contain(CustomerEmotionalResponse.Satisfied);
        responses.Should().Contain(CustomerEmotionalResponse.Delighted);
        responses.Should().Contain(CustomerEmotionalResponse.Conflicted);
    }

    [Fact]
    public void CustomerEmotionalResponse_HasCorrectUnderlyingValues()
    {
        // Act & Assert - Verify enum values for consistency
        Assert.Equal(0, (int)CustomerEmotionalResponse.Upset);
        Assert.Equal(1, (int)CustomerEmotionalResponse.Frustrated);
        Assert.Equal(2, (int)CustomerEmotionalResponse.Neutral);
        Assert.Equal(3, (int)CustomerEmotionalResponse.Satisfied);
        Assert.Equal(4, (int)CustomerEmotionalResponse.Delighted);
        Assert.Equal(5, (int)CustomerEmotionalResponse.Conflicted);
    }

    [Theory]
    [InlineData(CustomerEmotionalResponse.Upset, "Upset")]
    [InlineData(CustomerEmotionalResponse.Frustrated, "Frustrated")]
    [InlineData(CustomerEmotionalResponse.Neutral, "Neutral")]
    [InlineData(CustomerEmotionalResponse.Satisfied, "Satisfied")]
    [InlineData(CustomerEmotionalResponse.Delighted, "Delighted")]
    [InlineData(CustomerEmotionalResponse.Conflicted, "Conflicted")]
    public void CustomerEmotionalResponse_ToString_ReturnsCorrectString(CustomerEmotionalResponse response,
        string expectedString)
    {
        // Act
        var result = response.ToString();

        // Assert
        result.Should().Be(expectedString);
    }

    [Fact]
    public void CustomerEmotionalResponse_AllValuesAreDefined()
    {
        // Arrange
        var expectedCount = 6;

        // Act
        var actualCount = Enum.GetValues<CustomerEmotionalResponse>().Length;

        // Assert
        actualCount.Should().Be(expectedCount);
    }

    [Fact]
    public void CustomerEmotionalResponse_CanBeCompared()
    {
        // Act & Assert - Verify ordering makes sense (negative to positive)
        Assert.True(CustomerEmotionalResponse.Upset < CustomerEmotionalResponse.Frustrated);
        Assert.True(CustomerEmotionalResponse.Frustrated < CustomerEmotionalResponse.Neutral);
        Assert.True(CustomerEmotionalResponse.Neutral < CustomerEmotionalResponse.Satisfied);
        Assert.True(CustomerEmotionalResponse.Satisfied < CustomerEmotionalResponse.Delighted);

        // Conflicted is separate from the satisfaction spectrum
        Assert.True(CustomerEmotionalResponse.Conflicted > CustomerEmotionalResponse.Delighted);
    }

    [Fact]
    public void CustomerEmotionalResponse_CanBeParsedFromString()
    {
        // Act & Assert
        var upset = Enum.Parse<CustomerEmotionalResponse>("Upset");
        upset.Should().Be(CustomerEmotionalResponse.Upset);

        var delighted = Enum.Parse<CustomerEmotionalResponse>("Delighted");
        delighted.Should().Be(CustomerEmotionalResponse.Delighted);
    }

    [Fact]
    public void CustomerEmotionalResponse_InvalidStringThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Enum.Parse<CustomerEmotionalResponse>("InvalidResponse"));
    }

    [Theory]
    [InlineData(CustomerEmotionalResponse.Upset, true)]
    [InlineData(CustomerEmotionalResponse.Frustrated, true)]
    [InlineData(CustomerEmotionalResponse.Neutral, false)]
    [InlineData(CustomerEmotionalResponse.Satisfied, false)]
    [InlineData(CustomerEmotionalResponse.Delighted, false)]
    [InlineData(CustomerEmotionalResponse.Conflicted, false)]
    public void CustomerEmotionalResponse_IsNegative_ReturnsCorrectValue(CustomerEmotionalResponse response,
        bool expectedIsNegative)
    {
        // Act
        var isNegative = response == CustomerEmotionalResponse.Upset ||
                         response == CustomerEmotionalResponse.Frustrated;

        // Assert
        isNegative.Should().Be(expectedIsNegative);
    }

    [Theory]
    [InlineData(CustomerEmotionalResponse.Satisfied, true)]
    [InlineData(CustomerEmotionalResponse.Delighted, true)]
    [InlineData(CustomerEmotionalResponse.Neutral, false)]
    [InlineData(CustomerEmotionalResponse.Upset, false)]
    [InlineData(CustomerEmotionalResponse.Frustrated, false)]
    [InlineData(CustomerEmotionalResponse.Conflicted, false)]
    public void CustomerEmotionalResponse_IsPositive_ReturnsCorrectValue(CustomerEmotionalResponse response,
        bool expectedIsPositive)
    {
        // Act
        var isPositive = response == CustomerEmotionalResponse.Satisfied ||
                         response == CustomerEmotionalResponse.Delighted;

        // Assert
        isPositive.Should().Be(expectedIsPositive);
    }
}