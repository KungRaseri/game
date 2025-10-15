#nullable enable

namespace Game.UI.Models;

/// <summary>
/// Configuration for the loading system.
/// </summary>
public record LoadingConfiguration
{
    /// <summary>
    /// Core assets that must be loaded first.
    /// </summary>
    public List<string> CoreAssets { get; init; } = new();

    /// <summary>
    /// Game assets that can be loaded after core assets.
    /// </summary>
    public List<string> GameAssets { get; init; } = new();

    /// <summary>
    /// Optional assets that can be loaded in the background.
    /// </summary>
    public List<string> OptionalAssets { get; init; } = new();

    /// <summary>
    /// Maximum number of concurrent loading operations.
    /// </summary>
    public int MaxConcurrentLoads { get; init; } = 4;

    /// <summary>
    /// How often to update loading progress (in seconds).
    /// </summary>
    public float ProgressUpdateInterval { get; init; } = 0.1f;

    /// <summary>
    /// Whether to show detailed loading information.
    /// </summary>
    public bool ShowDetailedProgress { get; init; } = true;

    /// <summary>
    /// Loading tips to display during loading.
    /// </summary>
    public List<string> LoadingTips { get; init; } = new();

    /// <summary>
    /// Scene to transition to after loading completes.
    /// </summary>
    public string? NextScenePath { get; init; }

    /// <summary>
    /// Whether to preload assets in advance.
    /// </summary>
    public bool EnablePreloading { get; init; } = true;

    /// <summary>
    /// Timeout for individual resource loading (in seconds).
    /// </summary>
    public float ResourceTimeout { get; init; } = 30.0f;

    /// <summary>
    /// Whether to retry failed resource loads.
    /// </summary>
    public bool RetryFailedLoads { get; init; } = true;

    /// <summary>
    /// Maximum number of retry attempts for failed loads.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Default configuration for typical game loading.
    /// </summary>
    public static LoadingConfiguration Default => new()
    {
        CoreAssets = new List<string>
        {
            "res://Scenes/UI/Toast.tscn",
            "res://Scripts/DI/DependencyInjectionNode.cs"
        },
        GameAssets = new List<string>
        {
            "res://Scenes/MainGameScene.tscn",
            "res://Scenes/UI/AdventurerStatus.tscn",
            "res://Scenes/UI/CraftingWorkshop.tscn",
            "res://Scenes/UI/ShopManagement.tscn"
        },
        LoadingTips = new List<string>
        {
            "Gather herbs to craft powerful potions!",
            "Keep your adventurers well-equipped for dangerous expeditions.",
            "Happy customers bring more gold to your shop!",
            "Crafting higher quality items increases your reputation."
        },
        MaxConcurrentLoads = 4,
        ProgressUpdateInterval = 0.1f,
        ShowDetailedProgress = true,
        EnablePreloading = true,
        ResourceTimeout = 30.0f,
        RetryFailedLoads = true,
        MaxRetryAttempts = 3
    };

    /// <summary>
    /// Gets all assets in loading order (core first, then game, then optional).
    /// </summary>
    public List<string> GetAllAssets()
    {
        var allAssets = new List<string>();
        allAssets.AddRange(CoreAssets);
        allAssets.AddRange(GameAssets);
        allAssets.AddRange(OptionalAssets);
        return allAssets;
    }

    /// <summary>
    /// Gets the priority for a given asset path.
    /// </summary>
    public int GetAssetPriority(string assetPath)
    {
        if (CoreAssets.Contains(assetPath)) return 10; // Critical priority
        if (GameAssets.Contains(assetPath)) return 5;  // High priority
        if (OptionalAssets.Contains(assetPath)) return 1; // Low priority
        return 3; // Default priority
    }

    /// <summary>
    /// Gets the category for a given asset path.
    /// </summary>
    public string GetAssetCategory(string assetPath)
    {
        if (CoreAssets.Contains(assetPath)) return "core";
        if (GameAssets.Contains(assetPath)) return "game";
        if (OptionalAssets.Contains(assetPath)) return "optional";
        return "default";
    }
}
