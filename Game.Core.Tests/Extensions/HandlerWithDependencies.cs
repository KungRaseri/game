using Game.Core.CQS;
using Game.Core.Tests.CQS;

namespace Game.Core.Tests.Extensions;

/// <summary>
/// Test handler that has a dependency to verify DI works correctly.
/// </summary>
public class HandlerWithDependencies : ICommandHandler<TestCommand>
{
    private readonly TestDependency _dependency;

    public HandlerWithDependencies(TestDependency dependency)
    {
        _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
    }

    public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        _dependency.WasCalled = true;
        return Task.CompletedTask;
    }
}