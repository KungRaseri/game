#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Represents display settings data.
/// </summary>
public record DisplaySettingsData
{
    /// <summary>
    /// Fullscreen mode enabled.
    /// </summary>
    public bool Fullscreen { get; init; } = false;
}
