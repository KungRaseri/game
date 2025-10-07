#nullable enable

using Game.Core.CQS;

namespace Game.Economy.Commands;

/// <summary>
/// Command to add revenue to the treasury from various sources.
/// </summary>
public record AddRevenueCommand : ICommand
{
    /// <summary>
    /// Amount of revenue to add. Must be positive.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Source of the revenue (e.g., "Sales", "Investment Returns", etc.).
    /// </summary>
    public string Source { get; init; } = "Sales";
}
