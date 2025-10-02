using Game.Main.Models;

namespace Game.Main.Systems;

/// <summary>
/// Enhanced purchase decision with detailed analysis and reasoning.
/// </summary>
public class EnhancedPurchaseDecision
{
    public required PurchaseDecision Decision { get; init; }
    public required float Confidence { get; init; }
    public required string PrimaryReason { get; init; }
    public required List<string> SecondaryFactors { get; init; }
    public required float NegotiationWillingness { get; init; }
    public required float AlternativeInterest { get; init; }
    public required CustomerEmotionalResponse EmotionalResponse { get; init; }
    public required string SuggestedAction { get; init; }
}