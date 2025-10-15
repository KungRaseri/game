#nullable enable

using Game.Core.CQS;
using Game.UI.Models;
using Game.UI.Queries;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles the GetAllSettingsQuery by retrieving all settings.
/// </summary>
public class GetAllSettingsQueryHandler : IQueryHandler<GetAllSettingsQuery, SettingsData>
{
    private readonly ISettingsService _settingsService;

    public GetAllSettingsQueryHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task<SettingsData> HandleAsync(GetAllSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.GetAllSettings();
        return await Task.FromResult(settings);
    }
}
