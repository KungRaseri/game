using Game.Core.CQS;
using Game.Items.Models;
using Game.Shop.Models;

namespace Game.Shop.Commands;

/// <summary>
/// Command to set pricing strategy for specific item types.
/// </summary>
public record SetPricingStrategyCommand : ICommand
{
    /// <summary>
    /// The type of item to apply the strategy to.
    /// </summary>
    public required ItemType ItemType { get; init; }

    /// <summary>
    /// The pricing strategy to apply.
    /// </summary>
    public required PricingStrategy Strategy { get; init; }
}
