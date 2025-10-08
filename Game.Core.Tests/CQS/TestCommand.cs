#nullable enable

using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Test command without return value for testing ICommand interface.
/// </summary>
public record TestCommand(string Data) : ICommand;