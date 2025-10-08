namespace Game.Core.Tests.CQS;

public class DerivedCommandHandler : BaseCommandHandler
{
    public override Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        // Override behavior
        return base.HandleAsync(command, cancellationToken);
    }
}