#nullable enable

namespace Game.Core.Data.Models;

/// <summary>
/// Result wrapper for data loading operations that can succeed or fail
/// </summary>
/// <typeparam name="T">The type of data being loaded</typeparam>
public sealed class DataLoadResult<T> where T : class
{
    private DataLoadResult(T? data, string? error, bool isSuccess)
    {
        Data = data;
        ErrorMessage = error;
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// The loaded data (null if loading failed)
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Error message if loading failed (null if successful)
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Whether the loading operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Whether the loading operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    /// <param name="data">The loaded data</param>
    /// <returns>Successful result</returns>
    public static DataLoadResult<T> Success(T data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return new DataLoadResult<T>(data, null, true);
    }

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <returns>Failed result</returns>
    public static DataLoadResult<T> FromError(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new DataLoadResult<T>(null, errorMessage, false);
    }

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>Failed result</returns>
    public static DataLoadResult<T> FromError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new DataLoadResult<T>(null, exception.Message, false);
    }
}
