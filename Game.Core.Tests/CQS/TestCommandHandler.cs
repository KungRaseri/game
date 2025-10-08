using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Test command handler that implements ICommandHandler<TCommand>.
/// </summary>
public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public bool WasCalled { get; private set; }
    public TestCommand? LastCommand { get; private set; }

    public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        LastCommand = command;
        return Task.CompletedTask;
    }
}