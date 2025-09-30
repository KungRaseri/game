using Godot;

namespace Game.Main.Utils;

/// <summary>
/// Godot-specific logging backend that uses GD.Print and GD.PrintErr.
/// This class should only be instantiated when running in Godot environment.
/// Follows Godot C# best practices for integration with GD static class.
/// </summary>
public class GodotLoggerBackend : ILoggerBackend
{
    /// <summary>
    /// Logs a message using Godot's GD.Print or GD.PrintErr methods.
    /// </summary>
    /// <param name="level">The log level to determine output method.</param>
    /// <param name="message">The formatted message to log.</param>
    public void Log(GameLogger.LogLevel level, string message)
    {
        // Use GD.PrintErr for errors and warnings, GD.Print for others
        // This follows Godot's convention for error output
        if (level >= GameLogger.LogLevel.Warning)
        {
            GD.PrintErr(message);
        }
        else
        {
            GD.Print(message);
        }
    }
}
