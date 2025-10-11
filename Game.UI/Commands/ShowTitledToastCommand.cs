using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Commands;

/// <summary>
/// Command to display a toast with title and message.
/// </summary>
public record ShowTitledToastCommand(string Title, string Message, ToastStyle Style = ToastStyle.Default, ToastAnchor Anchor = ToastAnchor.TopCenter) : ICommand;