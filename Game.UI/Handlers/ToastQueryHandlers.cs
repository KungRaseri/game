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

/// <summary>
/// Handles getting a specific toast by its ID.
/// </summary>
public class GetToastByIdQueryHandler : IQueryHandler<GetToastByIdQuery, ToastInfo?>
{
    private readonly IToastOperations _toastOperations;

    public GetToastByIdQueryHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public Task<ToastInfo?> HandleAsync(GetToastByIdQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_toastOperations.GetToastById(query.ToastId));
    }
}

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
