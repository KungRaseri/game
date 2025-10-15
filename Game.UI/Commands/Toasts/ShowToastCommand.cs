#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands.Toasts;

/// <summary>
/// Command to display a toast notification.
/// </summary>
public record ShowToastCommand(ToastConfig Config) : ICommand;
