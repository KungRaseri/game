using Game.Core.CQS;

namespace Game.Core.Tests.CQS;

/// <summary>
/// Test query for testing IQuery<TResult> interface.
/// </summary>
public record TestQuery(string Filter) : IQuery<string>;