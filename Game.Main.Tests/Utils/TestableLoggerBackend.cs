#nullable enable

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Game.Main.Utils;

namespace Game.Main.Tests.Utils;

/// <summary>
/// A logger backend that captures log messages for testing purposes.
/// </summary>
public class TestableLoggerBackend : ILoggerBackend
{
    private readonly ConcurrentQueue<LogEntry> _logs = new();

    public void Log(GameLogger.LogLevel level, string message)
    {
        _logs.Enqueue(new LogEntry(level, message));
    }

    public IReadOnlyList<LogEntry> GetLogs() => _logs.ToList();

    public void Clear() 
    {
        while (_logs.TryDequeue(out _)) { }
    }

    public record LogEntry(GameLogger.LogLevel Level, string Message);
}
