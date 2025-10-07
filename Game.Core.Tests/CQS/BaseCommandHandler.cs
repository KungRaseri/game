using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

public class BaseCommandHandler : ICommandHandler<TestCommand>
{
    public virtual Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}