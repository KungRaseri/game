#nullable enable

using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles the GetAudioSettingsQuery by retrieving audio settings.
/// </summary>
public class GetAudioSettingsQueryHandler : IQueryHandler<GetAudioSettingsQuery, AudioSettingsData>
{
    private readonly ISettingsService _settingsService;

    public GetAudioSettingsQueryHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task<AudioSettingsData> HandleAsync(GetAudioSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var audioSettings = _settingsService.GetAudioSettings();
        return await Task.FromResult(audioSettings);
    }
}
