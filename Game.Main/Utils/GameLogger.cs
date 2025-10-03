namespace Game.Main.Utils;

/// <summary>
/// Centralized logging utility with level filtering and caller information.
/// Uses dependency injection pattern to work in both Godot runtime and test environments.
/// Follows Godot C# best practices for static utility classes.
/// </summary>
public static class GameLogger
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    private static LogLevel _currentLogLevel = LogLevel.Debug;
    private static ILoggerBackend _backend = new ConsoleLoggerBackend();

    /// <summary>
    /// Gets or sets the minimum log level to display.
    /// </summary>
    public static LogLevel CurrentLogLevel
    {
        get => _currentLogLevel;
        set => _currentLogLevel = value;
    }

    /// <summary>
    /// Sets the logging backend implementation.
    /// </summary>
    /// <param name="backend">The backend to use, or null to use console backend.</param>
    public static void SetBackend(ILoggerBackend? backend)
    {
        _backend = backend ?? new ConsoleLoggerBackend();
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    public static void Debug(string message, CallerInfo? caller = null)
    {
        LogMessage(LogLevel.Debug, message, caller ?? new CallerInfo());
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    public static void Info(string message, CallerInfo? caller = null)
    {
        LogMessage(LogLevel.Info, message, caller ?? new CallerInfo());
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void Warning(string message, CallerInfo? caller = null)
    {
        LogMessage(LogLevel.Warning, message, caller ?? new CallerInfo());
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public static void Error(string message, CallerInfo? caller = null)
    {
        LogMessage(LogLevel.Error, message, caller ?? new CallerInfo());
    }

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    public static void Error(Exception exception, string message = "", CallerInfo? caller = null)
    {
        caller ??= new CallerInfo();

        var fullMessage = string.IsNullOrEmpty(message)
            ? $"Exception: {exception.Message}"
            : $"{message} - Exception: {exception.Message}";

        LogMessage(LogLevel.Error, fullMessage, caller);

        if (_currentLogLevel <= LogLevel.Debug && !string.IsNullOrEmpty(exception.StackTrace))
        {
            LogMessage(LogLevel.Error, $"Stack trace: {exception.StackTrace}", caller);
        }
    }

    private static void LogMessage(LogLevel level, string message, CallerInfo caller)
    {
        if (level < _currentLogLevel)
            return;

        var fileName = System.IO.Path.GetFileNameWithoutExtension(caller.FilePath);
        var levelStr = level.ToString().ToUpper();
        var formattedMessage = $"[{levelStr}] {fileName}.{caller.MemberName}:{caller.LineNumber} - {message}";

        _backend.Log(level, formattedMessage);
    }
}