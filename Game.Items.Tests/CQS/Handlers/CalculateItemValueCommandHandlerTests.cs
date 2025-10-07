using FluentAssertions;
using Game.Items.Commands;
using Game.Items.Handlers;
using Game.Items.Models;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for CalculateItemValueCommandHandler to ensure proper value calculations.
/// </summary>
public class CalculateItemValueCommandHandlerTests
{
    private readonly CalculateItemValueCommandHandler _handler;

    public CalculateItemValueCommandHandlerTests()
    {
        _handler = new CalculateItemValueCommandHandler();
    }

    [Theory]
    [InlineData(100, QualityTier.Common, 100)]     // 1.0x multiplier
    [InlineData(100, QualityTier.Uncommon, 200)]   // 2.0x multiplier
    [InlineData(100, QualityTier.Rare, 400)]       // 4.0x multiplier
    [InlineData(100, QualityTier.Epic, 800)]       // 8.0x multiplier
    [InlineData(100, QualityTier.Legendary, 1600)] // 16.0x multiplier
    public async Task HandleAsync_WithDifferentQualities_ShouldCalculateCorrectValues(int baseValue, QualityTier quality, int expectedValue)
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = baseValue,
            Quality = quality
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task HandleAsync_WithZeroBaseValue_ShouldReturnZero()
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = 0,
            Quality = QualityTier.Legendary
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithNegativeBaseValue_ShouldHandleGracefully()
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = -50,
            Quality = QualityTier.Common
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(-50); // Negative values should be preserved in calculation
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(1000)]
    [InlineData(9999)]
    public async Task HandleAsync_WithVariousBaseValues_ShouldScaleCorrectly(int baseValue)
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = baseValue,
            Quality = QualityTier.Rare // 4x multiplier
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(baseValue * 4);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnIntegerResult()
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = 333,
            Quality = QualityTier.Uncommon // 2x multiplier
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(666);
    }

    [Fact]
    public async Task HandleAsync_WithLargeValues_ShouldHandleOverflow()
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = int.MaxValue / 2,
            Quality = QualityTier.Uncommon // 2x multiplier
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        // In case of overflow, the result will be negative due to integer overflow
        // This is expected behavior for edge cases with extremely large values
        result.Should().Be(-2147483648); // int.MinValue due to overflow
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = 100,
            Quality = QualityTier.Epic
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().Be(800);
    }

    [Fact]
    public async Task HandleAsync_ShouldBeConsistent()
    {
        // Arrange
        var command = new CalculateItemValueCommand
        {
            BaseValue = 150,
            Quality = QualityTier.Rare
        };

        // Act
        var result1 = await _handler.HandleAsync(command);
        var result2 = await _handler.HandleAsync(command);

        // Assert
        result1.Should().Be(result2);
        result1.Should().Be(600); // 150 * 4
    }
}
