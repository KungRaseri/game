#nullable enable

using Game.Core.CQS;
using Game.UI.Commands;
using Game.UI.Models;

namespace Game.UI.Handlers;

/// <summary>
/// Interface for the toast manager that command handlers will operate on.
/// This defines the contract that the Godot ToastManager must implement.
/// </summary>
public interface IToastManager
{
    Task ShowToastAsync(ToastConfig config);
    Task ClearAllToastsAsync();
    Task DismissToastAsync(string toastId);
    List<ToastInfo> GetActiveToasts();
    ToastInfo? GetToastById(string toastId);
    List<ToastInfo> GetToastsByAnchor(ToastAnchor anchor);
    int GetActiveToastCount();
    bool IsToastLimitReached();
}

/// <summary>
/// Handles all toast-related commands.
/// Operates directly on the ToastManager implementation.
/// </summary>
public class ToastCommandHandlers : 
    ICommandHandler<ShowToastCommand>,
    ICommandHandler<ShowSimpleToastCommand>,
    ICommandHandler<ShowTitledToastCommand>,
    ICommandHandler<ShowMaterialToastCommand>,
    ICommandHandler<ShowSuccessToastCommand>,
    ICommandHandler<ShowWarningToastCommand>,
    ICommandHandler<ShowErrorToastCommand>,
    ICommandHandler<ShowInfoToastCommand>,
    ICommandHandler<ClearAllToastsCommand>,
    ICommandHandler<DismissToastCommand>
{
    private readonly IToastManager _toastManager;

    public ToastCommandHandlers(IToastManager toastManager)
    {
        _toastManager = toastManager;
    }

    public async Task HandleAsync(ShowToastCommand command, CancellationToken cancellationToken = default)
    {
        await _toastManager.ShowToastAsync(command.Config);
    }

    public async Task HandleAsync(ShowSimpleToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig 
        { 
            Message = command.Message, 
            Style = command.Style 
        };
        await _toastManager.ShowToastAsync(config);
    }

    public async Task HandleAsync(ShowTitledToastCommand command, CancellationToken cancellationToken = default)
    {
        var config = new ToastConfig 
        { 
            Title = command.Title,
            Message = command.Message, 
            Style = command.Style 
        };
        await _toastManager.ShowToastAsync(config);
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
        
        await _toastManager.ShowToastAsync(config);
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
        await _toastManager.ShowToastAsync(config);
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
        await _toastManager.ShowToastAsync(config);
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
        await _toastManager.ShowToastAsync(config);
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
        await _toastManager.ShowToastAsync(config);
    }

    public async Task HandleAsync(ClearAllToastsCommand command, CancellationToken cancellationToken = default)
    {
        await _toastManager.ClearAllToastsAsync();
    }

    public async Task HandleAsync(DismissToastCommand command, CancellationToken cancellationToken = default)
    {
        await _toastManager.DismissToastAsync(command.ToastId);
    }
}
