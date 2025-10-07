namespace Game.UI.Models;

/// <summary>
/// Simplified toast data for the CQS system
/// </summary>
public record ToastData
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public float DurationSeconds { get; init; } = 3.0f;
    public bool IsPersistent { get; init; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}