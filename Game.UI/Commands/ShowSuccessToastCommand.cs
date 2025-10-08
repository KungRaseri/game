using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to display a success notification toast.
/// </summary>
public record ShowSuccessToastCommand(string Message) : ICommand;