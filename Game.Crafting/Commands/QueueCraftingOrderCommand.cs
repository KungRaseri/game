#nullable enable

using Game.Core.CQS;
using Game.Items.Models.Materials;

namespace Game.Crafting.Commands;

/// <summary>
/// Command to queue a new crafting order for processing.
/// </summary>
public record QueueCraftingOrderCommand : ICommand<string>
{
    /// <summary>
    /// The ID of the recipe to craft.
    /// </summary>
    public string RecipeId { get; init; } = string.Empty;

    /// <summary>
    /// Materials to use for crafting, keyed by material ID.
    /// </summary>
    public IReadOnlyDictionary<string, Material> Materials { get; init; } = new Dictionary<string, Material>();

    /// <summary>
    /// Optional priority for the crafting order (higher numbers = higher priority).
    /// </summary>
    public int Priority { get; init; } = 0;
}
