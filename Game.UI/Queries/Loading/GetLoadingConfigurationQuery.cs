#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Loading;

/// <summary>
/// Query to get the current loading configuration.
/// </summary>
public record GetLoadingConfigurationQuery : IQuery<LoadingConfiguration>
{
    /// <summary>
    /// Whether to include asset lists in the response.
    /// </summary>
    public bool IncludeAssetLists { get; init; } = true;

    /// <summary>
    /// Whether to include loading tips.
    /// </summary>
    public bool IncludeTips { get; init; } = false;

    /// <summary>
    /// Creates a basic configuration query.
    /// </summary>
    public static GetLoadingConfigurationQuery Basic() => new()
    {
        IncludeAssetLists = false,
        IncludeTips = false
    };

    /// <summary>
    /// Creates a complete configuration query.
    /// </summary>
    public static GetLoadingConfigurationQuery Complete() => new()
    {
        IncludeAssetLists = true,
        IncludeTips = true
    };
}
