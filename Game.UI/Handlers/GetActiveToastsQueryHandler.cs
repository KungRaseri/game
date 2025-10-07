#nullable enable

using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries;

namespace Game.UI.Handlers;

/// <summary>
/// Handles getting all currently active toasts.
/// </summary>
public class GetActiveToastsQueryHandler : IQueryHandler<GetActiveToastsQuery, List<ToastInfo>>
{
    private readonly IToastOperations _toastOperations;

    public GetActiveToastsQueryHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public Task<List<ToastInfo>> HandleAsync(GetActiveToastsQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastOperations.GetActiveToasts());
    }
}