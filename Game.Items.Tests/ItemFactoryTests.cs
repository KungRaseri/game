using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Tests;

public class ItemFactoryTests
{
    [Fact]
    public void CreateWeapon_CreatesWeaponWithCorrectProperties()
    {
        // Arrange
        var config = new WeaponConfig(
            ItemId: "test_weapon",
            Name: "Test Sword",
            Description: "A test sword",
            BaseValue: 100,
            BaseDamageBonus: 10,
            WeaponType: WeaponType.Sword
        );

        // Act
        var weapon = ItemFactory.CreateWeapon(config, QualityTier.Rare);

        // Assert
        Assert.Equal("test_weapon_rare", weapon.ItemId);
        Assert.Equal("Test Sword", weapon.Name);
        Assert.Equal("A test sword", weapon.Description);
        Assert.Equal(QualityTier.Rare, weapon.Quality);
        Assert.Equal(ItemType.Weapon, weapon.ItemType);
    }

    [Fact]
    public void CreateWeapon_AppliesQualityTierModifiers_ToDamageBonus()
    {
        // Arrange
        var config = ItemTypes.IronSword;

        // Act
        var commonWeapon = ItemFactory.CreateWeapon(config, QualityTier.Common);
        var legendaryWeapon = ItemFactory.CreateWeapon(config, QualityTier.Legendary);

        // Assert
        Assert.Equal(5, commonWeapon.DamageBonus);
        Assert.Equal(30, legendaryWeapon.DamageBonus);
    }

    [Fact]
    public void CreateWeapon_AppliesQualityTierModifiers_ToValue()
    {
        // Arrange
        var config = new WeaponConfig("test", "Test", "Test", 100, 10, WeaponType.Sword);

        // Act
        var commonWeapon = ItemFactory.CreateWeapon(config, QualityTier.Common);
        var uncommonWeapon = ItemFactory.CreateWeapon(config, QualityTier.Uncommon);
        var legendaryWeapon = ItemFactory.CreateWeapon(config, QualityTier.Legendary);

        // Assert
        Assert.Equal(100, commonWeapon.Value); // 100 * 1.0
        Assert.Equal(200, uncommonWeapon.Value); // 100 * 2.0
        Assert.Equal(1600, legendaryWeapon.Value); // 100 * 16.0
    }

    [Fact]
    public void CreateArmor_CreatesArmorWithCorrectProperties()
    {
        // Arrange
        var config = new ArmorConfig(
            ItemId: "test_armor",
            Name: "Test Armor",
            Description: "Test armor piece",
            BaseValue: 80,
            BaseDamageReduction: 5,
            ArmorType: ArmorType.Medium
        );

        // Act
        var armor = ItemFactory.CreateArmor(config, QualityTier.Epic);

        // Assert
        Assert.Equal("test_armor_epic", armor.ItemId);
        Assert.Equal("Test Armor", armor.Name);
        Assert.Equal("Test armor piece", armor.Description);
        Assert.Equal(QualityTier.Epic, armor.Quality);
        Assert.Equal(ItemType.Armor, armor.ItemType);
    }

    [Fact]
    public void CreateArmor_AppliesQualityTierModifiers_ToDamageReduction()
    {
        // Arrange
        var config = ItemTypes.LeatherArmor;

        // Act
        var commonArmor = ItemFactory.CreateArmor(config, QualityTier.Common);
        var legendaryArmor = ItemFactory.CreateArmor(config, QualityTier.Legendary);

        // Assert
        Assert.Equal(3, commonArmor.DamageReduction);
        Assert.Equal(18, legendaryArmor.DamageReduction);
    }

    [Fact]
    public void CreateArmor_AppliesQualityTierModifiers_ToValue()
    {
        // Arrange
        var config = new ArmorConfig("test", "Test", "Test", 50, 5, ArmorType.Light);

        // Act
        var commonArmor = ItemFactory.CreateArmor(config, QualityTier.Common);
        var rareArmor = ItemFactory.CreateArmor(config, QualityTier.Rare);

        // Assert
        Assert.Equal(50, commonArmor.Value); // 50 * 1.0
        Assert.Equal(200, rareArmor.Value); // 50 * 4.0
    }

    [Fact]
    public void CreateMaterial_CreatesMaterialWithCorrectProperties()
    {
        // Arrange
        var config = new MaterialConfig(
            ItemId: "test_material",
            Name: "Test Ore",
            Description: "Test material",
            BaseValue: 10,
            Category: Category.Metal,
            Stackable: true,
            MaxStackSize: 99
        );

        // Act
        var material = ItemFactory.CreateMaterial(config, QualityTier.Uncommon);

        // Assert
        Assert.Equal("test_material_uncommon", material.ItemId);
        Assert.Equal("Test Ore", material.Name);
        Assert.Equal("Test material", material.Description);
        Assert.Equal(QualityTier.Uncommon, material.Quality);
        Assert.Equal(ItemType.Material, material.ItemType);
        Assert.Equal(Category.Metal, material.Category);
        Assert.True(material.Stackable);
        Assert.Equal(99, material.MaxStackSize);
    }

    [Fact]
    public void CreateMaterial_AppliesQualityTierModifiers_ToValue()
    {
        // Arrange
        var config = new MaterialConfig("test", "Test", "Test", 20, Category.Wood);

        // Act
        var commonMaterial = ItemFactory.CreateMaterial(config, QualityTier.Common);
        var epicMaterial = ItemFactory.CreateMaterial(config, QualityTier.Epic);

        // Assert
        Assert.Equal(20, commonMaterial.Value); // 20 * 1.0
        Assert.Equal(160, epicMaterial.Value); // 20 * 8.0
    }

    [Theory]
    [InlineData(QualityTier.Common)]
    [InlineData(QualityTier.Uncommon)]
    [InlineData(QualityTier.Rare)]
    [InlineData(QualityTier.Epic)]
    [InlineData(QualityTier.Legendary)]
    public void CreateIronSword_CreatesWeaponWithSpecifiedQuality(QualityTier quality)
    {
        // Act
        var weapon = ItemFactory.CreateIronSword(quality);

        // Assert
        Assert.Equal(quality, weapon.Quality);
        Assert.Equal("Iron Sword", weapon.Name);
        Assert.Equal(ItemType.Weapon, weapon.ItemType);
    }

    [Fact]
    public void CreateIronSword_DefaultsToCommonQuality()
    {
        // Act
        var weapon = ItemFactory.CreateIronSword();

        // Assert
        Assert.Equal(QualityTier.Common, weapon.Quality);
    }

    [Theory]
    [InlineData(QualityTier.Common)]
    [InlineData(QualityTier.Uncommon)]
    [InlineData(QualityTier.Rare)]
    [InlineData(QualityTier.Epic)]
    [InlineData(QualityTier.Legendary)]
    public void CreateLeatherArmor_CreatesArmorWithSpecifiedQuality(QualityTier quality)
    {
        // Act
        var armor = ItemFactory.CreateLeatherArmor(quality);

        // Assert
        Assert.Equal(quality, armor.Quality);
        Assert.Equal("Leather Armor", armor.Name);
        Assert.Equal(ItemType.Armor, armor.ItemType);
    }

    [Fact]
    public void CreateLeatherArmor_DefaultsToCommonQuality()
    {
        // Act
        var armor = ItemFactory.CreateLeatherArmor();

        // Assert
        Assert.Equal(QualityTier.Common, armor.Quality);
    }

    [Theory]
    [InlineData(QualityTier.Common)]
    [InlineData(QualityTier.Uncommon)]
    [InlineData(QualityTier.Rare)]
    [InlineData(QualityTier.Epic)]
    [InlineData(QualityTier.Legendary)]
    public void CreateIronOre_CreatesMaterialWithSpecifiedQuality(QualityTier quality)
    {
        // Act
        var material = ItemFactory.CreateIronOre(quality);

        // Assert
        Assert.Equal(quality, material.Quality);
        Assert.Equal("Iron Ore", material.Name);
        Assert.Equal(ItemType.Material, material.ItemType);
        Assert.Equal(Category.Metal, material.Category);
    }

    [Fact]
    public void CreateIronOre_DefaultsToCommonQuality()
    {
        // Act
        var material = ItemFactory.CreateIronOre();

        // Assert
        Assert.Equal(QualityTier.Common, material.Quality);
    }

    [Fact]
    public void CreateSteelAxe_CreatesCorrectWeapon()
    {
        // Act
        var weapon = ItemFactory.CreateSteelAxe(QualityTier.Rare);

        // Assert
        Assert.Equal("Steel Axe", weapon.Name);
        Assert.Equal(QualityTier.Rare, weapon.Quality);
    }

    [Fact]
    public void CreateMithrilDagger_CreatesCorrectWeapon()
    {
        // Act
        var weapon = ItemFactory.CreateMithrilDagger(QualityTier.Epic);

        // Assert
        Assert.Equal("Mithril Dagger", weapon.Name);
        Assert.Equal(QualityTier.Epic, weapon.Quality);
    }

    [Fact]
    public void CreateChainMail_CreatesCorrectArmor()
    {
        // Act
        var armor = ItemFactory.CreateChainMail(QualityTier.Uncommon);

        // Assert
        Assert.Equal("Chain Mail", armor.Name);
        Assert.Equal(QualityTier.Uncommon, armor.Quality);
    }

    [Fact]
    public void CreatePlateArmor_CreatesCorrectArmor()
    {
        // Act
        var armor = ItemFactory.CreatePlateArmor(QualityTier.Legendary);

        // Assert
        Assert.Equal("Plate Armor", armor.Name);
        Assert.Equal(QualityTier.Legendary, armor.Quality);
    }

    [Fact]
    public void CreateSteelIngot_CreatesCorrectMaterial()
    {
        // Act
        var material = ItemFactory.CreateSteelIngot(QualityTier.Uncommon);

        // Assert
        Assert.Equal("Steel Ingot", material.Name);
        Assert.Equal(Category.Metal, material.Category);
        Assert.Equal(QualityTier.Uncommon, material.Quality);
    }

    [Fact]
    public void CreateMonsterHide_CreatesCorrectMaterial()
    {
        // Act
        var material = ItemFactory.CreateMonsterHide(QualityTier.Rare);

        // Assert
        Assert.Equal("Monster Hide", material.Name);
        Assert.Equal(Category.Leather, material.Category);
        Assert.Equal(QualityTier.Rare, material.Quality);
    }

    [Fact]
    public void CreateTannedLeather_CreatesCorrectMaterial()
    {
        // Act
        var material = ItemFactory.CreateTannedLeather();

        // Assert
        Assert.Equal("Tanned Leather", material.Name);
        Assert.Equal(Category.Leather, material.Category);
    }

    [Fact]
    public void CreateOakWood_CreatesCorrectMaterial()
    {
        // Act
        var material = ItemFactory.CreateOakWood();

        // Assert
        Assert.Equal("Oak Wood", material.Name);
        Assert.Equal(Category.Wood, material.Category);
    }

    [Fact]
    public void CreateRuby_CreatesCorrectMaterial()
    {
        // Act
        var material = ItemFactory.CreateRuby(QualityTier.Legendary);

        // Assert
        Assert.Equal("Ruby", material.Name);
        Assert.Equal(Category.Gem, material.Category);
        Assert.Equal(QualityTier.Legendary, material.Quality);
    }
}