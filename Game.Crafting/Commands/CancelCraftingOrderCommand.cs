#nullable enable

using Game.Core.CQS;

namespace Game.Crafting.Commands;

/// <summary>
/// Command to cancel a specific crafting order.
/// </summary>
public record CancelCraftingOrderCommand : ICommand
{
    /// <summary>
    /// The ID of the order to cancel.
    /// </summary>
    public string OrderId { get; init; } = string.Empty;
}
