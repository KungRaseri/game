#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands;
using Game.UI.Models;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles the ApplySettingsCommand by applying all settings to the system.
/// </summary>
public class ApplySettingsCommandHandler : ICommandHandler<ApplySettingsCommand>
{
    private readonly ISettingsService _settingsService;

    public ApplySettingsCommandHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task HandleAsync(ApplySettingsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Info("Applying all settings...");

            var settings = new SettingsData
            {
                MasterVolume = command.MasterVolume,
                MusicVolume = command.MusicVolume,
                SfxVolume = command.SfxVolume,
                Fullscreen = command.Fullscreen
            };

            // Apply settings to system
            _settingsService.ApplyAllSettings(settings);

            // Save to disk if requested
            if (command.SaveToDisk)
            {
                _settingsService.SaveSettings(settings);
            }

            GameLogger.Info("Settings applied successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to apply settings");
            throw;
        }

        await Task.CompletedTask;
    }
}
