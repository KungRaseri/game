#nullable enable

using Game.Core.CQS;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to check if a recipe is unlocked.
/// </summary>
public record IsRecipeUnlockedQuery : IQuery<bool>
{
    /// <summary>
    /// The ID of the recipe to check.
    /// </summary>
    public string RecipeId { get; init; } = string.Empty;
}
