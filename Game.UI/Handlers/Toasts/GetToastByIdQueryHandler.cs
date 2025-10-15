using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries.Toasts;

namespace Game.UI.Handlers.Toasts;

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
