#nullable enable

using Xunit;
using Game.Items.Models;
using Game.Items.Models.Materials;
using System;
using System.Linq;

namespace Game.Items.Tests.Utils;

/// <summary>
/// Validates that enum types used throughout the game are properly defined
/// and can be safely used for UI population and JSON serialization.
/// </summary>
public class EnumValidationTests
{
    [Fact]
    public void Category_AllValuesAreUnique()
    {
        // Arrange & Act
        var values = Enum.GetValues<Category>().Select(v => v.ToString()).ToList();

        // Assert
        Assert.Equal(values.Count, values.Distinct().Count());
    }

    [Fact]
    public void Category_AllValuesCanBeParsed()
    {
        // Arrange
        var enumValues = Enum.GetValues<Category>();

        // Act & Assert
        foreach (var value in enumValues)
        {
            var stringValue = value.ToString();
            Assert.True(Enum.TryParse<Category>(stringValue, out var parsed));
            Assert.Equal(value, parsed);
        }
    }

    [Fact]
    public void Category_CaseInsensitiveParsingWorks()
    {
        // Arrange & Act & Assert
        Assert.True(Enum.TryParse<Category>("metal", true, out var lower));
        Assert.Equal(Category.Metal, lower);

        Assert.True(Enum.TryParse<Category>("METAL", true, out var upper));
        Assert.Equal(Category.Metal, upper);

        Assert.True(Enum.TryParse<Category>("Metal", true, out var proper));
        Assert.Equal(Category.Metal, proper);
    }

    [Fact]
    public void QualityTier_AllValuesAreUnique()
    {
        // Arrange & Act
        var values = Enum.GetValues<QualityTier>().Select(v => v.ToString()).ToList();

        // Assert
        Assert.Equal(values.Count, values.Distinct().Count());
    }

    [Fact]
    public void QualityTier_AllValuesCanBeParsed()
    {
        // Arrange
        var enumValues = Enum.GetValues<QualityTier>();

        // Act & Assert
        foreach (var value in enumValues)
        {
            var stringValue = value.ToString();
            Assert.True(Enum.TryParse<QualityTier>(stringValue, out var parsed));
            Assert.Equal(value, parsed);
        }
    }

    [Fact]
    public void ItemType_AllValuesAreUnique()
    {
        // Arrange & Act
        var values = Enum.GetValues<ItemType>().Select(v => v.ToString()).ToList();

        // Assert
        Assert.Equal(values.Count, values.Distinct().Count());
    }

    [Fact]
    public void ItemType_AllValuesCanBeParsed()
    {
        // Arrange
        var enumValues = Enum.GetValues<ItemType>();

        // Act & Assert
        foreach (var value in enumValues)
        {
            var stringValue = value.ToString();
            Assert.True(Enum.TryParse<ItemType>(stringValue, out var parsed));
            Assert.Equal(value, parsed);
        }
    }

    [Theory]
    [InlineData("Metal")]
    [InlineData("Wood")]
    [InlineData("Leather")]
    [InlineData("Cloth")]
    [InlineData("Gem")]
    [InlineData("Herb")]
    [InlineData("Bone")]
    [InlineData("Magical")]
    [InlineData("Essence")]
    [InlineData("Fuel")]
    public void Category_ExpectedValuesExist(string categoryName)
    {
        // Act
        var result = Enum.TryParse<Category>(categoryName, out var category);

        // Assert
        Assert.True(result, $"Expected category '{categoryName}' should exist in Category enum");
        Assert.Equal(categoryName, category.ToString());
    }

    [Theory]
    [InlineData("Common")]
    [InlineData("Uncommon")]
    [InlineData("Rare")]
    [InlineData("Epic")]
    [InlineData("Legendary")]
    public void QualityTier_ExpectedValuesExist(string tierName)
    {
        // Act
        var result = Enum.TryParse<QualityTier>(tierName, out var tier);

        // Assert
        Assert.True(result, $"Expected quality tier '{tierName}' should exist in QualityTier enum");
        Assert.Equal(tierName, tier.ToString());
    }

    [Theory]
    [InlineData("Weapon")]
    [InlineData("Armor")]
    [InlineData("Accessory")]
    [InlineData("Ring")]
    [InlineData("Material")]
    [InlineData("Consumable")]
    public void ItemType_ExpectedValuesExist(string typeName)
    {
        // Act
        var result = Enum.TryParse<ItemType>(typeName, out var itemType);

        // Assert
        Assert.True(result, $"Expected item type '{typeName}' should exist in ItemType enum");
        Assert.Equal(typeName, itemType.ToString());
    }

    [Theory]
    [InlineData("InvalidCategory")]
    [InlineData("Misc")]
    [InlineData("Fabric")]
    [InlineData("Stone")]
    public void Category_InvalidValuesShouldNotParse(string invalidValue)
    {
        // Act
        var result = Enum.TryParse<Category>(invalidValue, true, out _);

        // Assert
        Assert.False(result, $"Invalid value '{invalidValue}' should not parse as Category");
    }
}
