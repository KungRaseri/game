using Game.Crafting.Data.Services;
using Game.Crafting.Systems;
using Game.Core.Utils;

namespace Game.Crafting.Data.Services;

/// <summary>
/// Service for initializing RecipeManager with data from JSON configurations.
/// Replaces the static initialization methods with dependency-injected services.
/// </summary>
public class RecipeInitializationService
{
    private readonly CraftingDataService _craftingDataService;

    public RecipeInitializationService(CraftingDataService craftingDataService)
    {
        _craftingDataService = craftingDataService ?? throw new ArgumentNullException(nameof(craftingDataService));
    }

    /// <summary>
    /// Initializes a recipe manager with all recipes from JSON configuration.
    /// This replaces the static StarterRecipes.InitializeRecipeManager method.
    /// </summary>
    /// <param name="recipeManager">The recipe manager to populate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task InitializeRecipeManagerAsync(RecipeManager recipeManager, CancellationToken cancellationToken = default)
    {
        if (recipeManager == null)
        {
            throw new ArgumentNullException(nameof(recipeManager));
        }

        try
        {
            // Load basic recipes (unlocked by default)
            var basicRecipes = await _craftingDataService.GetBasicRecipesAsync(cancellationToken);
            foreach (var recipe in basicRecipes)
            {
                recipeManager.AddRecipe(recipe, unlocked: true);
            }

            // Load advanced recipes (locked by default)
            var advancedRecipes = await _craftingDataService.GetAdvancedRecipesAsync(cancellationToken);
            foreach (var recipe in advancedRecipes)
            {
                recipeManager.AddRecipe(recipe, unlocked: false);
            }

            GameLogger.Info($"[Crafting] Initialized RecipeManager with {recipeManager.AllRecipes.Count} recipes from JSON");
        }
        catch (Exception ex)
        {
            GameLogger.Error($"[Crafting] Failed to initialize RecipeManager: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Initializes a recipe manager synchronously by waiting for the async operation.
    /// Use this only when async is not available.
    /// </summary>
    /// <param name="recipeManager">The recipe manager to populate</param>
    public void InitializeRecipeManager(RecipeManager recipeManager)
    {
        try
        {
            InitializeRecipeManagerAsync(recipeManager).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            GameLogger.Error($"[Crafting] Synchronous recipe manager initialization failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates and initializes a new RecipeManager with JSON data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Initialized RecipeManager</returns>
    public async Task<RecipeManager> CreateInitializedRecipeManagerAsync(CancellationToken cancellationToken = default)
    {
        var recipeManager = new RecipeManager();
        await InitializeRecipeManagerAsync(recipeManager, cancellationToken);
        return recipeManager;
    }

    /// <summary>
    /// Creates and initializes a new RecipeManager with JSON data synchronously.
    /// </summary>
    /// <returns>Initialized RecipeManager</returns>
    public RecipeManager CreateInitializedRecipeManager()
    {
        var recipeManager = new RecipeManager();
        InitializeRecipeManager(recipeManager);
        return recipeManager;
    }
}
