#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands;
using Game.UI.Models;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles the SaveSettingsCommand by saving settings to disk.
/// </summary>
public class SaveSettingsCommandHandler : ICommandHandler<SaveSettingsCommand>
{
    private readonly ISettingsService _settingsService;

    public SaveSettingsCommandHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task HandleAsync(SaveSettingsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Info("Saving settings to disk...");

            var settings = new SettingsData
            {
                MasterVolume = command.MasterVolume,
                MusicVolume = command.MusicVolume,
                SfxVolume = command.SfxVolume,
                Fullscreen = command.Fullscreen
            };

            var success = _settingsService.SaveSettings(settings);
            
            if (success)
            {
                GameLogger.Info("Settings saved successfully");
            }
            else
            {
                GameLogger.Error("Failed to save settings");
            }
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Exception while saving settings");
            throw;
        }

        await Task.CompletedTask;
    }
}
