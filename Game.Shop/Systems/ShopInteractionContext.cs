namespace Game.Shop.Systems;

/// <summary>
/// Context information about the current shop interaction.
/// </summary>
public class ShopInteractionContext
{
    public float InteractionQualityScore { get; set; } = 0.5f;
    public float ShopReputationScore { get; set; } = 0.5f;
    public float ShopAmbianceScore { get; set; } = 0.5f;
    public float OtherCustomersSatisfaction { get; set; } = 0.5f;
    public bool AlternativeItemsAvailable { get; set; } = true;
    public int TotalInteractions { get; set; } = 0;
    public bool DiscountOffered { get; set; } = false;
    public bool NegotiationAttempted { get; set; } = false;
}