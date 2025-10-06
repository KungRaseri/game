#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to search for recipes by name or description.
/// </summary>
public record SearchRecipesQuery : IQuery<IReadOnlyList<Recipe>>
{
    /// <summary>
    /// Search term to filter by.
    /// </summary>
    public string SearchTerm { get; init; } = string.Empty;

    /// <summary>
    /// Whether to include locked recipes in results.
    /// </summary>
    public bool IncludeLockedRecipes { get; init; } = false;

    /// <summary>
    /// Optional category filter.
    /// </summary>
    public RecipeCategory? Category { get; init; } = null;
}
