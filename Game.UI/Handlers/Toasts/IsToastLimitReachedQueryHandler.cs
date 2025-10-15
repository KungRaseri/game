using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries.Toasts;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles checking if the toast limit has been reached.
/// </summary>
public class IsToastLimitReachedQueryHandler : IQueryHandler<IsToastLimitReachedQuery, bool>
{
    private readonly IToastOperations _toastOperations;

    public IsToastLimitReachedQueryHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public Task<bool> HandleAsync(IsToastLimitReachedQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastOperations.IsToastLimitReached());
    }
}
