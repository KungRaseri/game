using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries.Toasts;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles getting all toasts at a specific anchor position.
/// </summary>
public class GetToastsByAnchorQueryHandler : IQueryHandler<GetToastsByAnchorQuery, List<ToastInfo>>
{
    private readonly IToastOperations _toastOperations;

    public GetToastsByAnchorQueryHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public Task<List<ToastInfo>> HandleAsync(GetToastsByAnchorQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastOperations.GetToastsByAnchor(query.Anchor));
    }
}
