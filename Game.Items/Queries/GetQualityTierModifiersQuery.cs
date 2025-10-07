using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Items.Queries;

/// <summary>
/// Query to get quality tier modifiers for weapons and armor.
/// </summary>
public record GetQualityTierModifiersQuery : IQuery<QualityTierModifierResult>
{
    public QualityTier Quality { get; init; }
    public string ModifierType { get; init; } = "all"; // "weapon", "armor", "value", or "all"
}
