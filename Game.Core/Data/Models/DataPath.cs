#nullable enable

using System.Runtime.CompilerServices;

namespace Game.Core.Data.Models;

/// <summary>
/// Helper for resolving data file paths within domain projects
/// </summary>
public static class DataPath
{
    /// <summary>
    /// Gets the path to a JSON data file within the calling domain's Data/json folder
    /// </summary>
    /// <param name="fileName">The JSON file name (with or without .json extension)</param>
    /// <param name="callerFilePath">Automatically filled by compiler</param>
    /// <returns>Full path to the JSON file</returns>
    public static string GetDomainJsonPath(string fileName, [CallerFilePath] string callerFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        // Ensure .json extension
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".json";
        }

        // Get the domain project root from the caller's file path
        var domainRoot = GetDomainRootFromFilePath(callerFilePath);
        
        return Path.Combine(domainRoot, "Data", "json", fileName);
    }

    /// <summary>
    /// Gets the domain root directory from a file path within the domain
    /// </summary>
    /// <param name="filePath">Path to a file within the domain project</param>
    /// <returns>Root directory of the domain project</returns>
    public static string GetDomainRootFromFilePath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        
        while (directory != null)
        {
            // Look for .csproj file to identify project root
            var csprojFiles = Directory.GetFiles(directory, "*.csproj");
            if (csprojFiles.Length > 0)
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException($"Could not find domain root for file: {filePath}");
    }

    /// <summary>
    /// Validates that a data file path exists
    /// </summary>
    /// <param name="dataPath">Path to validate</param>
    /// <returns>True if the file exists</returns>
    public static bool Exists(string dataPath)
    {
        return !string.IsNullOrWhiteSpace(dataPath) && File.Exists(dataPath);
    }

    /// <summary>
    /// Creates the directory for a data file path if it doesn't exist
    /// </summary>
    /// <param name="dataPath">Path to the data file</param>
    public static void EnsureDirectoryExists(string dataPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPath);

        var directory = Path.GetDirectoryName(dataPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
