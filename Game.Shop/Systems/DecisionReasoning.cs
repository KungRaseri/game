namespace Game.Shop.Systems;

/// <summary>
/// Detailed reasoning behind a purchase decision.
/// </summary>
public class DecisionReasoning
{
    public required string PrimaryReason { get; set; }
    public required List<string> SecondaryFactors { get; set; }
}