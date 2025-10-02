namespace Game.Main.Models;

/// <summary>
/// Represents a competitor's reaction to player actions or market changes.
/// </summary>
public class CompetitorReaction
{
    /// <summary>Which competitor is reacting.</summary>
    public required string CompetitorId { get; init; }
    
    /// <summary>Item type being affected by the reaction.</summary>
    public ItemType ItemType { get; init; }
    
    /// <summary>Quality tier being affected.</summary>
    public QualityTier QualityTier { get; init; }
    
    /// <summary>Type of reaction being taken.</summary>
    public CompetitorReactionType ReactionType { get; set; }
    
    /// <summary>Price multiplier applied as part of the reaction.</summary>
    public double PriceMultiplier { get; set; } = 1.0;
    
    /// <summary>When this reaction occurred.</summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;
    
    /// <summary>Description of why this reaction occurred.</summary>
    public string? Reason { get; init; }
}