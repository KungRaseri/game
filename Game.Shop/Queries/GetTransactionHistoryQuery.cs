using Game.Core.CQS;
using Game.Shop.Models;

namespace Game.Shop.Queries;

/// <summary>
/// Query to get transaction history.
/// </summary>
public record GetTransactionHistoryQuery : IQuery<IReadOnlyList<SaleTransaction>>
{
    /// <summary>
    /// Optional date range to filter transactions.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Optional date range to filter transactions.
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Filter by customer ID.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    /// Limit the number of results.
    /// </summary>
    public int? Limit { get; init; }
}
