#nullable enable

using Game.Core.CQS;
using Game.Economy.Queries;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for getting monthly budget for a specific expense type.
/// </summary>
public class GetMonthlyBudgetQueryHandler : IQueryHandler<GetMonthlyBudgetQuery, decimal?>
{
    private readonly ITreasuryManager _treasuryManager;

    public GetMonthlyBudgetQueryHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<decimal?> HandleAsync(GetMonthlyBudgetQuery query, CancellationToken cancellationToken = default)
    {
        var budget = _treasuryManager.GetMonthlyBudget(query.ExpenseType);
        return Task.FromResult(budget);
    }
}
