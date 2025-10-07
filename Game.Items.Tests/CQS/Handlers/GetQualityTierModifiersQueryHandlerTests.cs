using FluentAssertions;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Queries;

namespace Game.Items.Tests.CQS.Handlers;

/// <summary>
/// Tests for GetQualityTierModifiersQueryHandler to ensure proper quality tier modifier retrieval.
/// </summary>
public class GetQualityTierModifiersQueryHandlerTests
{
    private readonly GetQualityTierModifiersQueryHandler _handler;

    public GetQualityTierModifiersQueryHandlerTests()
    {
        _handler = new GetQualityTierModifiersQueryHandler();
    }

    [Theory]
    [InlineData(QualityTier.Common, 5, 3, 1.0f)]
    [InlineData(QualityTier.Uncommon, 10, 6, 2.0f)]
    [InlineData(QualityTier.Rare, 15, 9, 4.0f)]
    [InlineData(QualityTier.Epic, 20, 12, 8.0f)]
    [InlineData(QualityTier.Legendary, 30, 18, 16.0f)]
    public async Task HandleAsync_WithDifferentQualities_ShouldReturnCorrectModifiers(
        QualityTier quality, 
        int expectedWeaponBonus, 
        int expectedArmorReduction, 
        float expectedValueMultiplier)
    {
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = quality
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(quality);
        result.WeaponDamageBonus.Should().Be(expectedWeaponBonus);
        result.ArmorDamageReduction.Should().Be(expectedArmorReduction);
        result.ValueMultiplier.Should().Be(expectedValueMultiplier);
    }

    [Fact]
    public async Task HandleAsync_WithCommonQuality_ShouldReturnBaseModifiers()
    {
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Common,
            ModifierType = "all"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(QualityTier.Common);
        result.WeaponDamageBonus.Should().Be(5);
        result.ArmorDamageReduction.Should().Be(3);
        result.ValueMultiplier.Should().Be(1.0f);
        result.CalculatedValue.Should().Be(0); // No base value provided
    }

    [Fact]
    public async Task HandleAsync_WithLegendaryQuality_ShouldReturnMaximumModifiers()
    {
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Legendary,
            ModifierType = "weapon"
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(QualityTier.Legendary);
        result.WeaponDamageBonus.Should().Be(30);
        result.ArmorDamageReduction.Should().Be(18);
        result.ValueMultiplier.Should().Be(16.0f);
    }

    [Theory]
    [InlineData("weapon")]
    [InlineData("armor")]
    [InlineData("value")]
    [InlineData("all")]
    public async Task HandleAsync_WithDifferentModifierTypes_ShouldReturnSameResult(string modifierType)
    {
        // Note: ModifierType is currently not implemented in the handler, so all should return the same
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Rare,
            ModifierType = modifierType
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(QualityTier.Rare);
        result.WeaponDamageBonus.Should().Be(15);
        result.ArmorDamageReduction.Should().Be(9);
        result.ValueMultiplier.Should().Be(4.0f);
    }

    [Fact]
    public async Task HandleAsync_WithDefaultModifierType_ShouldReturnAllModifiers()
    {
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Epic
        }; // ModifierType defaults to "all"

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(QualityTier.Epic);
        result.WeaponDamageBonus.Should().Be(20);
        result.ArmorDamageReduction.Should().Be(12);
        result.ValueMultiplier.Should().Be(8.0f);
        result.CalculatedValue.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_ShouldAlwaysReturnZeroCalculatedValue()
    {
        // Since no base value is provided in the query, CalculatedValue should always be 0
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Legendary
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.CalculatedValue.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Uncommon
        };
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.HandleAsync(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(QualityTier.Uncommon);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConsistentResults()
    {
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Rare,
            ModifierType = "all"
        };

        // Act
        var result1 = await _handler.HandleAsync(query);
        var result2 = await _handler.HandleAsync(query);

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidRanges()
    {
        // Arrange
        var query = new GetQualityTierModifiersQuery
        {
            Quality = QualityTier.Rare
        };

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.WeaponDamageBonus.Should().BeGreaterThan(0);
        result.ArmorDamageReduction.Should().BeGreaterThan(0);
        result.ValueMultiplier.Should().BeGreaterThan(0f);
    }

    [Fact]
    public async Task HandleAsync_ShouldScaleCorrectlyAcrossQualities()
    {
        // Arrange & Act
        var commonResult = await _handler.HandleAsync(new GetQualityTierModifiersQuery { Quality = QualityTier.Common });
        var uncommonResult = await _handler.HandleAsync(new GetQualityTierModifiersQuery { Quality = QualityTier.Uncommon });
        var rareResult = await _handler.HandleAsync(new GetQualityTierModifiersQuery { Quality = QualityTier.Rare });

        // Assert
        // Weapon damage should increase with quality
        uncommonResult.WeaponDamageBonus.Should().BeGreaterThan(commonResult.WeaponDamageBonus);
        rareResult.WeaponDamageBonus.Should().BeGreaterThan(uncommonResult.WeaponDamageBonus);

        // Armor reduction should increase with quality
        uncommonResult.ArmorDamageReduction.Should().BeGreaterThan(commonResult.ArmorDamageReduction);
        rareResult.ArmorDamageReduction.Should().BeGreaterThan(uncommonResult.ArmorDamageReduction);

        // Value multiplier should increase with quality
        uncommonResult.ValueMultiplier.Should().BeGreaterThan(commonResult.ValueMultiplier);
        rareResult.ValueMultiplier.Should().BeGreaterThan(uncommonResult.ValueMultiplier);
    }
}
