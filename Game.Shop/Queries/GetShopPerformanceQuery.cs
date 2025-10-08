using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get shop performance metrics.
/// </summary>
public record GetShopPerformanceQuery : IQuery<ShopPerformanceMetrics>
{
    /// <summary>
    /// Optional date range to analyze (default is all time).
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Optional date range to analyze (default is all time).
    /// </summary>
    public DateTime? EndDate { get; init; }
}
