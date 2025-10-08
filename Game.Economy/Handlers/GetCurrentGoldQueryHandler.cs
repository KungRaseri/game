#nullable enable

using Game.Core.CQS;
using Game.Economy.Queries;
using Game.Economy.Systems;

namespace Game.Economy.Handlers;

/// <summary>
/// Handler for getting current treasury balance.
/// </summary>
public class GetCurrentGoldQueryHandler : IQueryHandler<GetCurrentGoldQuery, decimal>
{
    private readonly ITreasuryManager _treasuryManager;

    public GetCurrentGoldQueryHandler(ITreasuryManager treasuryManager)
    {
        _treasuryManager = treasuryManager ?? throw new ArgumentNullException(nameof(treasuryManager));
    }

    public Task<decimal> HandleAsync(GetCurrentGoldQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_treasuryManager.CurrentGold);
    }
}
