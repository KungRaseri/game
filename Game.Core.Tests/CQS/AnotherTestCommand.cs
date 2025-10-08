using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Another test command for testing multiple handler registrations.
/// </summary>
public record AnotherTestCommand(int Value) : ICommand;