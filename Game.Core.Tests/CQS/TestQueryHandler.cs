using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

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