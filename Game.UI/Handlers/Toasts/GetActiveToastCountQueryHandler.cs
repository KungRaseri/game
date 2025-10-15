using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries.Toasts;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles getting the current number of active toasts.
/// </summary>
public class GetActiveToastCountQueryHandler : IQueryHandler<GetActiveToastCountQuery, int>
{
    private readonly IToastOperations _toastOperations;

    public GetActiveToastCountQueryHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public Task<int> HandleAsync(GetActiveToastCountQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastOperations.GetActiveToastCount());
    }
}
