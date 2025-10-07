using Game.Core.CQS;

namespace Game.UI.Commands;

/// <summary>
/// Command to display an error notification toast.
/// </summary>
public record ShowErrorToastCommand(string Message) : ICommand;