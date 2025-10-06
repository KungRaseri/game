#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for searching recipes.
/// </summary>
public class SearchRecipesQueryHandler : IQueryHandler<SearchRecipesQuery, IReadOnlyList<Recipe>>
{
    private readonly RecipeManager _recipeManager;

    public SearchRecipesQueryHandler(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task<IReadOnlyList<Recipe>> HandleAsync(SearchRecipesQuery query, CancellationToken cancellationToken = default)
    {
        var recipes = _recipeManager.SearchRecipes(query.SearchTerm, query.IncludeLockedRecipes);

        // Apply category filter if specified
        if (query.Category.HasValue)
        {
            recipes = recipes.Where(r => r.Category == query.Category.Value).ToList();
        }

        return Task.FromResult<IReadOnlyList<Recipe>>(recipes.ToList());
    }
}
