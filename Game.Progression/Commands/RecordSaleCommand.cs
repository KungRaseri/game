#nullable enable

using Game.Core.CQS;

namespace Game.Progression.Commands;

/// <summary>
/// Command to record that a sale was completed.
/// </summary>
public record RecordSaleCommand : ICommand
{
    /// <summary>
    /// Amount of gold earned from the sale.
    /// </summary>
    public required decimal GoldEarned { get; init; }
    
    /// <summary>
    /// Optional: The item ID that was sold (for future analytics).
    /// </summary>
    public string? ItemId { get; init; }
}
