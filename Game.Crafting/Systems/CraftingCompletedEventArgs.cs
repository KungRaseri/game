using Game.Crafting.Models;
using Game.Items.Models;

namespace Game.Crafting.Systems;

/// <summary>
/// Event arguments for crafting completion events.
/// </summary>
public class CraftingCompletedEventArgs : CraftingEventArgs
{
    public Item? CraftedItem { get; }
    public bool WasSuccessful { get; }

    public CraftingCompletedEventArgs(CraftingOrder order, Item? craftedItem, bool wasSuccessful)
        : base(order)
    {
        CraftedItem = craftedItem;
        WasSuccessful = wasSuccessful;
    }
}