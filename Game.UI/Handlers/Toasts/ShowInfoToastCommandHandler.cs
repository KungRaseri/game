using Game.Core.CQS;
using Game.UI.Commands.Toasts;
using Game.UI.Models;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles showing an info notification toast.
/// </summary>
public class ShowInfoToastCommandHandler : ICommandHandler<ShowInfoToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowInfoToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowInfoToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Message = command.Message,
            Style = ToastStyle.Info,
            Animation = ToastAnimation.Fade,
            DisplayDuration = 3.0f,
            Anchor = command.Anchor
        };
        await _toastOperations.ShowToastAsync(config);
    }
}
