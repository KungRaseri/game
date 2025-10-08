using Game.Core.CQS;
using Game.UI.Commands;
using Game.UI.Models;

namespace Game.UI.Handlers;

/// <summary>
/// Handles showing a simple text toast.
/// </summary>
public class ShowSimpleToastCommandHandler : ICommandHandler<ShowSimpleToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowSimpleToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowSimpleToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Message = command.Message,
            Style = command.Style
        };
        await _toastOperations.ShowToastAsync(config);
    }
}