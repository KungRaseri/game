#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands;

/// <summary>
/// Command to display a toast notification.
/// </summary>
public record ShowToastCommand(ToastConfig Config) : ICommand;

/// <summary>
/// Command to display a simple text toast.
/// </summary>
public record ShowSimpleToastCommand(string Message, ToastStyle Style = ToastStyle.Default) : ICommand;

/// <summary>
/// Command to display a toast with title and message.
/// </summary>
public record ShowTitledToastCommand(string Title, string Message, ToastStyle Style = ToastStyle.Default) : ICommand;

/// <summary>
/// Command to display a material collection toast.
/// </summary>
public record ShowMaterialToastCommand(List<string> Materials) : ICommand;

/// <summary>
/// Command to display a success notification toast.
/// </summary>
public record ShowSuccessToastCommand(string Message) : ICommand;

/// <summary>
/// Command to display a warning notification toast.
/// </summary>
public record ShowWarningToastCommand(string Message) : ICommand;

/// <summary>
/// Command to display an error notification toast.
/// </summary>
public record ShowErrorToastCommand(string Message) : ICommand;

/// <summary>
/// Command to display an info notification toast.
/// </summary>
public record ShowInfoToastCommand(string Message) : ICommand;

/// <summary>
/// Command to clear all active toasts.
/// </summary>
public record ClearAllToastsCommand : ICommand;

/// <summary>
/// Command to dismiss a specific toast by ID.
/// </summary>
public record DismissToastCommand(string ToastId) : ICommand;
