#nullable enable

using Game.Core.Tests;
using Game.Core.Utils;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Crafting.Tests.CQS;

/// <summary>
/// Test helpers and utilities for CQS testing in the crafting system.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a test recipe for use in tests.
    /// </summary>
    public static Recipe CreateTestRecipe(string id = "test_recipe", bool unlocked = false)
    {
        var materials = new List<MaterialRequirement>
        {
            new(Category.Metal, QualityTier.Common, 2),
            new(Category.Wood, QualityTier.Common, 1)
        };

        var result = new CraftingResult(
            "iron_sword",
            "Iron Sword",
            ItemType.Weapon,
            QualityTier.Common,
            1,
            50);

        return new Recipe(
            id,
            "Test Recipe",
            "A recipe for testing",
            RecipeCategory.Weapons,
            materials,
            result,
            30.0,
            isUnlocked: unlocked);
    }

    /// <summary>
    /// Creates a test crafting order for use in tests.
    /// </summary>
    public static CraftingOrder CreateTestOrder(string id = "test_order")
    {
        var recipe = CreateTestRecipe();
        
        // Create materials that satisfy the recipe requirements:
        // Recipe needs: 2x Metal (Common+), 1x Wood (Common+)
        var materials = new Dictionary<string, Material>
        {
            ["metal1"] = new Material("iron_ore_1", "Iron Ore 1", "Basic metal", QualityTier.Common, 10, Category.Metal),
            ["metal2"] = new Material("iron_ore_2", "Iron Ore 2", "Basic metal", QualityTier.Common, 10, Category.Metal),
            ["wood1"] = new Material("oak_wood", "Oak Wood", "Quality wood", QualityTier.Common, 5, Category.Wood)
        };

        return new CraftingOrder(id, recipe, materials);
    }

    /// <summary>
    /// Creates a test material for use in tests.
    /// </summary>
    public static Material CreateTestMaterial(
        string id = "test_material",
        Category category = Category.Metal,
        QualityTier quality = QualityTier.Common)
    {
        return new Material(id, "Test Material", "A material for testing", quality, 10, category);
    }

    /// <summary>
    /// Sets up a testable logger backend to capture log output during tests.
    /// </summary>
    public static TestableLoggerBackend SetupTestLogging()
    {
        var backend = new TestableLoggerBackend();
        GameLogger.SetBackend(backend);
        return backend;
    }
}