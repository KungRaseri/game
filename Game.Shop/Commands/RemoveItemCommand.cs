using Game.Core.CQS;

namespace Game.Shop.Commands;

/// <summary>
/// Command to remove an item from a shop display slot.
/// </summary>
public record RemoveItemCommand : ICommand<string?>
{
    /// <summary>
    /// The display slot to clear.
    /// </summary>
    public required int SlotId { get; init; }
}
