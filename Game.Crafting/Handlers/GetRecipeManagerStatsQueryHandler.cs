#nullable enable

using Game.Core.CQS;
using Game.Crafting.Queries;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for retrieving recipe manager statistics.
/// </summary>
public class GetRecipeManagerStatsQueryHandler : IQueryHandler<GetRecipeManagerStatsQuery, Dictionary<string, object>>
{
    private readonly RecipeManager _recipeManager;

    public GetRecipeManagerStatsQueryHandler(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task<Dictionary<string, object>> HandleAsync(GetRecipeManagerStatsQuery query, CancellationToken cancellationToken = default)
    {
        var stats = _recipeManager.GetStatistics();
        return Task.FromResult(stats);
    }
}
