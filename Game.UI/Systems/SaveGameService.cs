#nullable enable

using Game.Core.Utils;
using Game.UI.Models;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.UI.Systems;

/// <summary>
/// Service for managing save game operations.
/// </summary>
public class SaveGameService : ISaveGameService
{
    private const string SaveFileExtension = ".sav";
    private const string SaveDirectoryName = "saves";
    
    private readonly string _saveDirectoryPath;

    public SaveGameService()
    {
        _saveDirectoryPath = System.IO.Path.Combine(OS.GetUserDataDir(), SaveDirectoryName);
        GameLogger.Info($"SaveGameService: Save directory: {_saveDirectoryPath}");
    }

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
                GameLogger.Error($"SaveGameService: Failed to open save directory: {_saveDirectoryPath}");
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
            GameLogger.Error($"SaveGameService: Exception checking for save files: {ex.Message}");
            return false;
        }
    }

    public SaveGameMetadata? GetMostRecentSave()
    {
        var saveFiles = GetAllSaves();
        return saveFiles.OrderByDescending(s => s.LastModified).FirstOrDefault();
    }

    public IReadOnlyList<SaveGameMetadata> GetAllSaves()
    {
        var saves = new List<SaveGameMetadata>();

        try
        {
            if (!DirAccess.DirExistsAbsolute(_saveDirectoryPath))
            {
                return saves;
            }

            using var dir = DirAccess.Open(_saveDirectoryPath);
            if (dir == null)
            {
                GameLogger.Error($"SaveGameService: Failed to open save directory");
                return saves;
            }

            dir.ListDirBegin();
            string? fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(SaveFileExtension))
                {
                    var filePath = System.IO.Path.Combine(_saveDirectoryPath, fileName);
                    var metadata = GetSaveMetadata(filePath);
                    if (metadata != null)
                    {
                        saves.Add(metadata);
                    }
                }
                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SaveGameService: Exception getting save files: {ex.Message}");
        }

        return saves;
    }

    private static SaveGameMetadata? GetSaveMetadata(string filePath)
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
            // For now, just get file modification time
            
            var fileName = System.IO.Path.GetFileName(filePath);
            var lastModified = Godot.FileAccess.GetModifiedTime(filePath);
            var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)lastModified).DateTime;
            
            return new SaveGameMetadata
            {
                FileName = fileName,
                FilePath = filePath,
                LastModified = dateTime,
                PlayTimeSeconds = 0, // TODO: Read from save file when implemented
                CharacterName = string.Empty, // TODO: Read from save file when implemented
                Version = "1.0"
            };
        }
        catch (Exception ex)
        {
            GameLogger.Error($"SaveGameService: Exception reading save file metadata: {ex.Message}");
            return null;
        }
    }
}

