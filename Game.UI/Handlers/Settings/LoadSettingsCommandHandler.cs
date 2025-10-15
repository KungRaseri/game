#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands.Settings;
using Game.UI.Models;
using Game.UI.Systems;

namespace Game.UI.Handlers.Settings;

/// <summary>
/// Handles the LoadSettingsCommand by loading settings from disk.
/// </summary>
public class LoadSettingsCommandHandler : ICommandHandler<LoadSettingsCommand>
{
    private readonly ISettingsService _settingsService;

    public LoadSettingsCommandHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task HandleAsync(LoadSettingsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Info("Loading settings from disk...");

            // Load settings from disk
            _settingsService.LoadSettings();

            // Apply immediately if requested
            if (command.ApplyImmediately)
            {
                var settings = _settingsService.GetAllSettings();
                _settingsService.ApplyAllSettings(settings);
            }

            GameLogger.Info("Settings loaded successfully");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to load settings");
            throw;
        }

        await Task.CompletedTask;
    }
}
