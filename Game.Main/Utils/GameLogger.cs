using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Game.Main.Utils
{
    /// <summary>
    /// Centralized logging utility that extends Godot's Logger
    /// with game-specific functionality and log level filtering
    /// </summary>
    public class GameLogger : Logger
    {
        private LogLevel _currentLogLevel = LogLevel.Info;
        private bool _isDevelopmentEnvironment = true;

        /// <summary>
        /// Current minimum log level that will be output
        /// </summary>
        public LogLevel CurrentLogLevel
        {
            get => _currentLogLevel;
            set => _currentLogLevel = value;
        }

        /// <summary>
        /// Whether we're running in development environment
        /// </summary>
        public bool IsDevelopmentEnvironment
        {
            get => _isDevelopmentEnvironment;
            set => _isDevelopmentEnvironment = value;
        }

        public GameLogger()
        {
            InitializeEnvironment();
        }

        /// <summary>
        /// Initialize logger based on build configuration
        /// </summary>
        private void InitializeEnvironment()
        {
#if DEBUG
            _isDevelopmentEnvironment = true;
            _currentLogLevel = LogLevel.Debug;
#else
            _isDevelopmentEnvironment = false;
            _currentLogLevel = LogLevel.Info;
#endif
            GD.Print($"[SYSTEM] GameLogger initialized. Environment: {(_isDevelopmentEnvironment ? "Development" : "Production")}, LogLevel: {_currentLogLevel}");
        }

        /// <summary>
        /// Log debug message (only in development environment)
        /// </summary>
        [Conditional("DEBUG")]
        public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!ShouldLog(LogLevel.Debug)) return;

            GD.Print($"[DEBUG] {memberName}: {message}");
        }

        /// <summary>
        /// Log informational message
        /// </summary>
        public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!ShouldLog(LogLevel.Info)) return;

            GD.Print($"[INFO] {memberName}: {message}");
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void Warn(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!ShouldLog(LogLevel.Warn)) return;

            GD.PrintErr($"[WARN] {memberName}: {message}");
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!ShouldLog(LogLevel.Error)) return;

            GD.PrintErr($"[ERROR] {memberName}: {message}");
        }

        /// <summary>
        /// Log error with exception details
        /// </summary>
        public void Error(string message, Exception exception, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!ShouldLog(LogLevel.Error)) return;

            GD.PrintErr($"[ERROR] {memberName}: {message}");
            GD.PrintErr($"Exception: {exception.GetType().Name}: {exception.Message}");

            if (_isDevelopmentEnvironment && !string.IsNullOrEmpty(exception.StackTrace))
            {
                GD.PrintErr($"Stack Trace: {exception.StackTrace}");
            }
        }

        /// <summary>
        /// Log combat-specific events (can be filtered separately)
        /// </summary>
        public void Combat(string message, [CallerMemberName] string memberName = "")
        {
            if (!ShouldLog(LogLevel.Debug)) return;

            GD.Print($"[COMBAT] {memberName}: {message}");
        }

        /// <summary>
        /// Log system events like initialization, disposal
        /// </summary>
        public void System(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
        {
            if (!ShouldLog(LogLevel.Info)) return;

            GD.Print($"[SYSTEM] {memberName}: {message}");
        }

        /// <summary>
        /// Check if message should be logged based on current level
        /// </summary>
        private bool ShouldLog(LogLevel level)
        {
            return (int)level >= (int)_currentLogLevel;
        }

        /// <summary>
        /// Configure logger for testing environment
        /// </summary>
        public void ConfigureForTesting(LogLevel logLevel = LogLevel.Error)
        {
            _currentLogLevel = logLevel;
            _isDevelopmentEnvironment = false;
        }

        /// <summary>
        /// Reset logger to default configuration
        /// </summary>
        public void ResetToDefaults()
        {
            InitializeEnvironment();
        }
    }

    /// <summary>
    /// Log levels in order of severity
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3
    }
}