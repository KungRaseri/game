#nullable enable

using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Test command without return value for testing ICommand interface.
/// </summary>
public record TestCommand(string Data) : ICommand;

/// <summary>
/// Test command with return value for testing ICommand<TResult> interface.
/// </summary>
public record TestCommandWithResult(string Data) : ICommand<string>;

/// <summary>
/// Test query for testing IQuery<TResult> interface.
/// </summary>
public record TestQuery(string Filter) : IQuery<string>;

/// <summary>
/// Another test command for testing multiple handler registrations.
/// </summary>
public record AnotherTestCommand(int Value) : ICommand;

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

/// <summary>
/// Test query handler that implements IQueryHandler<TQuery, TResult>.
/// </summary>
public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public bool WasCalled { get; private set; }
    public TestQuery? LastQuery { get; private set; }

    public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        LastQuery = query;
        return Task.FromResult($"Query result for: {query.Filter}");
    }
}

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
