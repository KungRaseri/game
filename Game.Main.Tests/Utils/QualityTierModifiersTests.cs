using Xunit;
using Game.Main.Models;
using Game.Main.Utils;

namespace Game.Main.Tests.Utils;

public class QualityTierModifiersTests
{
    [Theory]
    [InlineData(QualityTier.Common, 5)]
    [InlineData(QualityTier.Uncommon, 10)]
    [InlineData(QualityTier.Rare, 15)]
    [InlineData(QualityTier.Epic, 20)]
    [InlineData(QualityTier.Legendary, 30)]
    public void GetWeaponDamageBonus_ReturnsCorrectValue_ForQualityTier(QualityTier quality, int expectedBonus)
    {
        // Act
        var bonus = QualityTierModifiers.GetWeaponDamageBonus(quality);

        // Assert
        Assert.Equal(expectedBonus, bonus);
    }

    [Theory]
    [InlineData(QualityTier.Common, 3)]
    [InlineData(QualityTier.Uncommon, 6)]
    [InlineData(QualityTier.Rare, 9)]
    [InlineData(QualityTier.Epic, 12)]
    [InlineData(QualityTier.Legendary, 18)]
    public void GetArmorDamageReduction_ReturnsCorrectValue_ForQualityTier(QualityTier quality, int expectedReduction)
    {
        // Act
        var reduction = QualityTierModifiers.GetArmorDamageReduction(quality);

        // Assert
        Assert.Equal(expectedReduction, reduction);
    }

    [Theory]
    [InlineData(QualityTier.Common, 1.0f)]
    [InlineData(QualityTier.Uncommon, 2.0f)]
    [InlineData(QualityTier.Rare, 4.0f)]
    [InlineData(QualityTier.Epic, 8.0f)]
    [InlineData(QualityTier.Legendary, 16.0f)]
    public void GetValueMultiplier_ReturnsCorrectValue_ForQualityTier(QualityTier quality, float expectedMultiplier)
    {
        // Act
        var multiplier = QualityTierModifiers.GetValueMultiplier(quality);

        // Assert
        Assert.Equal(expectedMultiplier, multiplier);
    }

    [Theory]
    [InlineData(100, QualityTier.Common, 100)]
    [InlineData(100, QualityTier.Uncommon, 200)]
    [InlineData(100, QualityTier.Rare, 400)]
    [InlineData(100, QualityTier.Epic, 800)]
    [InlineData(100, QualityTier.Legendary, 1600)]
    [InlineData(50, QualityTier.Common, 50)]
    [InlineData(50, QualityTier.Legendary, 800)]
    [InlineData(0, QualityTier.Legendary, 0)]
    public void CalculateItemValue_ReturnsCorrectValue(int baseValue, QualityTier quality, int expectedValue)
    {
        // Act
        var value = QualityTierModifiers.CalculateItemValue(baseValue, quality);

        // Assert
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void CalculateItemValue_RoundsToNearestInteger()
    {
        // Arrange - use a base value that will result in proper rounding
        int testValue = 15;

        // Act
        var uncommonValue = QualityTierModifiers.CalculateItemValue(testValue, QualityTier.Uncommon);

        // Assert - 15 * 2.0 = 30.0, rounds to 30
        Assert.Equal(30, uncommonValue);
    }

    [Fact]
    public void GetWeaponDamageBonus_ReturnsDefaultValue_ForInvalidQualityTier()
    {
        // Arrange - cast an invalid int to QualityTier
        var invalidQuality = (QualityTier)999;

        // Act
        var bonus = QualityTierModifiers.GetWeaponDamageBonus(invalidQuality);

        // Assert
        Assert.Equal(5, bonus); // Should default to Common value
    }

    [Fact]
    public void GetArmorDamageReduction_ReturnsDefaultValue_ForInvalidQualityTier()
    {
        // Arrange - cast an invalid int to QualityTier
        var invalidQuality = (QualityTier)999;

        // Act
        var reduction = QualityTierModifiers.GetArmorDamageReduction(invalidQuality);

        // Assert
        Assert.Equal(3, reduction); // Should default to Common value
    }

    [Fact]
    public void GetValueMultiplier_ReturnsDefaultValue_ForInvalidQualityTier()
    {
        // Arrange - cast an invalid int to QualityTier
        var invalidQuality = (QualityTier)999;

        // Act
        var multiplier = QualityTierModifiers.GetValueMultiplier(invalidQuality);

        // Assert
        Assert.Equal(1.0f, multiplier); // Should default to Common value
    }
}
