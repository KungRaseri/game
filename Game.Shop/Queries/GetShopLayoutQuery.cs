using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get current shop layout.
/// </summary>
public record GetShopLayoutQuery : IQuery<ShopLayout>
{
    // No parameters needed - gets current shop layout
}
