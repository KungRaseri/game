#nullable enable

using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries;

namespace Game.UI.Handlers;

/// <summary>
/// Handles all toast-related queries.
/// Operates directly on the ToastManager implementation.
/// </summary>
public class ToastQueryHandlers :
    IQueryHandler<GetActiveToastsQuery, List<ToastInfo>>,
    IQueryHandler<GetToastByIdQuery, ToastInfo?>,
    IQueryHandler<GetToastsByAnchorQuery, List<ToastInfo>>,
    IQueryHandler<GetActiveToastCountQuery, int>,
    IQueryHandler<IsToastLimitReachedQuery, bool>
{
    private readonly IToastManager _toastManager;

    public ToastQueryHandlers(IToastManager toastManager)
    {
        _toastManager = toastManager;
    }

    public Task<List<ToastInfo>> HandleAsync(GetActiveToastsQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastManager.GetActiveToasts());
    }

    public Task<ToastInfo?> HandleAsync(GetToastByIdQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastManager.GetToastById(query.ToastId));
    }

    public Task<List<ToastInfo>> HandleAsync(GetToastsByAnchorQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastManager.GetToastsByAnchor(query.Anchor));
    }

    public Task<int> HandleAsync(GetActiveToastCountQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastManager.GetActiveToastCount());
    }

    public Task<bool> HandleAsync(IsToastLimitReachedQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastManager.IsToastLimitReached());
    }
}
