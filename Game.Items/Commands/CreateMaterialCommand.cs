using Game.Core.CQS;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Commands;

/// <summary>
/// Command to create a material with specified configuration and quality.
/// </summary>
public record CreateMaterialCommand : ICommand<Material>
{
    public required string MaterialConfigId { get; init; }
    public QualityTier Quality { get; init; } = QualityTier.Common;
}
