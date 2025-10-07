using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Test command with return value for testing ICommand<TResult> interface.
/// </summary>
public record TestCommandWithResult(string Data) : ICommand<string>;