using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Items.Commands;

/// <summary>
/// Command to create armor with specified configuration and quality.
/// </summary>
public record CreateArmorCommand : ICommand<Armor>
{
    public required string ArmorConfigId { get; init; }
    public QualityTier Quality { get; init; } = QualityTier.Common;
}
