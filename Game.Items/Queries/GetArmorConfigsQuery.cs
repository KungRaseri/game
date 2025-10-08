using Game.Core.CQS;
using Game.Items.Data;
using Game.Items.Models;

namespace Game.Items.Queries;

/// <summary>
/// Query to get available armor configurations.
/// </summary>
public record GetArmorConfigsQuery : IQuery<IEnumerable<ArmorConfig>>
{
    public string? FilterByName { get; init; }
    public QualityTier? MinQuality { get; init; }
}
