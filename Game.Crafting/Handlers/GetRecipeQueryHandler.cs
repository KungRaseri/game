#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for retrieving specific recipes.
/// </summary>
public class GetRecipeQueryHandler : IQueryHandler<GetRecipeQuery, Recipe?>
{
    private readonly RecipeManager _recipeManager;

    public GetRecipeQueryHandler(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task<Recipe?> HandleAsync(GetRecipeQuery query, CancellationToken cancellationToken = default)
    {
        var recipe = _recipeManager.GetRecipe(query.RecipeId);
        return Task.FromResult(recipe);
    }
}
