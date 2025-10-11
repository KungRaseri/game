#nullable enable

using Game.Core.CQS;

namespace Game.Progression.Commands;

/// <summary>
/// Command to record that materials were gathered.
/// </summary>
public record RecordMaterialsGatheredCommand : ICommand
{
    /// <summary>
    /// The item IDs of materials that were gathered.
    /// </summary>
    public required IEnumerable<string> MaterialItemIds { get; init; }
    
    /// <summary>
    /// Total quantity of materials gathered.
    /// </summary>
    public required int TotalQuantity { get; init; }
}
