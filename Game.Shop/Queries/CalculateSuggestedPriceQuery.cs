using Game.Core.CQS;
using Game.Items.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to calculate suggested price for an item.
/// </summary>
public record CalculateSuggestedPriceQuery : IQuery<decimal>
{
    /// <summary>
    /// The item to calculate price for.
    /// </summary>
    public required Item Item { get; init; }

    /// <summary>
    /// The desired profit margin (default 0.5 = 50%).
    /// </summary>
    public float ProfitMargin { get; init; } = 0.5f;
}
