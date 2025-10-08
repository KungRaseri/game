#nullable enable

using System.Collections.Concurrent;

namespace Game.Core.Utils;

/// <summary>
/// A logger backend that captures log messages for testing purposes.
/// This class is intended for unit testing scenarios where you need to verify logging behavior.
/// </summary>
public class TestableLoggerBackend : ILoggerBackend
{
    private readonly ConcurrentQueue<LogEntry> _logs = new();

    public void Log(GameLogger.LogLevel level, string message)
    {
        _logs.Enqueue(new LogEntry(level, message));
    }

    /// <summary>
    /// Gets all captured log entries.
    /// </summary>
    public IReadOnlyList<LogEntry> GetLogs() => _logs.ToList();

    /// <summary>
    /// Clears all captured log entries.
    /// </summary>
    public void Clear()
    {
        while (_logs.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// Represents a captured log entry for testing verification.
    /// </summary>
    public record LogEntry(GameLogger.LogLevel Level, string Message);
}
