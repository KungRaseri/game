using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Handler that throws an exception for testing error scenarios.
/// </summary>
public class ThrowingCommandHandler : ICommandHandler<AnotherTestCommand>
{
    public Task HandleAsync(AnotherTestCommand command, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Test exception");
    }
}