using Game.Core.CQS;
using Game.Items.Data;
using Game.Items.Models;

namespace Game.Items.Queries;

/// <summary>
/// Query to get available weapon configurations.
/// </summary>
public record GetWeaponConfigsQuery : IQuery<IEnumerable<WeaponConfig>>
{
    public string? FilterByName { get; init; }
    public QualityTier? MinQuality { get; init; }
}
