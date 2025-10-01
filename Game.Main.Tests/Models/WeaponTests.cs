using System;
using Xunit;
using Game.Main.Models;

namespace Game.Main.Tests.Models;

public class WeaponTests
{
    [Fact]
    public void Constructor_SetsValues_Correctly()
    {
        // Arrange & Act
        var weapon = new Weapon(
            itemId: "weapon_001",
            name: "Iron Sword",
            description: "A sharp iron blade",
            quality: QualityTier.Uncommon,
            value: 100,
            damageBonus: 15
        );

        // Assert
        Assert.Equal("weapon_001", weapon.ItemId);
        Assert.Equal("Iron Sword", weapon.Name);
        Assert.Equal("A sharp iron blade", weapon.Description);
        Assert.Equal(ItemType.Weapon, weapon.ItemType);
        Assert.Equal(QualityTier.Uncommon, weapon.Quality);
        Assert.Equal(100, weapon.Value);
        Assert.Equal(15, weapon.DamageBonus);
        Assert.Equal(EquipmentSlot.Weapon, weapon.EquipmentSlot);
    }

    [Fact]
    public void Constructor_ClampsNegativeDamageBonus_ToZero()
    {
        // Arrange & Act
        var weapon = new Weapon(
            itemId: "weapon_001",
            name: "Test Weapon",
            description: "Test",
            quality: QualityTier.Common,
            value: 50,
            damageBonus: -10
        );

        // Assert
        Assert.Equal(0, weapon.DamageBonus);
    }

    [Fact]
    public void DamageBonus_Setter_ClampsNegativeValue_ToZero()
    {
        // Arrange
        var weapon = new Weapon("weapon_001", "Test", "Test", QualityTier.Common, 50, 10);

        // Act
        weapon.DamageBonus = -5;

        // Assert
        Assert.Equal(0, weapon.DamageBonus);
    }

    [Fact]
    public void DamageBonus_Setter_AcceptsZero()
    {
        // Arrange
        var weapon = new Weapon("weapon_001", "Test", "Test", QualityTier.Common, 50, 10);

        // Act
        weapon.DamageBonus = 0;

        // Assert
        Assert.Equal(0, weapon.DamageBonus);
    }

    [Fact]
    public void DamageBonus_Setter_AcceptsPositiveValue()
    {
        // Arrange
        var weapon = new Weapon("weapon_001", "Test", "Test", QualityTier.Common, 50, 10);

        // Act
        weapon.DamageBonus = 25;

        // Assert
        Assert.Equal(25, weapon.DamageBonus);
    }

    [Fact]
    public void ToString_ReturnsFormattedString_WithDamageBonus()
    {
        // Arrange
        var weapon = new Weapon("weapon_001", "Steel Axe", "Test", QualityTier.Rare, 150, 20);

        // Act
        var result = weapon.ToString();

        // Assert
        Assert.Equal("Steel Axe (Rare Weapon) - +20 Damage - 150g", result);
    }

    [Fact]
    public void Weapon_InheritsFrom_Equipment()
    {
        // Arrange
        var weapon = new Weapon("weapon_001", "Test", "Test", QualityTier.Common, 50, 10);

        // Act & Assert
        Assert.IsAssignableFrom<Equipment>(weapon);
        Assert.IsAssignableFrom<Item>(weapon);
    }
}
