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
    [JsonPropertyName("BasicRecipes")]
    public List<RecipeData> BasicRecipes { get; set; } = [];

        [JsonPropertyName("AdvancedRecipes")]
    public List<RecipeData> AdvancedRecipes { get; set; } = [];

    /// <summary>
    /// Converts this data set to domain recipe objects.
    /// </summary>
    public RecipeCollection ToRecipeCollection()
    {
        var allRecipes = new List<Recipe>();

        allRecipes.AddRange(BasicRecipes.Select(r => r.ToRecipe()));
        allRecipes.AddRange(AdvancedRecipes.Select(r => r.ToRecipe()));

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
    public RecipeCategory Category { get; set; }

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
        var materialRequirements = MaterialRequirements.Select(mr => mr.ToMaterialRequirement()).ToList();
        var craftingResult = Result.ToCraftingResult();

        return new Recipe(
            recipeId: RecipeId,
            name: Name,
            description: Description,
            category: Category,
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
    public Category Category { get; set; }

    [JsonPropertyName("qualityTier")]
    public QualityTier QualityTier { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Converts this data object to a domain MaterialRequirement object.
    /// </summary>
    public MaterialRequirement ToMaterialRequirement()
    {
        return new MaterialRequirement(Category, QualityTier, Quantity);
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
    public ItemType ItemType { get; set; }

    [JsonPropertyName("baseQuality")]
    public QualityTier BaseQuality { get; set; }

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
        return new CraftingResult(
            itemId: ItemId,
            itemName: ItemName,
            itemType: ItemType,
            baseQuality: BaseQuality,
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
    public IReadOnlyList<Recipe> BasicRecipes { get; }
    public IReadOnlyList<Recipe> AdvancedRecipes { get; }

    public RecipeCollection(IEnumerable<Recipe> recipes)
    {
        AllRecipes = recipes.ToList();
        
        // Categorize recipes based on their unlocked status
        BasicRecipes = AllRecipes.Where(r => r.IsUnlocked).ToList();
        AdvancedRecipes = AllRecipes.Where(r => !r.IsUnlocked).ToList();
    }
}
