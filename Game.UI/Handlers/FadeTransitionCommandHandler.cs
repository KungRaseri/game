#nullable enable

using Game.Core.CQS;
using Game.Core.Utils;
using Game.UI.Commands;
using Game.UI.Systems;

namespace Game.UI.Handlers;

/// <summary>
/// Handles fade transition commands by coordinating with the fade transition service.
/// </summary>
public class FadeTransitionCommandHandler : ICommandHandler<FadeTransitionCommand>
{
    private readonly IFadeTransitionService _fadeTransitionService;

    public FadeTransitionCommandHandler(IFadeTransitionService fadeTransitionService)
    {
        _fadeTransitionService = fadeTransitionService ?? throw new ArgumentNullException(nameof(fadeTransitionService));
    }

    public async Task HandleAsync(FadeTransitionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogger.Debug($"Handling fade transition command: {command.FadeType}");

            // Execute the fade transition through the service
            await _fadeTransitionService.ExecuteFadeAsync(command);

            GameLogger.Debug($"Fade transition completed: {command.FadeType}");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Failed to handle fade transition command: {command.FadeType}");
            throw;
        }
    }
}
