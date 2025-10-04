using Game.Items.Models;

namespace Game.Items.Tests;

public class ArmorTests
{
    [Fact]
    public void Constructor_SetsValues_Correctly()
    {
        // Arrange & Act
        var armor = new Armor(
            itemId: "armor_001",
            name: "Plate Mail",
            description: "Heavy plate armor",
            quality: QualityTier.Epic,
            value: 200,
            damageReduction: 12
        );

        // Assert
        Assert.Equal("armor_001", armor.ItemId);
        Assert.Equal("Plate Mail", armor.Name);
        Assert.Equal("Heavy plate armor", armor.Description);
        Assert.Equal(ItemType.Armor, armor.ItemType);
        Assert.Equal(QualityTier.Epic, armor.Quality);
        Assert.Equal(200, armor.Value);
        Assert.Equal(12, armor.DamageReduction);
    }

    [Fact]
    public void Constructor_ClampsNegativeDamageReduction_ToZero()
    {
        // Arrange & Act
        var armor = new Armor(
            itemId: "armor_001",
            name: "Test Armor",
            description: "Test",
            quality: QualityTier.Common,
            value: 50,
            damageReduction: -5
        );

        // Assert
        Assert.Equal(0, armor.DamageReduction);
    }

    [Fact]
    public void DamageReduction_Setter_ClampsNegativeValue_ToZero()
    {
        // Arrange
        var armor = new Armor("armor_001", "Test", "Test", QualityTier.Common, 50, 8);

        // Act
        armor.DamageReduction = -3;

        // Assert
        Assert.Equal(0, armor.DamageReduction);
    }

    [Fact]
    public void DamageReduction_Setter_AcceptsZero()
    {
        // Arrange
        var armor = new Armor("armor_001", "Test", "Test", QualityTier.Common, 50, 8);

        // Act
        armor.DamageReduction = 0;

        // Assert
        Assert.Equal(0, armor.DamageReduction);
    }

    [Fact]
    public void DamageReduction_Setter_AcceptsPositiveValue()
    {
        // Arrange
        var armor = new Armor("armor_001", "Test", "Test", QualityTier.Common, 50, 8);

        // Act
        armor.DamageReduction = 15;

        // Assert
        Assert.Equal(15, armor.DamageReduction);
    }

    [Fact]
    public void ToString_ReturnsFormattedString_WithDamageReduction()
    {
        // Arrange
        var armor = new Armor("armor_001", "Chain Mail", "Test", QualityTier.Uncommon, 80, 6);

        // Act
        var result = armor.ToString();

        // Assert
        Assert.Equal("Chain Mail (Uncommon Armor) - -6 Damage Taken - 80g", result);
    }

    [Fact]
    public void Armor_InheritsFrom_Equipment()
    {
        // Arrange
        var armor = new Armor("armor_001", "Test", "Test", QualityTier.Common, 50, 5);

        // Act & Assert
        Assert.IsAssignableFrom<Item>(armor);
    }
}