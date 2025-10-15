#nullable enable

using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries.SaveGames;
using Game.UI.Systems;

namespace Game.UI.Handlers.SaveGames;

/// <summary>
/// Handles the GetAllSavesQuery by retrieving all save files.
/// </summary>
public class GetAllSavesQueryHandler : IQueryHandler<GetAllSavesQuery, IReadOnlyList<SaveGameMetadata>>
{
    private readonly ISaveGameService _saveGameService;

    public GetAllSavesQueryHandler(ISaveGameService saveGameService)
    {
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
    }

    public async Task<IReadOnlyList<SaveGameMetadata>> HandleAsync(GetAllSavesQuery query, CancellationToken cancellationToken = default)
    {
        var saves = _saveGameService.GetAllSaves();
        return await Task.FromResult(saves);
    }
}
