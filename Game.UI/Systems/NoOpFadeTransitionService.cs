#nullable enable

using Game.Core.Utils;
using Game.UI.Commands;

namespace Game.UI.Systems;

/// <summary>
/// No-operation implementation of IFadeTransitionService for dependency injection.
/// This serves as a fallback when no real fade transition component is available.
/// </summary>
public class NoOpFadeTransitionService : IFadeTransitionService
{
    /// <summary>
    /// Executes a fake fade transition that completes immediately.
    /// </summary>
    /// <param name="command">The fade transition command to execute</param>
    /// <returns>A completed task</returns>
    public Task ExecuteFadeAsync(FadeTransitionCommand command)
    {
        GameLogger.Debug($"NoOp fade transition: {command.FadeType} (duration: {command.Duration}s)");
        
        // Return a completed task - no actual fade occurs
        return Task.CompletedTask;
    }

    /// <summary>
    /// Always returns false since no fade is ever in progress.
    /// </summary>
    public bool IsFading()
    {
        return false;
    }

    /// <summary>
    /// No-op implementation - nothing to cancel.
    /// </summary>
    public void CancelCurrentFade()
    {
        GameLogger.Debug("NoOp cancel fade transition");
    }

    /// <summary>
    /// No-op implementation - no visual state to set.
    /// </summary>
    public void SetFadeState(bool visible)
    {
        GameLogger.Debug($"NoOp set fade state: {visible}");
    }
}
