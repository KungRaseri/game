using Game.Core.CQS;
using Game.Items.Data;
using Game.Items.Models;

namespace Game.Items.Queries;

/// <summary>
/// Query to get available material configurations.
/// </summary>
public record GetMaterialConfigsQuery : IQuery<IEnumerable<MaterialConfig>>
{
    public string? FilterByName { get; init; }
    public QualityTier? MinQuality { get; init; }
}
