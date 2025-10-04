namespace Game.Core.Utils;

/// <summary>
/// Interface for logging backend implementations.
/// Allows for dependency injection of different logging targets.
/// </summary>
public interface ILoggerBackend
{
    /// <summary>
    /// Logs a message at the specified level.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The formatted message to log.</param>
    void Log(GameLogger.LogLevel level, string message);
}