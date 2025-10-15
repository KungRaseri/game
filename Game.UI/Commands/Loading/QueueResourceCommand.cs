#nullable enable

using Game.Core.CQS;

namespace Game.UI.Commands.Loading;

/// <summary>
/// Command to queue a resource for background loading.
/// </summary>
public record QueueResourceCommand : ICommand
{
    /// <summary>
    /// Path to the resource to load.
    /// </summary>
    public string ResourcePath { get; init; } = string.Empty;

    /// <summary>
    /// Priority of this resource (higher values load first).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Category this resource belongs to.
    /// </summary>
    public string Category { get; init; } = "default";

    /// <summary>
    /// Dependencies that must be loaded before this resource.
    /// </summary>
    public List<string> Dependencies { get; init; } = new();

    /// <summary>
    /// Expected size of the resource in bytes (if known).
    /// </summary>
    public long? ExpectedSize { get; init; }

    /// <summary>
    /// Creates a simple resource queue command.
    /// </summary>
    public static QueueResourceCommand Simple(string resourcePath) => new()
    {
        ResourcePath = resourcePath,
        Priority = 0,
        Category = "default"
    };

    /// <summary>
    /// Creates a high-priority resource queue command.
    /// </summary>
    public static QueueResourceCommand HighPriority(string resourcePath) => new()
    {
        ResourcePath = resourcePath,
        Priority = 10,
        Category = "core"
    };

    /// <summary>
    /// Creates a resource queue command with dependencies.
    /// </summary>
    public static QueueResourceCommand WithDependencies(string resourcePath, params string[] dependencies) => new()
    {
        ResourcePath = resourcePath,
        Priority = 5,
        Category = "game",
        Dependencies = dependencies.ToList()
    };
}
