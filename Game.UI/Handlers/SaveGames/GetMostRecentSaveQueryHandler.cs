#nullable enable

using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries.SaveGames;
using Game.UI.Systems;

namespace Game.UI.Handlers.SaveGames;

/// <summary>
/// Handles the GetMostRecentSaveQuery by retrieving the most recent save file.
/// </summary>
public class GetMostRecentSaveQueryHandler : IQueryHandler<GetMostRecentSaveQuery, SaveGameMetadata?>
{
    private readonly ISaveGameService _saveGameService;

    public GetMostRecentSaveQueryHandler(ISaveGameService saveGameService)
    {
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
    }

    public async Task<SaveGameMetadata?> HandleAsync(GetMostRecentSaveQuery query, CancellationToken cancellationToken = default)
    {
        var mostRecent = _saveGameService.GetMostRecentSave();
        return await Task.FromResult(mostRecent);
    }
}
