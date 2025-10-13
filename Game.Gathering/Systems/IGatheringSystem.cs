#nullable enable

using Game.Gathering.Commands;

namespace Game.Gathering.Systems;

/// <summary>
/// Interface for the gathering system to support dependency injection and testing.
/// </summary>
public interface IGatheringSystem
{
    /// <summary>
    /// Gathers materials from the specified location with the given effort level.
    /// </summary>
    /// <param name="location">The location to gather from</param>
    /// <param name="effort">The effort level to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the gathering operation</returns>
    Task<GatherMaterialsResult> GatherMaterialsAsync(string location, GatheringEffort effort, CancellationToken cancellationToken = default);
}
