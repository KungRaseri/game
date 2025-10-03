namespace Game.Core.Models;

/// <summary>
/// Represents an action taken by an AI competitor.
/// </summary>
public class CompetitorAction
{
    /// <summary>Which competitor took this action.</summary>
    public required string CompetitorId { get; init; }
    
    /// <summary>Type of action taken.</summary>
    public required CompetitorActionType ActionType { get; init; }
    
    /// <summary>When the action was taken.</summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;
    
    /// <summary>Description of the action.</summary>
    public required string Description { get; init; }
    
    /// <summary>Items affected by this action, if applicable.</summary>
    public List<(ItemType, QualityTier)> AffectedItems { get; init; } = new();
    
    /// <summary>Magnitude of the action's impact (0.0-1.0).</summary>
    public double ImpactMagnitude { get; init; } = 0.5;
}