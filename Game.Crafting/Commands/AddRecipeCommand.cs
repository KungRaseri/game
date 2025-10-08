#nullable enable

using Game.Core.CQS;
using Game.Crafting.Models;

namespace Game.Crafting.Commands;

/// <summary>
/// Command to add a new recipe to the recipe manager.
/// </summary>
public record AddRecipeCommand : ICommand
{
    /// <summary>
    /// The recipe to add.
    /// </summary>
    public Recipe Recipe { get; init; } = null!;

    /// <summary>
    /// Whether the recipe should start unlocked.
    /// </summary>
    public bool StartUnlocked { get; init; } = false;
}
