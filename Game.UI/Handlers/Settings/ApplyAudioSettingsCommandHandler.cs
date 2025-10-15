#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands.Settings;
using Game.UI.Models;
using Game.UI.Systems;

namespace Game.UI.Handlers.Settings;

/// <summary>
/// Handles the ApplyAudioSettingsCommand by applying audio settings to the audio system.
/// </summary>
public class ApplyAudioSettingsCommandHandler : ICommandHandler<ApplyAudioSettingsCommand>
{
    private readonly ISettingsService _settingsService;

    public ApplyAudioSettingsCommandHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task HandleAsync(ApplyAudioSettingsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Info("Applying audio settings...");

            var audioSettings = new AudioSettingsData
            {
                MasterVolume = command.MasterVolume,
                MusicVolume = command.MusicVolume,
                SfxVolume = command.SfxVolume
            };

            _settingsService.ApplyAudioSettings(audioSettings);

            GameLogger.Info("Audio settings applied successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to apply audio settings");
            throw;
        }

        await Task.CompletedTask;
    }
}
