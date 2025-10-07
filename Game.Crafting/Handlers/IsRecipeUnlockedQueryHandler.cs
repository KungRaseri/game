#nullable enable

using Game.Core.CQS;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for checking if a recipe is unlocked.
/// </summary>
public class IsRecipeUnlockedQueryHandler : IQueryHandler<IsRecipeUnlockedQuery, bool>
{
    private readonly IRecipeManager _recipeManager;

    public IsRecipeUnlockedQueryHandler(IRecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task<bool> HandleAsync(IsRecipeUnlockedQuery query, CancellationToken cancellationToken = default)
    {
        var isUnlocked = _recipeManager.IsRecipeUnlocked(query.RecipeId);
        return Task.FromResult(isUnlocked);
    }
}
