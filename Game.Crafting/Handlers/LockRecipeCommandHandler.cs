#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for locking recipes.
/// </summary>
public class LockRecipeCommandHandler : ICommandHandler<LockRecipeCommand>
{
    private readonly RecipeManager _recipeManager;

    public LockRecipeCommandHandler(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task HandleAsync(LockRecipeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = _recipeManager.LockRecipe(command.RecipeId);
            
            if (!success)
            {
                GameLogger.Warning($"Could not lock recipe '{command.RecipeId}' - recipe not found or already locked");
            }
            else
            {
                GameLogger.Info($"Successfully locked recipe '{command.RecipeId}'");
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error locking recipe '{command.RecipeId}'");
            throw;
        }
    }
}
