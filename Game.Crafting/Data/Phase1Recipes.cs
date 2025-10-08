#nullable enable

using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Crafting.Models;

namespace Game.Crafting.Data;

/// <summary>
/// Static data provider for Phase 1 crafting recipes.
/// These recipes only use materials available through manual gathering: Oak Wood, Iron Ore, Simple Herbs.
/// </summary>
public static class Phase1Recipes
{
    /// <summary>
    /// Gets all Phase 1 recipes that use only gatherable materials.
    /// </summary>
    /// <returns>List of Phase 1 recipes</returns>
    public static IReadOnlyList<Recipe> GetPhase1Recipes()
    {
        return new List<Recipe>
        {
            CreateSimpleHealthPotionRecipe(),
            CreateBasicSwordRecipe(),
            CreateWoodenShieldRecipe()
        };
    }

    /// <summary>
    /// Creates the Simple Health Potion recipe.
    /// Uses only simple herbs gathered from the plains.
    /// </summary>
    /// <returns>Simple Health Potion recipe</returns>
    private static Recipe CreateSimpleHealthPotionRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Herb, QualityTier.Common, 2) // 2x Simple Herbs
        };

        var result = new CraftingResult(
            itemId: "simple_health_potion",
            itemName: "Simple Health Potion",
            itemType: ItemType.Consumable,
            baseQuality: QualityTier.Common,
            quantity: 1,
            baseValue: 15,
            itemProperties: new Dictionary<string, object>
            {
                ["HealingAmount"] = 25
            });

        return new Recipe(
            recipeId: "recipe_simple_health_potion",
            name: "Simple Health Potion",
            description: "A basic healing potion made from simple herbs. Restores a small amount of health.",
            category: RecipeCategory.Consumables,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 10.0, // 10 seconds
            difficulty: 1,
            prerequisites: new List<string>(), // No prerequisites - Phase 1 starter
            isUnlocked: true, // Start unlocked
            experienceReward: 8);
    }

    /// <summary>
    /// Creates the Basic Sword recipe.
    /// A simple sword made from iron ore and oak wood.
    /// </summary>
    /// <returns>Basic Sword recipe</returns>
    private static Recipe CreateBasicSwordRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Metal, QualityTier.Common, 3), // 3x Iron Ore
            new(Category.Wood, QualityTier.Common, 1)   // 1x Oak Wood for handle
        };

        var result = new CraftingResult(
            itemId: "basic_sword",
            itemName: "Basic Sword",
            itemType: ItemType.Weapon,
            baseQuality: QualityTier.Common,
            quantity: 1,
            baseValue: 35,
            itemProperties: new Dictionary<string, object>
            {
                ["DamageBonus"] = 6
            });

        return new Recipe(
            recipeId: "recipe_basic_sword",
            name: "Basic Sword",
            description: "A simple sword forged from iron ore with an oak wood handle. Good for starting adventurers.",
            category: RecipeCategory.Weapons,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 25.0, // 25 seconds
            difficulty: 2,
            prerequisites: new List<string>(), // No prerequisites - Phase 1 starter
            isUnlocked: true, // Start unlocked
            experienceReward: 12);
    }

    /// <summary>
    /// Creates the Wooden Shield recipe.
    /// A basic shield made entirely from oak wood (no leather required for Phase 1).
    /// </summary>
    /// <returns>Wooden Shield recipe</returns>
    private static Recipe CreateWoodenShieldRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Wood, QualityTier.Common, 4) // 4x Oak Wood
        };

        var result = new CraftingResult(
            itemId: "wooden_shield",
            itemName: "Wooden Shield",
            itemType: ItemType.Armor,
            baseQuality: QualityTier.Common,
            quantity: 1,
            baseValue: 20,
            itemProperties: new Dictionary<string, object>
            {
                ["DamageReduction"] = 3
            });

        return new Recipe(
            recipeId: "recipe_wooden_shield",
            name: "Wooden Shield",
            description: "A simple wooden shield crafted from oak planks. Provides basic protection.",
            category: RecipeCategory.Armor,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 20.0, // 20 seconds
            difficulty: 1,
            prerequisites: new List<string>(), // No prerequisites - Phase 1 starter
            isUnlocked: true, // Start unlocked
            experienceReward: 10);
    }
}
