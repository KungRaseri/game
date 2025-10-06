#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for unlocking recipes.
/// </summary>
public class UnlockRecipeCommandHandler : ICommandHandler<UnlockRecipeCommand>
{
    private readonly RecipeManager _recipeManager;

    public UnlockRecipeCommandHandler(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task HandleAsync(UnlockRecipeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = _recipeManager.UnlockRecipe(command.RecipeId);
            
            if (!success)
            {
                GameLogger.Warning($"Could not unlock recipe '{command.RecipeId}' - recipe not found or already unlocked");
            }
            else
            {
                GameLogger.Info($"Successfully unlocked recipe '{command.RecipeId}'");
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error unlocking recipe '{command.RecipeId}'");
            throw;
        }
    }
}
