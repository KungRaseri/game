namespace Game.Main.Utils;

/// <summary>
/// Console-based logging backend for testing and non-Godot environments.
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