#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries.Loading;

/// <summary>
/// Query to get the current loading progress.
/// </summary>
public record GetLoadingProgressQuery : IQuery<LoadingProgress>
{
    /// <summary>
    /// Whether to include detailed progress information.
    /// </summary>
    public bool IncludeDetails { get; init; } = true;

    /// <summary>
    /// Whether to include error information.
    /// </summary>
    public bool IncludeErrors { get; init; } = true;

    /// <summary>
    /// Creates a simple progress query.
    /// </summary>
    public static GetLoadingProgressQuery Simple() => new()
    {
        IncludeDetails = false,
        IncludeErrors = false
    };

    /// <summary>
    /// Creates a detailed progress query.
    /// </summary>
    public static GetLoadingProgressQuery Detailed() => new()
    {
        IncludeDetails = true,
        IncludeErrors = true
    };
}
