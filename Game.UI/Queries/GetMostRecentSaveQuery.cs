#nullable enable

using Game.Core.CQS;
using Game.UI.Models;

namespace Game.UI.Queries;

/// <summary>
/// Query to retrieve the most recent save file.
/// </summary>
public record GetMostRecentSaveQuery : IQuery<SaveGameMetadata?>
{
}
