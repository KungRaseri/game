#nullable enable

using Xunit;
using System.Text.Json;
using Game.Crafting.Data.Models;
using Game.Crafting.Models;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Crafting.Tests.Data;

/// <summary>
/// Integration tests for Recipe JSON serialization with enum converters.
/// </summary>
public class RecipeJsonSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public RecipeJsonSerializationTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new Game.Core.Serialization.EnumJsonConverterFactory() }
        };
    }

    [Fact]
    public void RecipeData_Deserialize_ParsesEnumsCorrectly()
    {
        // Arrange
        var json = """
        {
            "recipeId": "test_recipe",
            "name": "Test Recipe",
            "description": "A test recipe",
            "category": "Weapons",
            "materialRequirements": [
                {
                    "category": "Metal",
                    "qualityTier": "Common",
                    "quantity": 5
                }
            ],
            "result": {
                "itemId": "test_item",
                "itemName": "Test Item",
                "itemType": "Weapon",
                "baseQuality": "Uncommon",
                "quantity": 1,
                "baseValue": 100,
                "itemProperties": {}
            },
            "craftingTime": 30.0,
            "difficulty": 1,
            "prerequisites": [],
            "isUnlocked": true,
            "experienceReward": 10
        }
        """;

        // Act
        var recipe = JsonSerializer.Deserialize<RecipeData>(json, _options);

        // Assert
        Assert.NotNull(recipe);
        Assert.Equal(RecipeCategory.Weapons, recipe.Category);
        Assert.Single(recipe.MaterialRequirements);
        Assert.Equal(Category.Metal, recipe.MaterialRequirements[0].Category);
        Assert.Equal(QualityTier.Common, recipe.MaterialRequirements[0].QualityTier);
        Assert.Equal(ItemType.Weapon, recipe.Result.ItemType);
        Assert.Equal(QualityTier.Uncommon, recipe.Result.BaseQuality);
    }

    [Fact]
    public void RecipeData_Serialize_WritesEnumsAsStrings()
    {
        // Arrange
        var recipe = new RecipeData
        {
            RecipeId = "test_recipe",
            Name = "Test Recipe",
            Description = "A test recipe",
            Category = RecipeCategory.Armor,
            MaterialRequirements = new List<MaterialRequirementData>
            {
                new()
                {
                    Category = Category.Leather,
                    QualityTier = QualityTier.Rare,
                    Quantity = 3
                }
            },
            Result = new CraftingResultData
            {
                ItemId = "test_armor",
                ItemName = "Test Armor",
                ItemType = ItemType.Armor,
                BaseQuality = QualityTier.Rare,
                Quantity = 1,
                BaseValue = 200
            }
        };

        // Act
        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new Game.Core.Serialization.EnumJsonConverterFactory() }
        };
        var json = JsonSerializer.Serialize(recipe, serializeOptions);

        // Assert
        Assert.Contains("\"category\": \"Armor\"", json);
        Assert.Contains("\"category\": \"Leather\"", json);
        Assert.Contains("\"qualityTier\": \"Rare\"", json);
        Assert.Contains("\"itemType\": \"Armor\"", json);
        Assert.Contains("\"baseQuality\": \"Rare\"", json);
    }

    [Fact]
    public void RecipeData_CaseInsensitive_ParsesCorrectly()
    {
        // Arrange
        var json = """
        {
            "recipeId": "test",
            "name": "Test",
            "description": "Test",
            "category": "weapons",
            "materialRequirements": [
                {
                    "category": "METAL",
                    "qualityTier": "common",
                    "quantity": 1
                }
            ],
            "result": {
                "itemId": "test",
                "itemName": "Test",
                "itemType": "weapon",
                "baseQuality": "UNCOMMON",
                "quantity": 1,
                "baseValue": 10,
                "itemProperties": {}
            },
            "craftingTime": 30.0,
            "difficulty": 1,
            "prerequisites": [],
            "isUnlocked": true,
            "experienceReward": 10
        }
        """;

        // Act
        var recipe = JsonSerializer.Deserialize<RecipeData>(json, _options);

        // Assert
        Assert.NotNull(recipe);
        Assert.Equal(RecipeCategory.Weapons, recipe.Category);
        Assert.Equal(Category.Metal, recipe.MaterialRequirements[0].Category);
        Assert.Equal(QualityTier.Common, recipe.MaterialRequirements[0].QualityTier);
        Assert.Equal(ItemType.Weapon, recipe.Result.ItemType);
        Assert.Equal(QualityTier.Uncommon, recipe.Result.BaseQuality);
    }

    [Fact]
    public void RecipeData_InvalidCategory_ThrowsJsonException()
    {
        // Arrange
        var json = """
        {
            "recipeId": "test",
            "name": "Test",
            "description": "Test",
            "category": "InvalidCategory",
            "materialRequirements": [],
            "result": {
                "itemId": "test",
                "itemName": "Test",
                "itemType": "Weapon",
                "baseQuality": "Common",
                "quantity": 1,
                "baseValue": 10,
                "itemProperties": {}
            },
            "craftingTime": 30.0,
            "difficulty": 1,
            "prerequisites": [],
            "isUnlocked": true,
            "experienceReward": 10
        }
        """;

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<RecipeData>(json, _options)
        );

        Assert.Contains("Invalid", exception.Message);
        Assert.Contains("RecipeCategory", exception.Message);
    }

    [Fact]
    public void RecipeData_ToRecipe_ConvertsCorrectly()
    {
        // Arrange
        var recipeData = new RecipeData
        {
            RecipeId = "test_recipe",
            Name = "Test Recipe",
            Description = "A test recipe",
            Category = RecipeCategory.Consumables,
            MaterialRequirements = new List<MaterialRequirementData>
            {
                new()
                {
                    Category = Category.Herb,
                    QualityTier = QualityTier.Common,
                    Quantity = 2
                }
            },
            Result = new CraftingResultData
            {
                ItemId = "test_potion",
                ItemName = "Test Potion",
                ItemType = ItemType.Consumable,
                BaseQuality = QualityTier.Common,
                Quantity = 1,
                BaseValue = 50
            },
            CraftingTime = 15.0,
            Difficulty = 1,
            IsUnlocked = true,
            ExperienceReward = 5
        };

        // Act
        var recipe = recipeData.ToRecipe();

        // Assert
        Assert.Equal("test_recipe", recipe.RecipeId);
        Assert.Equal(RecipeCategory.Consumables, recipe.Category);
        Assert.Single(recipe.MaterialRequirements);
        Assert.Equal(Category.Herb, recipe.MaterialRequirements[0].MaterialCategory);
        Assert.Equal(ItemType.Consumable, recipe.Result.ItemType);
    }
}
