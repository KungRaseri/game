#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;

namespace Game.Crafting.Queries;

/// <summary>
/// Query to get a specific recipe by ID.
/// </summary>
public record GetRecipeQuery : IQuery<Recipe?>
{
    /// <summary>
    /// The ID of the recipe to retrieve.
    /// </summary>
    public string RecipeId { get; init; } = string.Empty;
}
