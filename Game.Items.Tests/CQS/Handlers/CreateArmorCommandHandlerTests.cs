using FluentAssertions;
using Game.Items.Commands;
using Game.Items.Handlers;
using Game.Items.Models;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for CreateArmorCommandHandler to ensure proper armor creation from configurations.
/// </summary>
public class CreateArmorCommandHandlerTests
{
    private readonly CreateArmorCommandHandler _handler;

    public CreateArmorCommandHandlerTests()
    {
        _handler = new CreateArmorCommandHandler();
    }

    [Fact]
    public async Task HandleAsync_WithValidLeatherArmorConfig_ShouldCreateArmor()
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "leather_armor",
            Quality = QualityTier.Common
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Armor>();
        result.Name.Should().Be("Leather Armor");
        result.Quality.Should().Be(QualityTier.Common);
        result.DamageReduction.Should().Be(3); // Common quality armor damage reduction
    }

    [Fact]
    public async Task HandleAsync_WithValidChainMailConfig_ShouldCreateArmor()
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "chain_mail",
            Quality = QualityTier.Rare
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Armor>();
        result.Name.Should().Be("Chain Mail");
        result.Quality.Should().Be(QualityTier.Rare);
        result.DamageReduction.Should().Be(9); // Rare quality armor damage reduction
    }

    [Fact]
    public async Task HandleAsync_WithValidPlateArmorConfig_ShouldCreateArmor()
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "plate_armor",
            Quality = QualityTier.Legendary
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Armor>();
        result.Name.Should().Be("Plate Armor");
        result.Quality.Should().Be(QualityTier.Legendary);
        result.DamageReduction.Should().Be(18); // Legendary quality armor damage reduction
    }

    [Fact]
    public async Task HandleAsync_WithInvalidConfigId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "nonexistent_armor",
            Quality = QualityTier.Common
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Armor configuration 'nonexistent_armor' not found.");
    }

    [Theory]
    [InlineData(QualityTier.Common, 3)]
    [InlineData(QualityTier.Uncommon, 6)]
    [InlineData(QualityTier.Rare, 9)]
    [InlineData(QualityTier.Epic, 12)]
    [InlineData(QualityTier.Legendary, 18)]
    public async Task HandleAsync_WithDifferentQualities_ShouldApplyCorrectDamageReduction(QualityTier quality, int expectedDamageReduction)
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "leather_armor",
            Quality = quality
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.DamageReduction.Should().Be(expectedDamageReduction);
        result.Quality.Should().Be(quality);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetCorrectItemId()
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "chain_mail",
            Quality = QualityTier.Uncommon
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ItemId.Should().Be("armor_chainmail_uncommon");
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyQualityToValue()
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "plate_armor",
            Quality = QualityTier.Epic
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        // Epic quality should multiply value by 8
        result.Value.Should().BeGreaterThan(result.BaseValue);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var command = new CreateArmorCommand
        {
            ArmorConfigId = "leather_armor",
            Quality = QualityTier.Common
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Leather Armor");
    }
}
