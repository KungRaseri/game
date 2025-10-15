#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands;
using Game.UI.Models;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles the ApplyDisplaySettingsCommand by applying display settings to the display system.
/// </summary>
public class ApplyDisplaySettingsCommandHandler : ICommandHandler<ApplyDisplaySettingsCommand>
{
    private readonly ISettingsService _settingsService;

    public ApplyDisplaySettingsCommandHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task HandleAsync(ApplyDisplaySettingsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Info("Applying display settings...");

            var displaySettings = new DisplaySettingsData
            {
                Fullscreen = command.Fullscreen
            };

            _settingsService.ApplyDisplaySettings(displaySettings);

            GameLogger.Info("Display settings applied successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to apply display settings");
            throw;
        }

        await Task.CompletedTask;
    }
}
