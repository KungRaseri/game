#nullable enable

using Game.Core.CQS;

namespace Game.Crafting.Commands;

/// <summary>
/// Command to unlock a recipe for crafting.
/// </summary>
public record UnlockRecipeCommand : ICommand
{
    /// <summary>
    /// The ID of the recipe to unlock.
    /// </summary>
    public string RecipeId { get; init; } = string.Empty;
}
