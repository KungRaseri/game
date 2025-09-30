using Godot;

namespace Game.Main.Utils;

/// <summary>
/// Godot-specific logging backend that uses GD.Print and GD.PrintErr
/// This class should only be instantiated when running in Godot environment
/// </summary>
public class GodotLoggerBackend : ILoggerBackend
{
    public void Log(GameLogger.LogLevel level, string message)
    {
        if (level >= GameLogger.LogLevel.Error)
        {
            GD.PrintErr(message);
        }
        else
        {
            GD.Print(message);
        }
    }
}
