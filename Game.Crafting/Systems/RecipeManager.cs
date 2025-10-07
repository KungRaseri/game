using Game.Core.Utils;
using Game.Items.Models.Materials;
using Game.Crafting.Models;

namespace Game.Crafting.Systems;

/// <summary>
/// Manages recipes including discovery, unlocking, and storage.
/// </summary>
public class RecipeManager : IRecipeManager
{
    private readonly Dictionary<string, Recipe> _allRecipes;
    private readonly HashSet<string> _unlockedRecipes;
    private readonly Dictionary<RecipeCategory, HashSet<string>> _categoryIndex;

    /// <summary>
    /// Event raised when a new recipe is unlocked.
    /// </summary>
    public event EventHandler<RecipeEventArgs>? RecipeUnlocked;

    /// <summary>
    /// Event raised when a recipe is locked/removed.
    /// </summary>
    public event EventHandler<RecipeEventArgs>? RecipeLocked;

    /// <summary>
    /// Gets all recipes that are currently unlocked and available for crafting.
    /// </summary>
    public IReadOnlyList<Recipe> UnlockedRecipes => 
        _unlockedRecipes.Select(id => _allRecipes[id]).ToList();

    /// <summary>
    /// Gets all recipes in the system, regardless of unlock status.
    /// </summary>
    public IReadOnlyList<Recipe> AllRecipes => _allRecipes.Values.ToList();

    /// <summary>
    /// Gets all available recipe categories.
    /// </summary>
    public IReadOnlyList<RecipeCategory> Categories => _categoryIndex.Keys.ToList();

    public RecipeManager()
    {
        _allRecipes = new Dictionary<string, Recipe>();
        _unlockedRecipes = new HashSet<string>();
        _categoryIndex = new Dictionary<RecipeCategory, HashSet<string>>();
    }

    /// <summary>
    /// Adds a recipe to the manager. The recipe starts locked unless specified otherwise.
    /// </summary>
    /// <param name="recipe">The recipe to add</param>
    /// <param name="unlocked">Whether the recipe should start unlocked</param>
    public void AddRecipe(Recipe recipe, bool unlocked = false)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        if (_allRecipes.ContainsKey(recipe.RecipeId))
        {
            GameLogger.Warning($"Recipe with ID '{recipe.RecipeId}' already exists. Overwriting.");
        }

        _allRecipes[recipe.RecipeId] = recipe;

        // Update category index
        if (!_categoryIndex.ContainsKey(recipe.Category))
        {
            _categoryIndex[recipe.Category] = new HashSet<string>();
        }
        _categoryIndex[recipe.Category].Add(recipe.RecipeId);

        if (unlocked)
        {
            UnlockRecipe(recipe.RecipeId);
        }

        GameLogger.Info($"Added recipe: {recipe.Name} [{recipe.Category}]");
    }

    /// <summary>
    /// Removes a recipe from the manager.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to remove</param>
    /// <returns>True if the recipe was removed, false if it didn't exist</returns>
    public bool RemoveRecipe(string recipeId)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return false;
        }

        if (!_allRecipes.TryGetValue(recipeId, out var recipe))
        {
            return false;
        }

        // Remove from unlocked set
        _unlockedRecipes.Remove(recipeId);

        // Remove from category index
        if (_categoryIndex.TryGetValue(recipe.Category, out var categorySet))
        {
            categorySet.Remove(recipeId);
            if (categorySet.Count == 0)
            {
                _categoryIndex.Remove(recipe.Category);
            }
        }

        // Remove from main collection
        _allRecipes.Remove(recipeId);

        GameLogger.Info($"Removed recipe: {recipe.Name}");
        return true;
    }

    /// <summary>
    /// Unlocks a recipe, making it available for crafting.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to unlock</param>
    /// <returns>True if the recipe was unlocked, false if it didn't exist or was already unlocked</returns>
    public bool UnlockRecipe(string recipeId)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return false;
        }

        if (!_allRecipes.TryGetValue(recipeId, out var recipe))
        {
            GameLogger.Warning($"Attempted to unlock non-existent recipe: {recipeId}");
            return false;
        }

        if (_unlockedRecipes.Contains(recipeId))
        {
            return false; // Already unlocked
        }

        _unlockedRecipes.Add(recipeId);
        recipe.Unlock();

        GameLogger.Info($"Unlocked recipe: {recipe.Name}");
        RecipeUnlocked?.Invoke(this, new RecipeEventArgs(recipe));
        return true;
    }

    /// <summary>
    /// Locks a recipe, making it unavailable for crafting.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to lock</param>
    /// <returns>True if the recipe was locked, false if it didn't exist or was already locked</returns>
    public bool LockRecipe(string recipeId)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return false;
        }

        if (!_allRecipes.TryGetValue(recipeId, out var recipe))
        {
            return false;
        }

        if (!_unlockedRecipes.Contains(recipeId))
        {
            return false; // Already locked
        }

        _unlockedRecipes.Remove(recipeId);
        recipe.Lock();

        GameLogger.Info($"Locked recipe: {recipe.Name}");
        RecipeLocked?.Invoke(this, new RecipeEventArgs(recipe));
        return true;
    }

    /// <summary>
    /// Gets a recipe by its ID.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to retrieve</param>
    /// <returns>The recipe, or null if not found</returns>
    public Recipe? GetRecipe(string recipeId)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return null;
        }

        return _allRecipes.TryGetValue(recipeId, out var recipe) ? recipe : null;
    }

    /// <summary>
    /// Checks if a recipe is unlocked.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to check</param>
    /// <returns>True if the recipe exists and is unlocked</returns>
    public bool IsRecipeUnlocked(string recipeId)
    {
        return !string.IsNullOrWhiteSpace(recipeId) && _unlockedRecipes.Contains(recipeId);
    }

    /// <summary>
    /// Gets all unlocked recipes in a specific category.
    /// </summary>
    /// <param name="category">The category to filter by</param>
    /// <returns>List of unlocked recipes in the specified category</returns>
    public IReadOnlyList<Recipe> GetUnlockedRecipesByCategory(RecipeCategory category)
    {
        if (!_categoryIndex.TryGetValue(category, out var categoryRecipes))
        {
            return new List<Recipe>();
        }

        return categoryRecipes
            .Where(id => _unlockedRecipes.Contains(id))
            .Select(id => _allRecipes[id])
            .ToList();
    }

    /// <summary>
    /// Searches for recipes by name or description.
    /// </summary>
    /// <param name="searchTerm">The term to search for</param>
    /// <param name="includeLockedRecipes">Whether to include locked recipes in results</param>
    /// <returns>List of matching recipes</returns>
    public IReadOnlyList<Recipe> SearchRecipes(string searchTerm, bool includeLockedRecipes = false)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return includeLockedRecipes ? AllRecipes : UnlockedRecipes;
        }

        var term = searchTerm.ToLowerInvariant();
        var candidates = includeLockedRecipes ? _allRecipes.Values.ToList() : UnlockedRecipes;

        return candidates
            .Where(recipe => 
                recipe.Name.ToLowerInvariant().Contains(term) ||
                recipe.Description.ToLowerInvariant().Contains(term) ||
                recipe.Category.ToString().ToLowerInvariant().Contains(term))
            .ToList();
    }

    /// <summary>
    /// Discovers new recipes based on available materials.
    /// This checks for recipes whose material requirements can be satisfied.
    /// </summary>
    /// <param name="availableMaterials">Materials available for crafting</param>
    /// <returns>List of newly discovered recipes</returns>
    public IReadOnlyList<Recipe> DiscoverRecipes(IEnumerable<Material> availableMaterials)
    {
        if (availableMaterials == null)
        {
            return new List<Recipe>();
        }

        var materials = availableMaterials.ToList();
        var newlyDiscovered = new List<Recipe>();

        foreach (var recipe in _allRecipes.Values)
        {
            // Skip already unlocked recipes
            if (_unlockedRecipes.Contains(recipe.RecipeId))
            {
                continue;
            }

            // Check if prerequisites are met
            if (!ArePrerequisitesMet(recipe))
            {
                continue;
            }

            // Check if we have materials that satisfy the requirements
            if (CanSatisfyRequirements(recipe, materials))
            {
                if (UnlockRecipe(recipe.RecipeId))
                {
                    newlyDiscovered.Add(recipe);
                    GameLogger.Info($"Discovered new recipe: {recipe.Name}");
                }
            }
        }

        return newlyDiscovered;
    }

    /// <summary>
    /// Checks if the prerequisites for a recipe are met.
    /// </summary>
    /// <param name="recipe">The recipe to check</param>
    /// <returns>True if all prerequisites are satisfied</returns>
    private bool ArePrerequisitesMet(Recipe recipe)
    {
        return recipe.Prerequisites.All(prerequisite => _unlockedRecipes.Contains(prerequisite));
    }

    /// <summary>
    /// Checks if the available materials can satisfy the recipe requirements.
    /// </summary>
    /// <param name="recipe">The recipe to check</param>
    /// <param name="materials">Available materials</param>
    /// <returns>True if requirements can be satisfied</returns>
    private bool CanSatisfyRequirements(Recipe recipe, IEnumerable<Material> materials)
    {
        var materialList = materials.ToList();

        foreach (var requirement in recipe.MaterialRequirements)
        {
            var satisfyingMaterials = materialList
                .Where(requirement.IsSatisfiedBy)
                .Count();

            if (satisfyingMaterials < requirement.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets statistics about the recipe collection.
    /// </summary>
    /// <returns>Dictionary with various statistics</returns>
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            ["TotalRecipes"] = _allRecipes.Count,
            ["UnlockedRecipes"] = _unlockedRecipes.Count,
            ["LockedRecipes"] = _allRecipes.Count - _unlockedRecipes.Count,
            ["Categories"] = _categoryIndex.Count,
            ["RecipesByCategory"] = _categoryIndex.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Count)
        };
    }

    /// <summary>
    /// Clears all recipes and resets the manager to initial state.
    /// </summary>
    public void Clear()
    {
        _allRecipes.Clear();
        _unlockedRecipes.Clear();
        _categoryIndex.Clear();
        GameLogger.Info("Recipe manager cleared");
    }
}
