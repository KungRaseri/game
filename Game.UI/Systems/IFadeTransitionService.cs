#nullable enable

using Game.UI.Commands.Scenes;

namespace Game.UI.Systems;

/// <summary>
/// Interface for fade transition operations.
/// </summary>
public interface IFadeTransitionService
{
    /// <summary>
    /// Executes a fade transition effect.
    /// </summary>
    Task ExecuteFadeAsync(FadeTransitionCommand command);

    /// <summary>
    /// Checks if a fade transition is currently in progress.
    /// </summary>
    bool IsFading();

    /// <summary>
    /// Cancels any current fade transition.
    /// </summary>
    void CancelCurrentFade();

    /// <summary>
    /// Sets the fade state instantly without animation.
    /// </summary>
    void SetFadeState(bool visible);
}
