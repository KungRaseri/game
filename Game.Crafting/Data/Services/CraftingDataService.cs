using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;
using Game.Core.Utils;
using Game.Crafting.Data.Models;
using Game.Crafting.Models;

namespace Game.Crafting.Data.Services;

/// <summary>
/// Service for loading and accessing crafting recipe data from JSON configurations.
/// </summary>
public class CraftingDataService
{
    private readonly IDataLoader<RecipeDataSet> _dataLoader;
    private RecipeCollection? _cachedRecipes;

    public CraftingDataService(IDataLoader<RecipeDataSet> dataLoader)
    {
        _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
    }

    /// <summary>
    /// Gets all recipes asynchronously from JSON configuration.
    /// </summary>
    public async Task<IReadOnlyList<Recipe>> GetAllRecipesAsync(CancellationToken cancellationToken = default)
    {
        var recipes = await GetRecipeCollectionAsync(cancellationToken);
        return recipes.AllRecipes;
    }

    /// <summary>
    /// Gets starter recipes that should be available when the game begins.
    /// </summary>
    public async Task<IReadOnlyList<Recipe>> GetStarterRecipesAsync(CancellationToken cancellationToken = default)
    {
        var recipeData = await LoadRecipeDataAsync(cancellationToken);
        return recipeData.BasicRecipes.Select(r => r.ToRecipe()).ToList();
    }

    /// <summary>
    /// Gets advanced recipes that can be unlocked as the player progresses.
    /// </summary>
    public async Task<IReadOnlyList<Recipe>> GetAdvancedRecipesAsync(CancellationToken cancellationToken = default)
    {
        var recipeData = await LoadRecipeDataAsync(cancellationToken);
        return recipeData.AdvancedRecipes.Select(r => r.ToRecipe()).ToList();
    }

    /// <summary>
    /// Gets Phase 1 recipes that use only gatherable materials.
    /// </summary>
    public async Task<IReadOnlyList<Recipe>> GetPhase1RecipesAsync(CancellationToken cancellationToken = default)
    {
        var recipeData = await LoadRecipeDataAsync(cancellationToken);
        return recipeData.BasicRecipes.Select(r => r.ToRecipe()).ToList();
    }

    /// <summary>
    /// Gets a specific recipe by ID.
    /// </summary>
    public async Task<Recipe?> GetRecipeByIdAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return null;
        }

        var allRecipes = await GetAllRecipesAsync(cancellationToken);
        return allRecipes.FirstOrDefault(r => r.RecipeId == recipeId);
    }

    /// <summary>
    /// Gets recipes by category.
    /// </summary>
    public async Task<IReadOnlyList<Recipe>> GetRecipesByCategoryAsync(RecipeCategory category, CancellationToken cancellationToken = default)
    {
        var allRecipes = await GetAllRecipesAsync(cancellationToken);
        return allRecipes.Where(r => r.Category == category).ToList();
    }

    /// <summary>
    /// Loads the recipe collection with caching.
    /// </summary>
    private async Task<RecipeCollection> GetRecipeCollectionAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedRecipes != null)
        {
            return _cachedRecipes;
        }

        var recipeData = await LoadRecipeDataAsync(cancellationToken);
        _cachedRecipes = recipeData.ToRecipeCollection();
        return _cachedRecipes;
    }

    /// <summary>
    /// Loads recipe data with error handling.
    /// </summary>
    private async Task<RecipeDataSet> LoadRecipeDataAsync(CancellationToken cancellationToken = default)
    {
        var dataPath = DataPath.GetDomainJsonPath("recipes.json");
        
        try
        {
            // Load from file
            var loadResult = await _dataLoader.LoadAsync(dataPath, cancellationToken);
            if (!loadResult.IsSuccess || loadResult.Data == null)
            {
                GameLogger.Error($"[Crafting] Failed to load recipe data: {loadResult.ErrorMessage}");
                return GetFallbackRecipeData();
            }

            GameLogger.Info($"[Crafting] Loaded {loadResult.Data.BasicRecipes.Count + loadResult.Data.AdvancedRecipes.Count} recipes from JSON");
            return loadResult.Data;
        }
        catch (Exception ex)
        {
            GameLogger.Error($"[Crafting] Exception loading recipe data: {ex.Message}");
            return GetFallbackRecipeData();
        }
    }

    /// <summary>
    /// Provides fallback recipe data in case JSON loading fails.
    /// </summary>
    private static RecipeDataSet GetFallbackRecipeData()
    {
        GameLogger.Warning("[Crafting] Using fallback recipe data due to loading failure");
        
        // Return minimal fallback data
        return new RecipeDataSet
        {
            BasicRecipes = [],
            AdvancedRecipes = []
        };
    }

    /// <summary>
    /// Clears the internal cache, forcing fresh load on next request.
    /// </summary>
    public void ClearCache()
    {
        _cachedRecipes = null;
    }
}
