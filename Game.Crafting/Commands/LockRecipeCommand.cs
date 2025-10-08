#nullable enable

using Game.Core.CQS;

namespace Game.Crafting.Commands;

/// <summary>
/// Command to lock a recipe, making it unavailable for crafting.
/// </summary>
public record LockRecipeCommand : ICommand
{
    /// <summary>
    /// The ID of the recipe to lock.
    /// </summary>
    public string RecipeId { get; init; } = string.Empty;
}
