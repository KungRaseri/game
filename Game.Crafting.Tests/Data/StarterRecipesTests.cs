using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Game.Crafting.Data;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Crafting.Tests.Data;

/// <summary>
/// Tests for the StarterRecipes static data provider.
/// Ensures all starter and advanced recipes are properly configured.
/// </summary>
public class StarterRecipesTests
{
    #region GetStarterRecipes Tests

    [Fact]
    public void GetStarterRecipes_ReturnsExpectedCount()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();

        // Assert
        recipes.Should().HaveCount(3);
    }

    [Fact]
    public void GetStarterRecipes_AllRecipesAreUnlocked()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();

        // Assert
        recipes.Should().AllSatisfy(recipe => recipe.IsUnlocked.Should().BeTrue());
    }

    [Fact]
    public void GetStarterRecipes_AllRecipesHaveNoPrerequisites()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();

        // Assert
        recipes.Should().AllSatisfy(recipe => recipe.Prerequisites.Should().BeEmpty());
    }

    [Fact]
    public void GetStarterRecipes_ContainsExpectedRecipeIds()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();

        // Assert
        var recipeIds = recipes.Select(r => r.RecipeId).ToList();
        recipeIds.Should().Contain("recipe_iron_sword");
        recipeIds.Should().Contain("recipe_wooden_shield");
        recipeIds.Should().Contain("recipe_health_potion");
    }

    [Fact]
    public void GetStarterRecipes_AllRecipesHaveValidProperties()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();

        // Assert
        recipes.Should().AllSatisfy(recipe =>
        {
            recipe.RecipeId.Should().NotBeNullOrEmpty();
            recipe.Name.Should().NotBeNullOrEmpty();
            recipe.Description.Should().NotBeNullOrEmpty();
            recipe.MaterialRequirements.Should().NotBeEmpty();
            recipe.Result.Should().NotBeNull();
            recipe.CraftingTime.Should().BeGreaterThan(0);
            recipe.ExperienceReward.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void GetStarterRecipes_CoversMultipleCategories()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();

        // Assert
        var categories = recipes.Select(r => r.Category).Distinct().ToList();
        categories.Should().Contain(RecipeCategory.Weapons);
        categories.Should().Contain(RecipeCategory.Armor);
        categories.Should().Contain(RecipeCategory.Consumables);
        categories.Should().HaveCount(3);
    }

    #endregion

    #region GetAdvancedRecipes Tests

    [Fact]
    public void GetAdvancedRecipes_ReturnsExpectedCount()
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();

        // Assert
        recipes.Should().HaveCount(4);
    }

    [Fact]
    public void GetAdvancedRecipes_AllRecipesAreLockedByDefault()
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();

        // Assert
        recipes.Where(r => r.RecipeId != "recipe_leather_armor")
               .Should().AllSatisfy(recipe => recipe.IsUnlocked.Should().BeFalse());
    }

    [Fact]
    public void GetAdvancedRecipes_ContainsExpectedRecipeIds()
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();

        // Assert
        var recipeIds = recipes.Select(r => r.RecipeId).ToList();
        recipeIds.Should().Contain("recipe_steel_sword");
        recipeIds.Should().Contain("recipe_iron_shield");
        recipeIds.Should().Contain("recipe_greater_health_potion");
        recipeIds.Should().Contain("recipe_leather_armor");
    }

    [Fact]
    public void GetAdvancedRecipes_HigherDifficultyThanStarter()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var maxStarterDifficulty = starterRecipes.Max(r => r.Difficulty);

        // Act
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();

        // Assert
        advancedRecipes.Should().AllSatisfy(recipe =>
        {
            recipe.Difficulty.Should().BeGreaterThanOrEqualTo(maxStarterDifficulty);
        });
    }

    [Fact]
    public void GetAdvancedRecipes_HigherValueThanStarter()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var maxStarterValue = starterRecipes.Max(r => r.Result.BaseValue);

        // Act
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();

        // Assert
        advancedRecipes.Should().AllSatisfy(recipe =>
        {
            recipe.Result.BaseValue.Should().BeGreaterThanOrEqualTo(maxStarterValue);
        });
    }

    #endregion

    #region Individual Recipe Tests

    [Theory]
    [InlineData("recipe_iron_sword", "Iron Sword", RecipeCategory.Weapons, ItemType.Weapon, 30.0, 15)]
    [InlineData("recipe_wooden_shield", "Wooden Shield", RecipeCategory.Armor, ItemType.Armor, 25.0, 12)]
    [InlineData("recipe_health_potion", "Health Potion", RecipeCategory.Consumables, ItemType.Consumable, 15.0, 10)]
    public void StarterRecipe_HasExpectedProperties(string expectedId, string expectedName, 
        RecipeCategory expectedCategory, ItemType expectedItemType, double expectedCraftingTime, int expectedExp)
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();
        var recipe = recipes.FirstOrDefault(r => r.RecipeId == expectedId);

        // Assert
        recipe.Should().NotBeNull();
        recipe!.Name.Should().Be(expectedName);
        recipe.Category.Should().Be(expectedCategory);
        recipe.Result.ItemType.Should().Be(expectedItemType);
        recipe.CraftingTime.Should().Be(expectedCraftingTime);
        recipe.ExperienceReward.Should().Be(expectedExp);
    }

    [Theory]
    [InlineData("recipe_steel_sword", "Steel Sword", RecipeCategory.Weapons, ItemType.Weapon, 60.0, 25)]
    [InlineData("recipe_iron_shield", "Iron Shield", RecipeCategory.Armor, ItemType.Armor, 45.0, 20)]
    [InlineData("recipe_greater_health_potion", "Greater Health Potion", RecipeCategory.Consumables, ItemType.Consumable, 30.0, 18)]
    [InlineData("recipe_leather_armor", "Leather Armor", RecipeCategory.Armor, ItemType.Armor, 50.0, 22)]
    public void AdvancedRecipe_HasExpectedProperties(string expectedId, string expectedName, 
        RecipeCategory expectedCategory, ItemType expectedItemType, double expectedCraftingTime, int expectedExp)
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();
        var recipe = recipes.FirstOrDefault(r => r.RecipeId == expectedId);

        // Assert
        recipe.Should().NotBeNull();
        recipe!.Name.Should().Be(expectedName);
        recipe.Category.Should().Be(expectedCategory);
        recipe.Result.ItemType.Should().Be(expectedItemType);
        recipe.CraftingTime.Should().Be(expectedCraftingTime);
        recipe.ExperienceReward.Should().Be(expectedExp);
    }

    [Fact]
    public void IronSwordRecipe_HasCorrectMaterialRequirements()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();
        var ironSword = recipes.First(r => r.RecipeId == "recipe_iron_sword");

        // Assert
        ironSword.MaterialRequirements.Should().HaveCount(2);
        ironSword.MaterialRequirements.Should().Contain(req => 
            req.MaterialCategory == Category.Metal && req.MinimumQuality == QualityTier.Common && req.Quantity == 3);
        ironSword.MaterialRequirements.Should().Contain(req => 
            req.MaterialCategory == Category.Wood && req.MinimumQuality == QualityTier.Common && req.Quantity == 1);
    }

    [Fact]
    public void WoodenShieldRecipe_HasCorrectMaterialRequirements()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();
        var woodenShield = recipes.First(r => r.RecipeId == "recipe_wooden_shield");

        // Assert
        woodenShield.MaterialRequirements.Should().HaveCount(2);
        woodenShield.MaterialRequirements.Should().Contain(req => 
            req.MaterialCategory == Category.Wood && req.MinimumQuality == QualityTier.Common && req.Quantity == 4);
        woodenShield.MaterialRequirements.Should().Contain(req => 
            req.MaterialCategory == Category.Leather && req.MinimumQuality == QualityTier.Common && req.Quantity == 2);
    }

    [Fact]
    public void HealthPotionRecipe_HasCorrectMaterialRequirements()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();
        var healthPotion = recipes.First(r => r.RecipeId == "recipe_health_potion");

        // Assert
        healthPotion.MaterialRequirements.Should().HaveCount(2);
        healthPotion.MaterialRequirements.Should().Contain(req => 
            req.MaterialCategory == Category.Herb && req.MinimumQuality == QualityTier.Common && req.Quantity == 2);
        healthPotion.MaterialRequirements.Should().Contain(req => 
            req.MaterialCategory == Category.Gem && req.MinimumQuality == QualityTier.Common && req.Quantity == 1);
    }

    [Fact]
    public void SteelSwordRecipe_HasCorrectPrerequisite()
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();
        var steelSword = recipes.First(r => r.RecipeId == "recipe_steel_sword");

        // Assert
        steelSword.Prerequisites.Should().HaveCount(1);
        steelSword.Prerequisites.Should().Contain("recipe_iron_sword");
    }

    [Fact]
    public void IronShieldRecipe_HasCorrectPrerequisite()
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();
        var ironShield = recipes.First(r => r.RecipeId == "recipe_iron_shield");

        // Assert
        ironShield.Prerequisites.Should().HaveCount(1);
        ironShield.Prerequisites.Should().Contain("recipe_wooden_shield");
    }

    [Fact]
    public void GreaterHealthPotionRecipe_HasCorrectPrerequisite()
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();
        var greaterPotion = recipes.First(r => r.RecipeId == "recipe_greater_health_potion");

        // Assert
        greaterPotion.Prerequisites.Should().HaveCount(1);
        greaterPotion.Prerequisites.Should().Contain("recipe_health_potion");
    }

    [Fact]
    public void LeatherArmorRecipe_HasNoPrerequisites()
    {
        // Act
        var recipes = StarterRecipes.GetAdvancedRecipes();
        var leatherArmor = recipes.First(r => r.RecipeId == "recipe_leather_armor");

        // Assert
        leatherArmor.Prerequisites.Should().BeEmpty();
    }

    #endregion

    #region Recipe Results Tests

    [Fact]
    public void IronSwordResult_HasCorrectProperties()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();
        var ironSword = recipes.First(r => r.RecipeId == "recipe_iron_sword");

        // Assert
        ironSword.Result.ItemId.Should().Be("iron_sword");
        ironSword.Result.ItemName.Should().Be("Iron Sword");
        ironSword.Result.ItemType.Should().Be(ItemType.Weapon);
        ironSword.Result.BaseQuality.Should().Be(QualityTier.Common);
        ironSword.Result.Quantity.Should().Be(1);
        ironSword.Result.BaseValue.Should().Be(50);
        ironSword.Result.ItemProperties.Should().ContainKey("DamageBonus");
        ironSword.Result.ItemProperties["DamageBonus"].Should().Be(8);
    }

    [Fact]
    public void WoodenShieldResult_HasCorrectProperties()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();
        var woodenShield = recipes.First(r => r.RecipeId == "recipe_wooden_shield");

        // Assert
        woodenShield.Result.ItemId.Should().Be("wooden_shield");
        woodenShield.Result.ItemName.Should().Be("Wooden Shield");
        woodenShield.Result.ItemType.Should().Be(ItemType.Armor);
        woodenShield.Result.BaseQuality.Should().Be(QualityTier.Common);
        woodenShield.Result.Quantity.Should().Be(1);
        woodenShield.Result.BaseValue.Should().Be(30);
        woodenShield.Result.ItemProperties.Should().ContainKey("DamageReduction");
        woodenShield.Result.ItemProperties["DamageReduction"].Should().Be(5);
    }

    [Fact]
    public void HealthPotionResult_HasCorrectProperties()
    {
        // Act
        var recipes = StarterRecipes.GetStarterRecipes();
        var healthPotion = recipes.First(r => r.RecipeId == "recipe_health_potion");

        // Assert
        healthPotion.Result.ItemId.Should().Be("health_potion");
        healthPotion.Result.ItemName.Should().Be("Health Potion");
        healthPotion.Result.ItemType.Should().Be(ItemType.Consumable);
        healthPotion.Result.BaseQuality.Should().Be(QualityTier.Common);
        healthPotion.Result.Quantity.Should().Be(1);
        healthPotion.Result.BaseValue.Should().Be(25);
        healthPotion.Result.ItemProperties.Should().ContainKey("HealingAmount");
        healthPotion.Result.ItemProperties["HealingAmount"].Should().Be(50);
    }

    [Fact]
    public void SteelSwordResult_HasBetterStatsThanIronSword()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var ironSword = starterRecipes.First(r => r.RecipeId == "recipe_iron_sword");
        var steelSword = advancedRecipes.First(r => r.RecipeId == "recipe_steel_sword");

        // Assert
        steelSword.Result.BaseValue.Should().BeGreaterThan(ironSword.Result.BaseValue);
        ((int)steelSword.Result.BaseQuality).Should().BeGreaterThan((int)ironSword.Result.BaseQuality);
        
        var ironDamage = (int)ironSword.Result.ItemProperties["DamageBonus"];
        var steelDamage = (int)steelSword.Result.ItemProperties["DamageBonus"];
        steelDamage.Should().BeGreaterThan(ironDamage);
    }

    [Fact]
    public void IronShieldResult_HasBetterStatsThanWoodenShield()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var woodenShield = starterRecipes.First(r => r.RecipeId == "recipe_wooden_shield");
        var ironShield = advancedRecipes.First(r => r.RecipeId == "recipe_iron_shield");

        // Assert
        ironShield.Result.BaseValue.Should().BeGreaterThan(woodenShield.Result.BaseValue);
        ((int)ironShield.Result.BaseQuality).Should().BeGreaterThan((int)woodenShield.Result.BaseQuality);
        
        var woodenReduction = (int)woodenShield.Result.ItemProperties["DamageReduction"];
        var ironReduction = (int)ironShield.Result.ItemProperties["DamageReduction"];
        ironReduction.Should().BeGreaterThan(woodenReduction);
    }

    [Fact]
    public void GreaterHealthPotionResult_HasBetterHealingThanBasicPotion()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var basicPotion = starterRecipes.First(r => r.RecipeId == "recipe_health_potion");
        var greaterPotion = advancedRecipes.First(r => r.RecipeId == "recipe_greater_health_potion");

        // Assert
        greaterPotion.Result.BaseValue.Should().BeGreaterThan(basicPotion.Result.BaseValue);
        ((int)greaterPotion.Result.BaseQuality).Should().BeGreaterThan((int)basicPotion.Result.BaseQuality);
        
        var basicHealing = (int)basicPotion.Result.ItemProperties["HealingAmount"];
        var greaterHealing = (int)greaterPotion.Result.ItemProperties["HealingAmount"];
        greaterHealing.Should().BeGreaterThan(basicHealing);
    }

    #endregion

    #region InitializeRecipeManager Tests

    [Fact]
    public void InitializeRecipeManager_WithNullManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => StarterRecipes.InitializeRecipeManager(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*recipeManager*");
    }

    [Fact]
    public void InitializeRecipeManager_AddsAllStarterRecipesAsUnlocked()
    {
        // Arrange
        var recipeManager = new RecipeManager();

        // Act
        StarterRecipes.InitializeRecipeManager(recipeManager);

        // Assert
        var starterRecipeIds = StarterRecipes.GetStarterRecipes().Select(r => r.RecipeId).ToList();
        
        foreach (var recipeId in starterRecipeIds)
        {
            recipeManager.IsRecipeUnlocked(recipeId).Should().BeTrue($"starter recipe {recipeId} should be unlocked");
        }
    }

    [Fact]
    public void InitializeRecipeManager_AddsAllAdvancedRecipesAsLocked()
    {
        // Arrange
        var recipeManager = new RecipeManager();

        // Act
        StarterRecipes.InitializeRecipeManager(recipeManager);

        // Assert
        var advancedRecipeIds = StarterRecipes.GetAdvancedRecipes().Select(r => r.RecipeId).ToList();
        
        foreach (var recipeId in advancedRecipeIds)
        {
            recipeManager.IsRecipeUnlocked(recipeId).Should().BeFalse($"advanced recipe {recipeId} should be locked");
        }
    }

    [Fact]
    public void InitializeRecipeManager_AddsExpectedTotalRecipeCount()
    {
        // Arrange
        var recipeManager = new RecipeManager();
        var expectedTotal = StarterRecipes.GetStarterRecipes().Count + StarterRecipes.GetAdvancedRecipes().Count;

        // Act
        StarterRecipes.InitializeRecipeManager(recipeManager);

        // Assert
        recipeManager.AllRecipes.Should().HaveCount(expectedTotal);
    }

    [Fact]
    public void InitializeRecipeManager_AllRecipesAreAccessible()
    {
        // Arrange
        var recipeManager = new RecipeManager();

        // Act
        StarterRecipes.InitializeRecipeManager(recipeManager);

        // Assert
        var starterRecipeIds = StarterRecipes.GetStarterRecipes().Select(r => r.RecipeId).ToList();
        var advancedRecipeIds = StarterRecipes.GetAdvancedRecipes().Select(r => r.RecipeId).ToList();
        var allExpectedIds = starterRecipeIds.Concat(advancedRecipeIds).ToList();

        var actualIds = recipeManager.AllRecipes.Select(r => r.RecipeId).ToList();
        actualIds.Should().Contain(allExpectedIds);
    }

    [Fact]
    public void InitializeRecipeManager_CanQueryByCategory()
    {
        // Arrange
        var recipeManager = new RecipeManager();

        // Act
        StarterRecipes.InitializeRecipeManager(recipeManager);

        // Assert
        var weaponRecipes = recipeManager.GetUnlockedRecipesByCategory(RecipeCategory.Weapons);
        var armorRecipes = recipeManager.GetUnlockedRecipesByCategory(RecipeCategory.Armor);
        var consumableRecipes = recipeManager.GetUnlockedRecipesByCategory(RecipeCategory.Consumables);

        weaponRecipes.Should().NotBeEmpty();
        armorRecipes.Should().NotBeEmpty();
        consumableRecipes.Should().NotBeEmpty();
    }

    #endregion

    #region Data Consistency Tests

    [Fact]
    public void AllRecipes_HaveUniqueIds()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var allRecipes = starterRecipes.Concat(advancedRecipes).ToList();

        // Act
        var recipeIds = allRecipes.Select(r => r.RecipeId).ToList();

        // Assert
        recipeIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllRecipes_HaveUniqueNames()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var allRecipes = starterRecipes.Concat(advancedRecipes).ToList();

        // Act
        var recipeNames = allRecipes.Select(r => r.Name).ToList();

        // Assert
        recipeNames.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllPrerequisites_ReferenceValidRecipes()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var allRecipes = starterRecipes.Concat(advancedRecipes).ToList();
        var allRecipeIds = allRecipes.Select(r => r.RecipeId).ToHashSet();

        // Act & Assert
        foreach (var recipe in allRecipes)
        {
            foreach (var prerequisite in recipe.Prerequisites)
            {
                allRecipeIds.Should().Contain(prerequisite, 
                    $"prerequisite '{prerequisite}' in recipe '{recipe.RecipeId}' should reference a valid recipe");
            }
        }
    }

    [Fact]
    public void AllMaterialRequirements_HaveValidQuantities()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var allRecipes = starterRecipes.Concat(advancedRecipes).ToList();

        // Act & Assert
        foreach (var recipe in allRecipes)
        {
            foreach (var requirement in recipe.MaterialRequirements)
            {
                requirement.Quantity.Should().BeGreaterThan(0, 
                    $"material requirement in recipe '{recipe.RecipeId}' should have positive quantity");
            }
        }
    }

    [Fact]
    public void AllRecipeResults_HaveValidProperties()
    {
        // Arrange
        var starterRecipes = StarterRecipes.GetStarterRecipes();
        var advancedRecipes = StarterRecipes.GetAdvancedRecipes();
        var allRecipes = starterRecipes.Concat(advancedRecipes).ToList();

        // Act & Assert
        foreach (var recipe in allRecipes)
        {
            var result = recipe.Result;
            result.ItemId.Should().NotBeNullOrEmpty($"recipe '{recipe.RecipeId}' should have valid ItemId");
            result.ItemName.Should().NotBeNullOrEmpty($"recipe '{recipe.RecipeId}' should have valid ItemName");
            result.Quantity.Should().BeGreaterThan(0, $"recipe '{recipe.RecipeId}' should produce positive quantity");
            result.BaseValue.Should().BeGreaterThan(0, $"recipe '{recipe.RecipeId}' should have positive base value");
        }
    }

    #endregion
}
