using Game.Core.CQS;

namespace Game.UI.Commands.Toasts;

/// <summary>
/// Command to clear all active toasts.
/// </summary>
public record ClearAllToastsCommand : ICommand;
