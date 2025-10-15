#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Loading;

/// <summary>
/// Query to get information about the current loading phase.
/// </summary>
public record GetLoadingPhaseQuery : IQuery<LoadingPhaseInfo>
{
    /// <summary>
    /// Whether to include timing information.
    /// </summary>
    public bool IncludeTiming { get; init; } = true;

    /// <summary>
    /// Whether to include progress details.
    /// </summary>
    public bool IncludeProgress { get; init; } = true;

    /// <summary>
    /// Creates a basic phase query.
    /// </summary>
    public static GetLoadingPhaseQuery Basic() => new()
    {
        IncludeTiming = false,
        IncludeProgress = false
    };

    /// <summary>
    /// Creates a detailed phase query.
    /// </summary>
    public static GetLoadingPhaseQuery Detailed() => new()
    {
        IncludeTiming = true,
        IncludeProgress = true
    };
}
