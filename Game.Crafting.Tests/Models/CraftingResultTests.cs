using FluentAssertions;
using Game.Items.Models;
using Game.Crafting.Models;

namespace Game.Crafting.Tests.Models;

/// <summary>
/// Tests for the CraftingResult class.
/// </summary>
public class CraftingResultTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        // Assert
        result.ItemId.Should().Be("iron_sword");
        result.ItemName.Should().Be("Iron Sword");
        result.ItemType.Should().Be(ItemType.Weapon);
        result.BaseQuality.Should().Be(QualityTier.Common);
        result.Quantity.Should().Be(1);
        result.BaseValue.Should().Be(50);
        result.ItemProperties.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithItemProperties_SetsProperties()
    {
        // Arrange
        var properties = new Dictionary<string, object>
        {
            ["DamageBonus"] = 10,
            ["Durability"] = 100
        };

        // Act
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50,
            properties);

        // Assert
        result.ItemProperties.Should().HaveCount(2);
        result.ItemProperties["DamageBonus"].Should().Be(10);
        result.ItemProperties["Durability"].Should().Be(100);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidItemId_ThrowsArgumentException(string itemId)
    {
        // Arrange & Act & Assert
        var action = () => new CraftingResult(
            itemId,
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Item ID cannot be null or empty*");
    }

    [Fact]
    public void Constructor_WithNullItemId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new CraftingResult(
            null!,
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Item ID cannot be null or empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidItemName_ThrowsArgumentException(string itemName)
    {
        // Arrange & Act & Assert
        var action = () => new CraftingResult(
            "iron_sword",
            itemName,
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Item name cannot be null or empty*");
    }

    [Fact]
    public void Constructor_WithNullItemName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new CraftingResult(
            "iron_sword",
            null!,
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Item name cannot be null or empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_WithInvalidQuantity_ThrowsArgumentException(int quantity)
    {
        // Arrange & Act & Assert
        var action = () => new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            quantity,
            50);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*");
    }

    [Fact]
    public void Constructor_WithNegativeBaseValue_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            -10);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Base value cannot be negative*");
    }

    [Fact]
    public void GetProperty_WithExistingProperty_ReturnsValue()
    {
        // Arrange
        var properties = new Dictionary<string, object>
        {
            ["DamageBonus"] = 15
        };
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50,
            properties);

        // Act
        var damageBonus = result.GetProperty<int>("DamageBonus");

        // Assert
        damageBonus.Should().Be(15);
    }

    [Fact]
    public void GetProperty_WithNonExistentProperty_ReturnsDefault()
    {
        // Arrange
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        // Act
        var damageBonus = result.GetProperty<int>("DamageBonus");

        // Assert
        damageBonus.Should().Be(0);
    }

    [Fact]
    public void GetProperty_WithCustomDefault_ReturnsCustomDefault()
    {
        // Arrange
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        // Act
        var damageBonus = result.GetProperty("DamageBonus", 5);

        // Assert
        damageBonus.Should().Be(5);
    }

    [Fact]
    public void GetProperty_WithWrongType_ReturnsDefault()
    {
        // Arrange
        var properties = new Dictionary<string, object>
        {
            ["DamageBonus"] = "not_an_int"
        };
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50,
            properties);

        // Act
        var damageBonus = result.GetProperty<int>("DamageBonus");

        // Assert
        damageBonus.Should().Be(0);
    }

    [Fact]
    public void SetProperty_AddsOrUpdatesProperty()
    {
        // Arrange
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        // Act
        result.SetProperty("DamageBonus", 20);
        result.SetProperty("Durability", 150);

        // Assert
        result.GetProperty<int>("DamageBonus").Should().Be(20);
        result.GetProperty<int>("Durability").Should().Be(150);
    }

    [Fact]
    public void SetProperty_UpdatesExistingProperty()
    {
        // Arrange
        var properties = new Dictionary<string, object>
        {
            ["DamageBonus"] = 10
        };
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50,
            properties);

        // Act
        result.SetProperty("DamageBonus", 25);

        // Assert
        result.GetProperty<int>("DamageBonus").Should().Be(25);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        // Act
        var stringResult = result.ToString();

        // Assert
        stringResult.Should().Be("1x Iron Sword (Common Weapon) - 50g");
    }

    [Fact]
    public void Equals_WithSameProperties_ReturnsTrue()
    {
        // Arrange
        var result1 = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        var result2 = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        // Act & Assert
        result1.Equals(result2).Should().BeTrue();
        result1.GetHashCode().Should().Be(result2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentProperties_ReturnsFalse()
    {
        // Arrange
        var result1 = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        var result2 = new CraftingResult(
            "steel_sword",
            "Steel Sword",
            ItemType.Weapon,
            QualityTier.Uncommon,
            1,
            100);

        // Act & Assert
        result1.Equals(result2).Should().BeFalse();
    }
}
