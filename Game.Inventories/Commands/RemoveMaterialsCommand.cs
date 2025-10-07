#nullable enable

using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Inventories.Commands;

/// <summary>
/// Command to remove materials from the inventory.
/// </summary>
public record RemoveMaterialsCommand : ICommand<int>
{
    /// <summary>
    /// ID of the material to remove.
    /// </summary>
    public string MaterialId { get; init; } = string.Empty;

    /// <summary>
    /// Quality tier of the material to remove.
    /// </summary>
    public QualityTier Quality { get; init; }

    /// <summary>
    /// Quantity to remove.
    /// </summary>
    public int Quantity { get; init; }
}
