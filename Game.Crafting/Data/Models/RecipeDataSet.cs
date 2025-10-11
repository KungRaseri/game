using System.Text.Json.Serialization;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Crafting.Models;

namespace Game.Crafting.Data.Models;

/// <summary>
/// Root data structure for all recipe configurations loaded from JSON.
/// </summary>
public class RecipeDataSet
{
    [JsonPropertyName("starterRecipes")]
    public List<RecipeData> StarterRecipes { get; set; } = [];

    [JsonPropertyName("advancedRecipes")]
    public List<RecipeData> AdvancedRecipes { get; set; } = [];

    [JsonPropertyName("phase1Recipes")]
    public List<RecipeData> Phase1Recipes { get; set; } = [];

    /// <summary>
    /// Converts this data set to domain recipe objects.
    /// </summary>
    public RecipeCollection ToRecipeCollection()
    {
        var allRecipes = new List<Recipe>();

        allRecipes.AddRange(StarterRecipes.Select(r => r.ToRecipe()));
        allRecipes.AddRange(AdvancedRecipes.Select(r => r.ToRecipe()));
        allRecipes.AddRange(Phase1Recipes.Select(r => r.ToRecipe()));

        return new RecipeCollection(allRecipes);
    }
}

/// <summary>
/// JSON representation of a crafting recipe.
/// </summary>
public class RecipeData
{
    [JsonPropertyName("recipeId")]
    public string RecipeId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("materialRequirements")]
    public List<MaterialRequirementData> MaterialRequirements { get; set; } = [];

    [JsonPropertyName("result")]
    public CraftingResultData Result { get; set; } = new();

    [JsonPropertyName("craftingTime")]
    public double CraftingTime { get; set; }

    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; } = 1;

    [JsonPropertyName("prerequisites")]
    public List<string> Prerequisites { get; set; } = [];

    [JsonPropertyName("isUnlocked")]
    public bool IsUnlocked { get; set; } = false;

    [JsonPropertyName("experienceReward")]
    public int ExperienceReward { get; set; } = 10;

    /// <summary>
    /// Converts this data object to a domain Recipe object.
    /// </summary>
    public Recipe ToRecipe()
    {
        if (!Enum.TryParse<RecipeCategory>(Category, true, out var recipeCategory))
        {
            throw new InvalidOperationException($"Invalid recipe category: {Category}");
        }

        var materialRequirements = MaterialRequirements.Select(mr => mr.ToMaterialRequirement()).ToList();
        var craftingResult = Result.ToCraftingResult();

        return new Recipe(
            recipeId: RecipeId,
            name: Name,
            description: Description,
            category: recipeCategory,
            materialRequirements: materialRequirements,
            result: craftingResult,
            craftingTime: CraftingTime,
            difficulty: Difficulty,
            prerequisites: Prerequisites,
            isUnlocked: IsUnlocked,
            experienceReward: ExperienceReward
        );
    }
}

/// <summary>
/// JSON representation of a material requirement.
/// </summary>
public class MaterialRequirementData
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("qualityTier")]
    public string QualityTier { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Converts this data object to a domain MaterialRequirement object.
    /// </summary>
    public MaterialRequirement ToMaterialRequirement()
    {
        if (!Enum.TryParse<Category>(Category, true, out var category))
        {
            throw new InvalidOperationException($"Invalid material category: {Category}");
        }

        if (!Enum.TryParse<QualityTier>(QualityTier, true, out var qualityTier))
        {
            throw new InvalidOperationException($"Invalid quality tier: {QualityTier}");
        }

        return new MaterialRequirement(category, qualityTier, Quantity);
    }
}

/// <summary>
/// JSON representation of a crafting result.
/// </summary>
public class CraftingResultData
{
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("itemName")]
    public string ItemName { get; set; } = string.Empty;

    [JsonPropertyName("itemType")]
    public string ItemType { get; set; } = string.Empty;

    [JsonPropertyName("baseQuality")]
    public string BaseQuality { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("baseValue")]
    public int BaseValue { get; set; }

    [JsonPropertyName("itemProperties")]
    public Dictionary<string, object> ItemProperties { get; set; } = [];

    /// <summary>
    /// Converts this data object to a domain CraftingResult object.
    /// </summary>
    public CraftingResult ToCraftingResult()
    {
        if (!Enum.TryParse<ItemType>(ItemType, true, out var itemType))
        {
            throw new InvalidOperationException($"Invalid item type: {ItemType}");
        }

        if (!Enum.TryParse<QualityTier>(BaseQuality, true, out var baseQuality))
        {
            throw new InvalidOperationException($"Invalid base quality: {BaseQuality}");
        }

        return new CraftingResult(
            itemId: ItemId,
            itemName: ItemName,
            itemType: itemType,
            baseQuality: baseQuality,
            quantity: Quantity,
            baseValue: BaseValue,
            itemProperties: ItemProperties
        );
    }
}

/// <summary>
/// Collection wrapper for converted recipes.
/// </summary>
public class RecipeCollection
{
    public IReadOnlyList<Recipe> AllRecipes { get; }
    public IReadOnlyList<Recipe> StarterRecipes { get; }
    public IReadOnlyList<Recipe> AdvancedRecipes { get; }
    public IReadOnlyList<Recipe> Phase1Recipes { get; }

    public RecipeCollection(IEnumerable<Recipe> recipes)
    {
        AllRecipes = recipes.ToList();
        
        // Note: This is a simplified approach. In a more complex system,
        // you might want to track the original categorization from the JSON
        StarterRecipes = AllRecipes.Where(r => r.IsUnlocked).ToList();
        AdvancedRecipes = AllRecipes.Where(r => !r.IsUnlocked && r.Prerequisites.Any()).ToList();
        Phase1Recipes = AllRecipes.Where(r => r.IsUnlocked && !r.Prerequisites.Any()).ToList();
    }
}
