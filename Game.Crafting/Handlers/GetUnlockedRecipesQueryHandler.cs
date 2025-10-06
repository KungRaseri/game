#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for retrieving unlocked recipes.
/// </summary>
public class GetUnlockedRecipesQueryHandler : IQueryHandler<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>
{
    private readonly RecipeManager _recipeManager;

    public GetUnlockedRecipesQueryHandler(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task<IReadOnlyList<Recipe>> HandleAsync(GetUnlockedRecipesQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Recipe> recipes;

        if (query.Category.HasValue)
        {
            recipes = _recipeManager.GetUnlockedRecipesByCategory(query.Category.Value);
        }
        else
        {
            recipes = _recipeManager.UnlockedRecipes;
        }

        return Task.FromResult(recipes);
    }
}
