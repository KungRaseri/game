using Game.Core.CQS;

namespace Game.Shop.Commands;

/// <summary>
/// Command to make an investment.
/// </summary>
public record MakeInvestmentCommand : ICommand<bool>
{
    /// <summary>
    /// The unique identifier of the investment to make.
    /// </summary>
    public required string InvestmentId { get; init; }
}
