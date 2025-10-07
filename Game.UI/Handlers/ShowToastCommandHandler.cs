#nullable enable

using Game.Core.CQS;
using Game.UI.Commands;
using Game.UI.Models;

namespace Game.UI.Handlers;

/// <summary>
/// Handles showing a toast with full configuration.
/// </summary>
public class ShowToastCommandHandler : ICommandHandler<ShowToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowToastCommand command, CancellationToken cancellationToken = default)
    {
        await _toastOperations.ShowToastAsync(command.Config);
    }
}

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

/// <summary>
/// Handles showing a toast with title and message.
/// </summary>
public class ShowTitledToastCommandHandler : ICommandHandler<ShowTitledToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowTitledToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowTitledToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Title = command.Title,
            Message = command.Message,
            Style = command.Style
        };
        await _toastOperations.ShowToastAsync(config);
    }
}

/// <summary>
/// Handles showing a material collection toast.
/// </summary>
public class ShowMaterialToastCommandHandler : ICommandHandler<ShowMaterialToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowMaterialToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowMaterialToastCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Materials.Count == 0) return;

        var message = string.Join(", ", command.Materials);
        var config = new ToastConfig
        {
            Title = "Materials Collected",
            Message = message,
            Style = ToastStyle.Material,
            Anchor = ToastAnchor.TopRight,
            Animation = ToastAnimation.SlideFromRight,
            DisplayDuration = 4.0f
        };

        await _toastOperations.ShowToastAsync(config);
    }
}

/// <summary>
/// Handles showing a success notification toast.
/// </summary>
public class ShowSuccessToastCommandHandler : ICommandHandler<ShowSuccessToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowSuccessToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowSuccessToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Message = command.Message,
            Style = ToastStyle.Success,
            Animation = ToastAnimation.Bounce,
            DisplayDuration = 3.0f
        };
        await _toastOperations.ShowToastAsync(config);
    }
}

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
            DisplayDuration = 4.0f
        };
        await _toastOperations.ShowToastAsync(config);
    }
}

/// <summary>
/// Handles showing an error notification toast.
/// </summary>
public class ShowErrorToastCommandHandler : ICommandHandler<ShowErrorToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public ShowErrorToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ShowErrorToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig
        {
            Message = command.Message,
            Style = ToastStyle.Error,
            Animation = ToastAnimation.Scale,
            DisplayDuration = 5.0f,
            Anchor = ToastAnchor.Center
        };
        await _toastOperations.ShowToastAsync(config);
    }
}

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
            DisplayDuration = 3.0f
        };
        await _toastOperations.ShowToastAsync(config);
    }
}

/// <summary>
/// Handles clearing all active toasts.
/// </summary>
public class ClearAllToastsCommandHandler : ICommandHandler<ClearAllToastsCommand>
{
    private readonly IToastOperations _toastOperations;

    public ClearAllToastsCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(ClearAllToastsCommand command, CancellationToken cancellationToken = default)
    {
        await _toastOperations.ClearAllToastsAsync();
    }
}

/// <summary>
/// Handles dismissing a specific toast by ID.
/// </summary>
public class DismissToastCommandHandler : ICommandHandler<DismissToastCommand>
{
    private readonly IToastOperations _toastOperations;

    public DismissToastCommandHandler(IToastOperations toastOperations)
    {
        _toastOperations = toastOperations ?? throw new ArgumentNullException(nameof(toastOperations));
    }

    public async Task HandleAsync(DismissToastCommand command, CancellationToken cancellationToken = default)
    {
        await _toastOperations.DismissToastAsync(command.ToastId);
    }
}
