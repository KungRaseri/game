#nullable enable

using Game.Core.CQS;

namespace Game.Crafting.Commands;

/// <summary>
/// Command to cancel all pending crafting orders.
/// </summary>
public record CancelAllCraftingOrdersCommand : ICommand
{
}
