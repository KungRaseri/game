using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to dismiss a specific toast by ID.
/// </summary>
public record DismissToastCommand(string ToastId) : ICommand;