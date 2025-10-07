using Game.Crafting.Models;

namespace Game.Crafting.Systems;

/// <summary>
/// Event arguments for recipe-related events.
/// </summary>
public class RecipeEventArgs : EventArgs
{
    public Recipe Recipe { get; }

    public RecipeEventArgs(Recipe recipe)
    {
        Recipe = recipe ?? throw new ArgumentNullException(nameof(recipe));
    }
}