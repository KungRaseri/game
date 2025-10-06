namespace Game.UI.Models;

/// <summary>
/// Interface for toast operations used by command handlers
/// </summary>
public interface IToastOperations
{
    /// <summary>
    /// Shows a toast with the specified configuration
    /// </summary>
    /// <param name="config">The toast configuration</param>
    Task ShowToastAsync(ToastConfig config);

    /// <summary>
    /// Dismisses a specific toast by ID
    /// </summary>
    /// <param name="toastId">The unique identifier of the toast to dismiss</param>
    Task DismissToastAsync(string toastId);

    /// <summary>
    /// Dismisses all currently active toasts
    /// </summary>
    Task ClearAllToastsAsync();

    /// <summary>
    /// Gets all currently active toasts
    /// </summary>
    /// <returns>A list of active toast information</returns>
    List<ToastInfo> GetActiveToasts();

    /// <summary>
    /// Gets a specific toast by ID
    /// </summary>
    /// <param name="toastId">The unique identifier of the toast</param>
    /// <returns>The toast information if found, otherwise null</returns>
    ToastInfo? GetToastById(string toastId);

    /// <summary>
    /// Gets the total count of active toasts
    /// </summary>
    /// <returns>The number of currently active toasts</returns>
    int GetActiveToastCount();

    /// <summary>
    /// Checks if a toast with the specified ID exists
    /// </summary>
    /// <param name="toastId">The unique identifier of the toast</param>
    /// <returns>True if the toast exists, otherwise false</returns>
    bool ToastExists(string toastId);

    /// <summary>
    /// Gets toasts by anchor position
    /// </summary>
    /// <param name="anchor">The anchor position to filter by</param>
    /// <returns>A list of toasts at the specified anchor position</returns>
    List<ToastInfo> GetToastsByAnchor(ToastAnchor anchor);

    /// <summary>
    /// Checks if the toast limit has been reached
    /// </summary>
    /// <returns>True if the toast limit is reached, otherwise false</returns>
    bool IsToastLimitReached();
}
