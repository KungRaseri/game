#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to get all unlocked recipes.
/// </summary>
public record GetUnlockedRecipesQuery : IQuery<IReadOnlyList<Recipe>>
{
    /// <summary>
    /// Optional category filter.
    /// </summary>
    public RecipeCategory? Category { get; init; } = null;
}
