#nullable enable

using Game.Core.CQS;

namespace Game.Economy.Commands;

/// <summary>
/// Command to make an investment in shop improvements.
/// </summary>
public record MakeInvestmentCommand : ICommand<bool>
{
    /// <summary>
    /// ID of the investment opportunity to invest in.
    /// </summary>
    public string InvestmentId { get; init; } = string.Empty;
}
