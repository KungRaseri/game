#nullable enable

using Game.Core.CQS;
using Game.Crafting.Commands;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Items.Models.Materials;

namespace Game.Crafting.Systems;

/// <summary>
/// High-level service that provides a simplified interface to the crafting system using CQS patterns.
/// This service acts as a facade over the underlying CQS commands and queries.
/// </summary>
public class CraftingService
{
    private readonly IDispatcher _dispatcher;

    public CraftingService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    #region Crafting Orders

    /// <summary>
    /// Queues a new crafting order.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to craft</param>
    /// <param name="materials">Materials to use for crafting</param>
    /// <param name="priority">Optional priority for the order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the created order</returns>
    public async Task<string> QueueCraftingOrderAsync(
        string recipeId,
        IReadOnlyDictionary<string, Material> materials,
        int priority = 0,
        CancellationToken cancellationToken = default)
    {
        var command = new QueueCraftingOrderCommand
        {
            RecipeId = recipeId,
            Materials = materials,
            Priority = priority
        };

        return await _dispatcher.DispatchCommandAsync<QueueCraftingOrderCommand, string>(command, cancellationToken);
    }

    /// <summary>
    /// Cancels a specific crafting order.
    /// </summary>
    /// <param name="orderId">The ID of the order to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CancelCraftingOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var command = new CancelCraftingOrderCommand { OrderId = orderId };
        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Cancels all crafting orders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CancelAllCraftingOrdersAsync(CancellationToken cancellationToken = default)
    {
        var command = new CancelAllCraftingOrdersCommand();
        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Gets a specific crafting order by ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The crafting order or null if not found</returns>
    public async Task<CraftingOrder?> GetCraftingOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var query = new GetCraftingOrderQuery { OrderId = orderId };
        return await _dispatcher.DispatchQueryAsync<GetCraftingOrderQuery, CraftingOrder?>(query, cancellationToken);
    }

    /// <summary>
    /// Gets all crafting orders (current and queued).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing current and queued orders</returns>
    public async Task<CraftingOrdersResult> GetAllCraftingOrdersAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetAllCraftingOrdersQuery();
        return await _dispatcher.DispatchQueryAsync<GetAllCraftingOrdersQuery, CraftingOrdersResult>(query, cancellationToken);
    }

    /// <summary>
    /// Gets crafting station statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary containing various statistics</returns>
    public async Task<Dictionary<string, object>> GetCraftingStationStatsAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetCraftingStationStatsQuery();
        return await _dispatcher.DispatchQueryAsync<GetCraftingStationStatsQuery, Dictionary<string, object>>(query, cancellationToken);
    }

    #endregion

    #region Recipe Management

    /// <summary>
    /// Adds a new recipe to the recipe manager.
    /// </summary>
    /// <param name="recipe">The recipe to add</param>
    /// <param name="startUnlocked">Whether the recipe should start unlocked</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task AddRecipeAsync(Recipe recipe, bool startUnlocked = false, CancellationToken cancellationToken = default)
    {
        var command = new AddRecipeCommand
        {
            Recipe = recipe,
            StartUnlocked = startUnlocked
        };

        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Unlocks a recipe for crafting.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to unlock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task UnlockRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        var command = new UnlockRecipeCommand { RecipeId = recipeId };
        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Locks a recipe, making it unavailable for crafting.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to lock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task LockRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        var command = new LockRecipeCommand { RecipeId = recipeId };
        await _dispatcher.DispatchCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Discovers new recipes based on available materials.
    /// </summary>
    /// <param name="availableMaterials">Materials available for recipe discovery</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of recipes discovered</returns>
    public async Task<int> DiscoverRecipesAsync(IEnumerable<Material> availableMaterials, CancellationToken cancellationToken = default)
    {
        var command = new DiscoverRecipesCommand { AvailableMaterials = availableMaterials };
        return await _dispatcher.DispatchCommandAsync<DiscoverRecipesCommand, int>(command, cancellationToken);
    }

    /// <summary>
    /// Gets a specific recipe by ID.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The recipe or null if not found</returns>
    public async Task<Recipe?> GetRecipeAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        var query = new GetRecipeQuery { RecipeId = recipeId };
        return await _dispatcher.DispatchQueryAsync<GetRecipeQuery, Recipe?>(query, cancellationToken);
    }

    /// <summary>
    /// Gets all unlocked recipes.
    /// </summary>
    /// <param name="category">Optional category filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of unlocked recipes</returns>
    public async Task<IReadOnlyList<Recipe>> GetUnlockedRecipesAsync(RecipeCategory? category = null, CancellationToken cancellationToken = default)
    {
        var query = new GetUnlockedRecipesQuery { Category = category };
        return await _dispatcher.DispatchQueryAsync<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>(query, cancellationToken);
    }

    /// <summary>
    /// Searches for recipes by name or description.
    /// </summary>
    /// <param name="searchTerm">Term to search for</param>
    /// <param name="includeLockedRecipes">Whether to include locked recipes</param>
    /// <param name="category">Optional category filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching recipes</returns>
    public async Task<IReadOnlyList<Recipe>> SearchRecipesAsync(
        string searchTerm,
        bool includeLockedRecipes = false,
        RecipeCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchRecipesQuery
        {
            SearchTerm = searchTerm,
            IncludeLockedRecipes = includeLockedRecipes,
            Category = category
        };

        return await _dispatcher.DispatchQueryAsync<SearchRecipesQuery, IReadOnlyList<Recipe>>(query, cancellationToken);
    }

    /// <summary>
    /// Checks if a recipe is unlocked.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the recipe is unlocked</returns>
    public async Task<bool> IsRecipeUnlockedAsync(string recipeId, CancellationToken cancellationToken = default)
    {
        var query = new IsRecipeUnlockedQuery { RecipeId = recipeId };
        return await _dispatcher.DispatchQueryAsync<IsRecipeUnlockedQuery, bool>(query, cancellationToken);
    }

    /// <summary>
    /// Gets recipe manager statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary containing various statistics</returns>
    public async Task<Dictionary<string, object>> GetRecipeManagerStatsAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetRecipeManagerStatsQuery();
        return await _dispatcher.DispatchQueryAsync<GetRecipeManagerStatsQuery, Dictionary<string, object>>(query, cancellationToken);
    }

    #endregion
}
