#nullable enable

using Game.Core.CQS;

namespace Game.Progression.Commands;

/// <summary>
/// Command to record that an item was crafted.
/// </summary>
public record RecordItemCraftedCommand : ICommand
{
    /// <summary>
    /// Optional: The item ID that was crafted (for future analytics).
    /// </summary>
    public string? ItemId { get; init; }
    
    /// <summary>
    /// Optional: The quantity crafted (defaults to 1).
    /// </summary>
    public int Quantity { get; init; } = 1;
}
