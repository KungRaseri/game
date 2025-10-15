#nullable enable

using Game.Core.CQS;

namespace Game.UI.Queries.SaveGames;

/// <summary>
/// Query to check if any save files exist.
/// </summary>
public record HasSaveFilesQuery : IQuery<bool>
{
}
