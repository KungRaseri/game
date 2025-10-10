#nullable enable

using Game.Core.Data.Models;

namespace Game.Core.Data.Interfaces;

/// <summary>
/// Generic interface for loading data from various sources (JSON files, databases, etc.)
/// </summary>
/// <typeparam name="T">The type of data to load</typeparam>
public interface IDataLoader<T> where T : class
{
    /// <summary>
    /// Loads data from the specified path asynchronously
    /// </summary>
    /// <param name="dataPath">Path to the data source (file path, connection string, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the loaded data or error information</returns>
    Task<DataLoadResult<T>> LoadAsync(string dataPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads data from the specified path synchronously (for fallback scenarios)
    /// </summary>
    /// <param name="dataPath">Path to the data source</param>
    /// <returns>Result containing the loaded data or error information</returns>
    DataLoadResult<T> Load(string dataPath);
}
