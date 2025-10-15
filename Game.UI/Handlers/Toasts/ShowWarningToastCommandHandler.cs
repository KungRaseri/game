using Game.Core.CQS;
using Game.UI.Commands.Toasts;
using Game.UI.Models;

namespace Game.UI.Handlers.Toasts;

/// <summary>
/// Handles showing a warning notification toast.
/// </summary>
public class ShowWarningToastCommandHandler : ICommandHandler<ShowWarningToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowWarningToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowWarningToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Message = command.Message,
            Style = ToastStyle.Warning,
            Animation = ToastAnimation.SlideFromTop,
            DisplayDuration = 4.0f,
            Anchor = command.Anchor
        };
        await _toastOperations.ShowToastAsync(config);
    }
}
