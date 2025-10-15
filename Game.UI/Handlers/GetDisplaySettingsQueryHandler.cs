#nullable enable

using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles the GetDisplaySettingsQuery by retrieving display settings.
/// </summary>
public class GetDisplaySettingsQueryHandler : IQueryHandler<GetDisplaySettingsQuery, DisplaySettingsData>
{
    private readonly ISettingsService _settingsService;

    public GetDisplaySettingsQueryHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task<DisplaySettingsData> HandleAsync(GetDisplaySettingsQuery query, CancellationToken cancellationToken = default)
    {
        var displaySettings = _settingsService.GetDisplaySettings();
        return await Task.FromResult(displaySettings);
    }
}
