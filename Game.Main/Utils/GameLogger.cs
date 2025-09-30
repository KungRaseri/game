using System;

namespace Game.Main.Utils;

/// <summary>
/// Centralized logging utility with level filtering and caller information
/// Uses dependency injection pattern to work in both Godot runtime and test environments
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
    /// Gets or sets the minimum log level to display
    /// </summary>
    public static LogLevel CurrentLogLevel
    {
        get => _currentLogLevel;
        set => _currentLogLevel = value;
    }

    /// <summary>
    /// Sets the logging backend implementation
    /// </summary>
    public static void SetBackend(ILoggerBackend backend)
    {
        _backend = backend ?? new ConsoleLoggerBackend();
    }

    /// <summary>
    /// Logs a debug message
    /// </summary>
    public static void Debug(string message, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
    {
        LogMessage(LogLevel.Debug, message, callerName, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Logs an info message
    /// </summary>
    public static void Info(string message, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
    {
        LogMessage(LogLevel.Info, message, callerName, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    public static void Warning(string message, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
    {
        LogMessage(LogLevel.Warning, message, callerName, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Logs an error message
    /// </summary>
    public static void Error(string message, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
    {
        LogMessage(LogLevel.Error, message, callerName, callerFilePath, callerLineNumber);
    }

    /// <summary>
    /// Logs an error message with exception details
    /// </summary>
    public static void Error(Exception exception, string message = "", [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
    {
        var fullMessage = string.IsNullOrEmpty(message) 
            ? $"Exception: {exception.Message}" 
            : $"{message} - Exception: {exception.Message}";
        
        LogMessage(LogLevel.Error, fullMessage, callerName, callerFilePath, callerLineNumber);
        
        if (_currentLogLevel <= LogLevel.Debug)
        {
            LogMessage(LogLevel.Error, $"Stack trace: {exception.StackTrace}", callerName, callerFilePath, callerLineNumber);
        }
    }

    private static void LogMessage(LogLevel level, string message, string callerName, string callerFilePath, int callerLineNumber)
    {
        if (level < _currentLogLevel)
            return;

        var fileName = System.IO.Path.GetFileNameWithoutExtension(callerFilePath);
        var levelStr = level.ToString().ToUpper();
        var formattedMessage = $"[{levelStr}] {fileName}.{callerName}:{callerLineNumber} - {message}";

        _backend.Log(level, formattedMessage);
    }
}

/// <summary>
/// Interface for logging backend implementations
/// </summary>
public interface ILoggerBackend
{
    void Log(GameLogger.LogLevel level, string message);
}

/// <summary>
/// Console-based logging backend for testing and non-Godot environments
/// </summary>
public class ConsoleLoggerBackend : ILoggerBackend
{
    public void Log(GameLogger.LogLevel level, string message)
    {
        if (level >= GameLogger.LogLevel.Error)
        {
            Console.Error.WriteLine(message);
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
