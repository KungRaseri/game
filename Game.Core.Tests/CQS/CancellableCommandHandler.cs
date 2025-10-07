using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Handler that supports cancellation for testing cancellation token scenarios.
/// </summary>
public class CancellableCommandHandler : ICommandHandler<TestCommand>
{
    public async Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        // Simulate some async work
        await Task.Delay(100, cancellationToken);
    }
}