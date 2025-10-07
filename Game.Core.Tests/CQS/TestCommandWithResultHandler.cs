using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Test command handler with result that implements ICommandHandler<TCommand, TResult>.
/// </summary>
public class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, string>
{
    public bool WasCalled { get; private set; }
    public TestCommandWithResult? LastCommand { get; private set; }

    public Task<string> HandleAsync(TestCommandWithResult command, CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        LastCommand = command;
        return Task.FromResult($"Processed: {command.Data}");
    }
}