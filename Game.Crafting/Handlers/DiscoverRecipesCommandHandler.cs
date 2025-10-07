#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for discovering new recipes based on available materials.
/// </summary>
public class DiscoverRecipesCommandHandler : ICommandHandler<DiscoverRecipesCommand, int>
{
    private readonly IRecipeManager _recipeManager;

    public DiscoverRecipesCommandHandler(IRecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task<int> HandleAsync(DiscoverRecipesCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var discoveredRecipes = _recipeManager.DiscoverRecipes(command.AvailableMaterials);
            var count = discoveredRecipes.Count;
            
            GameLogger.Info($"Discovered {count} new recipes from available materials");
            return Task.FromResult(count);
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Error discovering recipes");
            throw;
        }
    }
}
