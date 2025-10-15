#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries;

/// <summary>
/// Query to retrieve all save files.
/// </summary>
public record GetAllSavesQuery : IQuery<IReadOnlyList<SaveGameMetadata>>
{
}
