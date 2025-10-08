#nullable enable

using Game.Core.CQS;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for getting expense history with optional filtering.
/// </summary>
public class GetExpenseHistoryQueryHandler : IQueryHandler<GetExpenseHistoryQuery, IReadOnlyList<ShopExpense>>
{
    private readonly ITreasuryManager _treasuryManager;

    public GetExpenseHistoryQueryHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<IReadOnlyList<ShopExpense>> HandleAsync(GetExpenseHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var expenses = _treasuryManager.GetExpenseHistory(
            query.ExpenseType,
            query.StartDate,
            query.EndDate,
            query.RecurringOnly
        );

        return Task.FromResult(expenses);
    }
}
