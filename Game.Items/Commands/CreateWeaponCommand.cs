using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Items.Commands;

/// <summary>
/// Command to create a weapon with specified configuration and quality.
/// </summary>
public record CreateWeaponCommand : ICommand<Weapon>
{
    public required string WeaponConfigId { get; init; }
    public QualityTier Quality { get; init; } = QualityTier.Common;
}
