#nullable enable

using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Inventories.Queries;

/// <summary>
/// Query to check if materials can be consumed (availability check).
/// </summary>
public record CanConsumeMaterialsQuery : IQuery<bool>
{
    /// <summary>
    /// Dictionary of materials to check.
    /// Key: (MaterialId, Quality), Value: Quantity required
    /// </summary>
    public Dictionary<(string MaterialId, QualityTier Quality), int> Requirements { get; init; } = new();
}
