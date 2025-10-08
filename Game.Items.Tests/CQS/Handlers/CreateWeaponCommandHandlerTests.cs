using FluentAssertions;
using Game.Items.Commands;
using Game.Items.Handlers;
using Game.Items.Models;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for CreateWeaponCommandHandler to ensure proper weapon creation from configurations.
/// </summary>
public class CreateWeaponCommandHandlerTests
{
    private readonly CreateWeaponCommandHandler _handler;

    public CreateWeaponCommandHandlerTests()
    {
        _handler = new CreateWeaponCommandHandler();
    }

    [Fact]
    public async Task HandleAsync_WithValidIronSwordConfig_ShouldCreateWeapon()
    {
        // Arrange
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "iron_sword",
            Quality = QualityTier.Common
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Weapon>();
        result.Name.Should().Be("Iron Sword");
        result.Quality.Should().Be(QualityTier.Common);
        result.DamageBonus.Should().Be(5); // Common quality weapon damage bonus
    }

    [Fact]
    public async Task HandleAsync_WithValidSteelAxeConfig_ShouldCreateWeapon()
    {
        // Arrange
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "steel_axe",
            Quality = QualityTier.Rare
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Weapon>();
        result.Name.Should().Be("Steel Axe");
        result.Quality.Should().Be(QualityTier.Rare);
        result.DamageBonus.Should().Be(15); // Rare quality weapon damage bonus
    }

    [Fact]
    public async Task HandleAsync_WithValidMithrilDaggerConfig_ShouldCreateWeapon()
    {
        // Arrange
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "mithril_dagger",
            Quality = QualityTier.Legendary
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Weapon>();
        result.Name.Should().Be("Mithril Dagger");
        result.Quality.Should().Be(QualityTier.Legendary);
        result.DamageBonus.Should().Be(30); // Legendary quality weapon damage bonus
    }

    [Fact]
    public async Task HandleAsync_WithInvalidConfigId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "nonexistent_weapon",
            Quality = QualityTier.Common
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Weapon configuration 'nonexistent_weapon' not found.");
    }

    [Theory]
    [InlineData(QualityTier.Common, 5)]
    [InlineData(QualityTier.Uncommon, 10)]
    [InlineData(QualityTier.Rare, 15)]
    [InlineData(QualityTier.Epic, 20)]
    [InlineData(QualityTier.Legendary, 30)]
    public async Task HandleAsync_WithDifferentQualities_ShouldApplyCorrectDamageBonus(QualityTier quality, int expectedDamageBonus)
    {
        // Arrange
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "iron_sword",
            Quality = quality
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.DamageBonus.Should().Be(expectedDamageBonus);
        result.Quality.Should().Be(quality);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetCorrectItemId()
    {
        // Arrange
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "iron_sword",
            Quality = QualityTier.Uncommon
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ItemId.Should().Be("weapon_iron_sword_uncommon");
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyQualityToValue()
    {
        // Arrange
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "iron_sword",
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
        var command = new CreateWeaponCommand
        {
            WeaponConfigId = "steel_axe",
            Quality = QualityTier.Common
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Steel Axe");
    }
}
