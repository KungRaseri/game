#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Systems;

namespace Game.Crafting.Handlers;

/// <summary>
/// Handler for adding new recipes to the recipe manager.
/// </summary>
public class AddRecipeCommandHandler : ICommandHandler<AddRecipeCommand>
{
    private readonly RecipeManager _recipeManager;

    public AddRecipeCommandHandler(RecipeManager recipeManager)
    {
        _recipeManager = recipeManager ?? throw new ArgumentNullException(nameof(recipeManager));
    }

    public Task HandleAsync(AddRecipeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _recipeManager.AddRecipe(command.Recipe, command.StartUnlocked);
            GameLogger.Info($"Successfully added recipe '{command.Recipe.Name}' (ID: {command.Recipe.RecipeId})");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error adding recipe '{command.Recipe?.Name}' (ID: {command.Recipe?.RecipeId})");
            throw;
        }
    }
}
