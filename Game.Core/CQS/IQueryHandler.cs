#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Game.Core.CQS;

/// <summary>
/// Interface for query handlers that execute queries and return data.
/// Handlers contain the actual business logic for data retrieval operations.
/// Queries should be side-effect free and idempotent.
/// </summary>
/// <typeparam name="TQuery">Type of query to handle</typeparam>
/// <typeparam name="TResult">Type of result returned by the query</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Executes the query asynchronously and returns the requested data.
    /// Contains the actual business logic for data retrieval.
    /// Should not modify state - read-only operation.
    /// </summary>
    /// <param name="query">Query to execute</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Data requested by the query</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
