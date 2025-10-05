#nullable enable

namespace Game.Core.CQS;

/// <summary>
/// Interface for queries that return data without modifying state.
/// Queries should be side-effect free and idempotent.
/// Following CQS principles: Queries return data but don't change state.
/// </summary>
/// <typeparam name="TResult">Type of data returned by the query</typeparam>
public interface IQuery<TResult>
{
}
