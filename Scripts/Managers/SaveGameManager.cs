#nullable enable

using Game.Core.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Scripts.Managers;

/// <summary>
/// Manages save game files, including detection, validation, and metadata retrieval.
/// </summary>
public class SaveGameManager
{
    private const string SaveFileExtension = ".sav";
    private const string SaveDirectoryName = "saves";
    
    private readonly string _saveDirectoryPath;

    public SaveGameManager()
    {
        _saveDirectoryPath = System.IO.Path.Combine(OS.GetUserDataDir(), SaveDirectoryName);
        GameLogger.Info($"SaveGameManager: Save directory: {_saveDirectoryPath}");
    }

    /// <summary>
    /// Checks if any save files exist.
    /// </summary>
    public bool HasAnySaveFiles()
    {
        try
        {
            if (!DirAccess.DirExistsAbsolute(_saveDirectoryPath))
            {
                return false;
            }

            using var dir = DirAccess.Open(_saveDirectoryPath);
            if (dir == null)
            {
                GameLogger.Error($"SaveGameManager: Failed to open save directory: {_saveDirectoryPath}");
                return false;
            }

            dir.ListDirBegin();
            string? fileName = dir.GetNext();
            
            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(SaveFileExtension))
                {
                    dir.ListDirEnd();
                    return true;
                }
                fileName = dir.GetNext();
            }
            
            dir.ListDirEnd();
            return false;
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SaveGameManager: Exception checking for save files: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the most recent save file.
    /// </summary>
    public SaveFileInfo? GetMostRecentSave()
    {
        var saveFiles = GetAllSaveFiles();
        return saveFiles.OrderByDescending(s => s.LastModified).FirstOrDefault();
    }

    /// <summary>
    /// Gets information about all save files.
    /// </summary>
    public List<SaveFileInfo> GetAllSaveFiles()
    {
        var saves = new List<SaveFileInfo>();

        try
        {
            if (!DirAccess.DirExistsAbsolute(_saveDirectoryPath))
            {
                return saves;
            }

            using var dir = DirAccess.Open(_saveDirectoryPath);
            if (dir == null)
            {
                GameLogger.Error($"SaveGameManager: Failed to open save directory");
                return saves;
            }

            dir.ListDirBegin();
            string? fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(SaveFileExtension))
                {
                    var filePath = System.IO.Path.Combine(_saveDirectoryPath, fileName);
                    var fileInfo = GetSaveFileInfo(filePath);
                    if (fileInfo != null)
                    {
                        saves.Add(fileInfo);
                    }
                }
                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SaveGameManager: Exception getting save files: {ex.Message}");
        }

        return saves;
    }

    /// <summary>
    /// Gets information about a specific save file.
    /// </summary>
    private SaveFileInfo? GetSaveFileInfo(string filePath)
    {
        try
        {
            if (!Godot.FileAccess.FileExists(filePath))
            {
                return null;
            }

            using var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                return null;
            }

            // TODO: Read actual save file metadata when save system is fully implemented
            // For now, just get file modification time and size
            
            var fileName = System.IO.Path.GetFileName(filePath);
            var lastModified = Godot.FileAccess.GetModifiedTime(filePath);
            
            return new SaveFileInfo
            {
                FileName = fileName,
                FilePath = filePath,
                LastModified = lastModified,
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SaveGameManager: Exception reading save file info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates the save directory if it doesn't exist.
    /// </summary>
    public void EnsureSaveDirectoryExists()
    {
        try
        {
            if (!DirAccess.DirExistsAbsolute(_saveDirectoryPath))
            {
                DirAccess.MakeDirRecursiveAbsolute(_saveDirectoryPath);
                GameLogger.Info($"SaveGameManager: Created save directory: {_saveDirectoryPath}");
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SaveGameManager: Failed to create save directory: {ex.Message}");
        }
    }
}

/// <summary>
/// Information about a save game file.
/// </summary>
public class SaveFileInfo
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required ulong LastModified { get; init; }
    public required bool IsValid { get; init; }
    
    // TODO: Add more metadata fields as the save system is implemented
    // public string? CharacterName { get; init; }
    // public int? Level { get; init; }
    // public TimeSpan? PlayTime { get; init; }
    // public string? LocationName { get; init; }
}
