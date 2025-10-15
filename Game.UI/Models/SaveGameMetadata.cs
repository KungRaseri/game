#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Represents save game metadata.
/// </summary>
public record SaveGameMetadata
{
    /// <summary>
    /// The save file name.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// The save file path.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// The last modified timestamp.
    /// </summary>
    public DateTime LastModified { get; init; }

    /// <summary>
    /// The play time in seconds.
    /// </summary>
    public double PlayTimeSeconds { get; init; }

    /// <summary>
    /// The character name or identifier.
    /// </summary>
    public string CharacterName { get; init; } = string.Empty;

    /// <summary>
    /// The save game version.
    /// </summary>
    public string Version { get; init; } = "1.0";
}
