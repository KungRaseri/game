#nullable enable

using Game.Items.Models.Materials;
using Game.Crafting.Models;

namespace Game.Crafting.Systems;

/// <summary>
/// Interface for the recipe manager that handles recipe storage, unlocking, and discovery.
/// </summary>
public interface IRecipeManager
{
    /// <summary>
    /// Event raised when a recipe is unlocked.
    /// </summary>
    event EventHandler<RecipeEventArgs>? RecipeUnlocked;

    /// <summary>
    /// Gets all unlocked recipes.
    /// </summary>
    IReadOnlyList<Recipe> UnlockedRecipes { get; }

    /// <summary>
    /// Gets all recipes in the manager.
    /// </summary>
    IReadOnlyList<Recipe> AllRecipes { get; }

    /// <summary>
    /// Adds a recipe to the manager.
    /// </summary>
    /// <param name="recipe">The recipe to add</param>
    /// <param name="startUnlocked">Whether the recipe should start unlocked</param>
    void AddRecipe(Recipe recipe, bool startUnlocked = false);

    /// <summary>
    /// Gets a recipe by its ID.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe</param>
    /// <returns>The recipe if found, null otherwise</returns>
    Recipe? GetRecipe(string recipeId);

    /// <summary>
    /// Gets unlocked recipes by category.
    /// </summary>
    /// <param name="category">The category to filter by</param>
    /// <returns>Unlocked recipes in the specified category</returns>
    IReadOnlyList<Recipe> GetUnlockedRecipesByCategory(RecipeCategory category);

    /// <summary>
    /// Checks if a recipe is unlocked.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to check</param>
    /// <returns>True if the recipe is unlocked</returns>
    bool IsRecipeUnlocked(string recipeId);

    /// <summary>
    /// Unlocks a recipe by its ID.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to unlock</param>
    /// <returns>True if the recipe was found and unlocked</returns>
    bool UnlockRecipe(string recipeId);

    /// <summary>
    /// Locks a recipe by its ID.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to lock</param>
    /// <returns>True if the recipe was found and locked</returns>
    bool LockRecipe(string recipeId);

    /// <summary>
    /// Searches for recipes by name or description.
    /// </summary>
    /// <param name="searchTerm">The term to search for</param>
    /// <param name="unlockedOnly">Whether to search only unlocked recipes</param>
    /// <returns>Matching recipes</returns>
    IReadOnlyList<Recipe> SearchRecipes(string searchTerm, bool unlockedOnly = true);

    /// <summary>
    /// Discovers recipes based on available materials.
    /// </summary>
    /// <param name="availableMaterials">The materials available for discovery</param>
    /// <returns>List of newly discovered recipes</returns>
    IReadOnlyList<Recipe> DiscoverRecipes(IEnumerable<Material> availableMaterials);

    /// <summary>
    /// Gets recipe manager statistics.
    /// </summary>
    /// <returns>Statistics about the recipe manager</returns>
    Dictionary<string, object> GetStatistics();
}
