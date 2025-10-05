using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Crafting.Models;
using Game.Crafting.Systems;

namespace Game.Crafting.Data;

/// <summary>
/// Static data provider for starter crafting recipes.
/// These are the initial recipes available in the game.
/// </summary>
public static class StarterRecipes
{
    /// <summary>
    /// Gets all starter recipes that should be available when the game begins.
    /// </summary>
    /// <returns>List of starter recipes</returns>
    public static IReadOnlyList<Recipe> GetStarterRecipes()
    {
        return new List<Recipe>
        {
            CreateIronSwordRecipe(),
            CreateWoodenShieldRecipe(),
            CreateHealthPotionRecipe()
        };
    }

    /// <summary>
    /// Creates the Iron Sword recipe.
    /// A basic weapon that requires iron ore and wood for the handle.
    /// </summary>
    /// <returns>Iron Sword recipe</returns>
    private static Recipe CreateIronSwordRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Metal, QualityTier.Common, 3), // 3x Iron Ore (or better)
            new(Category.Wood, QualityTier.Common, 1)   // 1x Wood for handle
        };

        var result = new CraftingResult(
            itemId: "iron_sword",
            itemName: "Iron Sword",
            itemType: ItemType.Weapon,
            baseQuality: QualityTier.Common,
            quantity: 1,
            baseValue: 50,
            itemProperties: new Dictionary<string, object>
            {
                ["DamageBonus"] = 8
            });

        return new Recipe(
            recipeId: "recipe_iron_sword",
            name: "Iron Sword",
            description: "A sturdy iron sword suitable for novice adventurers. Requires basic metalworking skills.",
            category: RecipeCategory.Weapons,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 30.0, // 30 seconds
            difficulty: 2,
            prerequisites: new List<string>(), // No prerequisites - starter recipe
            isUnlocked: true, // Start unlocked
            experienceReward: 15);
    }

    /// <summary>
    /// Creates the Wooden Shield recipe.
    /// A basic defensive item made primarily from wood with leather strapping.
    /// </summary>
    /// <returns>Wooden Shield recipe</returns>
    private static Recipe CreateWoodenShieldRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Wood, QualityTier.Common, 4),    // 4x Wood planks
            new(Category.Leather, QualityTier.Common, 2)  // 2x Leather for straps
        };

        var result = new CraftingResult(
            itemId: "wooden_shield",
            itemName: "Wooden Shield",
            itemType: ItemType.Armor,
            baseQuality: QualityTier.Common,
            quantity: 1,
            baseValue: 30,
            itemProperties: new Dictionary<string, object>
            {
                ["DamageReduction"] = 5
            });

        return new Recipe(
            recipeId: "recipe_wooden_shield",
            name: "Wooden Shield",
            description: "A simple wooden shield reinforced with leather straps. Provides basic protection.",
            category: RecipeCategory.Armor,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 25.0, // 25 seconds
            difficulty: 1,
            prerequisites: new List<string>(), // No prerequisites - starter recipe
            isUnlocked: true, // Start unlocked
            experienceReward: 12);
    }

    /// <summary>
    /// Creates the Health Potion recipe.
    /// A basic consumable that restores health, made from herbs.
    /// </summary>
    /// <returns>Health Potion recipe</returns>
    private static Recipe CreateHealthPotionRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Herb, QualityTier.Common, 2), // 2x Healing herbs
            new(Category.Gem, QualityTier.Common, 1)   // 1x Crystal for magical infusion
        };

        var result = new CraftingResult(
            itemId: "health_potion",
            itemName: "Health Potion",
            itemType: ItemType.Consumable,
            baseQuality: QualityTier.Common,
            quantity: 1,
            baseValue: 25,
            itemProperties: new Dictionary<string, object>
            {
                ["HealingAmount"] = 50
            });

        return new Recipe(
            recipeId: "recipe_health_potion",
            name: "Health Potion",
            description: "A basic healing potion brewed from common herbs. Restores health when consumed.",
            category: RecipeCategory.Consumables,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 15.0, // 15 seconds
            difficulty: 1,
            prerequisites: new List<string>(), // No prerequisites - starter recipe
            isUnlocked: true, // Start unlocked
            experienceReward: 10);
    }

    /// <summary>
    /// Gets additional recipes that can be unlocked as the player progresses.
    /// These require the starter recipes as prerequisites.
    /// </summary>
    /// <returns>List of advanced recipes</returns>
    public static IReadOnlyList<Recipe> GetAdvancedRecipes()
    {
        return new List<Recipe>
        {
            CreateSteelSwordRecipe(),
            CreateIronShieldRecipe(),
            CreateGreaterHealthPotionRecipe(),
            CreateLeatherArmorRecipe()
        };
    }

    /// <summary>
    /// Creates the Steel Sword recipe (advanced).
    /// Requires knowledge of iron sword crafting.
    /// </summary>
    /// <returns>Steel Sword recipe</returns>
    private static Recipe CreateSteelSwordRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Metal, QualityTier.Uncommon, 4), // 4x High-quality metal
            new(Category.Wood, QualityTier.Uncommon, 1),  // 1x Quality wood
            new(Category.Gem, QualityTier.Common, 1)      // 1x Gem for enhancement
        };

        var result = new CraftingResult(
            itemId: "steel_sword",
            itemName: "Steel Sword",
            itemType: ItemType.Weapon,
            baseQuality: QualityTier.Uncommon,
            quantity: 1,
            baseValue: 120,
            itemProperties: new Dictionary<string, object>
            {
                ["DamageBonus"] = 15
            });

        return new Recipe(
            recipeId: "recipe_steel_sword",
            name: "Steel Sword",
            description: "An improved sword made from high-quality steel. Significantly more effective than iron.",
            category: RecipeCategory.Weapons,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 60.0, // 1 minute
            difficulty: 4,
            prerequisites: new List<string> { "recipe_iron_sword" },
            isUnlocked: false, // Must be discovered/unlocked
            experienceReward: 25);
    }

    /// <summary>
    /// Creates the Iron Shield recipe (advanced).
    /// An upgrade to the wooden shield.
    /// </summary>
    /// <returns>Iron Shield recipe</returns>
    private static Recipe CreateIronShieldRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Metal, QualityTier.Common, 3),   // 3x Iron
            new(Category.Leather, QualityTier.Common, 2), // 2x Leather
            new(Category.Wood, QualityTier.Common, 1)     // 1x Wood for backing
        };

        var result = new CraftingResult(
            itemId: "iron_shield",
            itemName: "Iron Shield",
            itemType: ItemType.Armor,
            baseQuality: QualityTier.Uncommon,
            quantity: 1,
            baseValue: 75,
            itemProperties: new Dictionary<string, object>
            {
                ["DamageReduction"] = 10
            });

        return new Recipe(
            recipeId: "recipe_iron_shield",
            name: "Iron Shield",
            description: "A reinforced shield with iron plating. Much more durable than wood alone.",
            category: RecipeCategory.Armor,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 45.0, // 45 seconds
            difficulty: 3,
            prerequisites: new List<string> { "recipe_wooden_shield" },
            isUnlocked: false,
            experienceReward: 20);
    }

    /// <summary>
    /// Creates the Greater Health Potion recipe (advanced).
    /// A more potent healing potion.
    /// </summary>
    /// <returns>Greater Health Potion recipe</returns>
    private static Recipe CreateGreaterHealthPotionRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Herb, QualityTier.Uncommon, 3),  // 3x Quality herbs
            new(Category.Gem, QualityTier.Uncommon, 2)    // 2x Quality crystals
        };

        var result = new CraftingResult(
            itemId: "greater_health_potion",
            itemName: "Greater Health Potion",
            itemType: ItemType.Consumable,
            baseQuality: QualityTier.Uncommon,
            quantity: 1,
            baseValue: 60,
            itemProperties: new Dictionary<string, object>
            {
                ["HealingAmount"] = 100
            });

        return new Recipe(
            recipeId: "recipe_greater_health_potion",
            name: "Greater Health Potion",
            description: "An enhanced healing potion with double the potency of the basic version.",
            category: RecipeCategory.Consumables,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 30.0, // 30 seconds
            difficulty: 3,
            prerequisites: new List<string> { "recipe_health_potion" },
            isUnlocked: false,
            experienceReward: 18);
    }

    /// <summary>
    /// Creates the Leather Armor recipe (advanced).
    /// Basic body armor for protection.
    /// </summary>
    /// <returns>Leather Armor recipe</returns>
    private static Recipe CreateLeatherArmorRecipe()
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Leather, QualityTier.Common, 6), // 6x Leather pieces
            new(Category.Metal, QualityTier.Common, 2)    // 2x Metal for buckles/studs
        };

        var result = new CraftingResult(
            itemId: "leather_armor",
            itemName: "Leather Armor",
            itemType: ItemType.Armor,
            baseQuality: QualityTier.Common,
            quantity: 1,
            baseValue: 80,
            itemProperties: new Dictionary<string, object>
            {
                ["DamageReduction"] = 8
            });

        return new Recipe(
            recipeId: "recipe_leather_armor",
            name: "Leather Armor",
            description: "Flexible leather armor that provides good protection without hindering movement.",
            category: RecipeCategory.Armor,
            materialRequirements: materialRequirements,
            result: result,
            craftingTime: 50.0, // 50 seconds
            difficulty: 3,
            prerequisites: new List<string>(), // No prerequisites - can be discovered independently
            isUnlocked: false,
            experienceReward: 22);
    }

    /// <summary>
    /// Initializes a recipe manager with all starter and advanced recipes.
    /// </summary>
    /// <param name="recipeManager">The recipe manager to populate</param>
    public static void InitializeRecipeManager(RecipeManager recipeManager)
    {
        if (recipeManager == null)
        {
            throw new ArgumentNullException(nameof(recipeManager));
        }

        // Add starter recipes (unlocked by default)
        foreach (var recipe in GetStarterRecipes())
        {
            recipeManager.AddRecipe(recipe, unlocked: true);
        }

        // Add advanced recipes (locked by default)
        foreach (var recipe in GetAdvancedRecipes())
        {
            recipeManager.AddRecipe(recipe, unlocked: false);
        }
    }
}
