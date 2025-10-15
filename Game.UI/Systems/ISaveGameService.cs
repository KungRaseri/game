#nullable enable

using Game.UI.Models;

namespace Game.UI.Systems;

/// <summary>
/// Interface for managing save game operations.
/// </summary>
public interface ISaveGameService
{
    /// <summary>
    /// Checks if any save files exist.
    /// </summary>
    bool HasAnySaveFiles();

    /// <summary>
    /// Gets the most recent save file metadata.
    /// </summary>
    SaveGameMetadata? GetMostRecentSave();

    /// <summary>
    /// Gets all save file metadata.
    /// </summary>
    IReadOnlyList<SaveGameMetadata> GetAllSaves();
}
