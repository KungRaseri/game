#nullable enable

using Game.Core.CQS;
using Game.UI.Queries;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles the HasSaveFilesQuery by checking if save files exist.
/// </summary>
public class HasSaveFilesQueryHandler : IQueryHandler<HasSaveFilesQuery, bool>
{
    private readonly ISaveGameService _saveGameService;

    public HasSaveFilesQueryHandler(ISaveGameService saveGameService)
    {
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
    }

    public async Task<bool> HandleAsync(HasSaveFilesQuery query, CancellationToken cancellationToken = default)
    {
        var hasSaves = _saveGameService.HasAnySaveFiles();
        return await Task.FromResult(hasSaves);
    }
}
