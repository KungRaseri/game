using Game.Crafting.Models;

namespace Game.Crafting.Systems;

/// <summary>
/// Event arguments for crafting-related events.
/// </summary>
public class CraftingEventArgs : EventArgs
{
    public CraftingOrder Order { get; }

    public CraftingEventArgs(CraftingOrder order)
    {
        Order = order ?? throw new ArgumentNullException(nameof(order));
    }
}