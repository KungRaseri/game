using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Commands;

/// <summary>
/// Command to update the shop layout.
/// </summary>
public record UpdateShopLayoutCommand : ICommand
{
    /// <summary>
    /// The new layout configuration for the shop.
    /// </summary>
    public required ShopLayout NewLayout { get; init; }
}
