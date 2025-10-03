#nullable enable

using Game.Core.Models;
using Game.Main.Utils;

namespace Game.Main.Systems;

/// <summary>
/// Enhanced AI system for customer decision-making with improved intelligence and interaction capabilities.
/// Provides sophisticated purchase decisions, negotiation strategies, and realistic customer behavior.
/// </summary>
public class EnhancedCustomerAI
{
    private readonly Customer _customer;
    private readonly Random _random;
    private readonly Dictionary<string, int> _itemViewCounts;
    private readonly Dictionary<string, DateTime> _lastViewTimes;
    
    // Decision factors and weights
    private const float PriceWeight = 0.35f;
    private const float QualityWeight = 0.25f;
    private const float PersonalityWeight = 0.20f;
    private const float ShopReputationWeight = 0.10f;
    private const float InteractionWeight = 0.10f;
    
    public EnhancedCustomerAI(Customer customer)
    {
        _customer = customer ?? throw new ArgumentNullException(nameof(customer));
        _random = new Random();
        _itemViewCounts = new Dictionary<string, int>();
        _lastViewTimes = new Dictionary<string, DateTime>();
    }
    
    /// <summary>
    /// Makes an enhanced purchase decision considering all factors including recent interactions.
    /// </summary>
    public EnhancedPurchaseDecision MakeEnhancedPurchaseDecision(Item item, decimal price, ShopInteractionContext context)
    {
        var baseDecision = _customer.MakePurchaseDecision(item, price);
        
        // Enhance the decision with additional AI factors
        var enhancedFactors = AnalyzeDecisionFactors(item, price, context);
        var finalDecision = ApplyEnhancedLogic(baseDecision, enhancedFactors);
        
        // Update internal tracking
        TrackItemInteraction(item);
        
        // Generate detailed reasoning
        var reasoning = GenerateDecisionReasoning(item, price, enhancedFactors, finalDecision);
        
        GameLogger.Info($"Enhanced AI decision for {_customer.Name}: {finalDecision} - {reasoning.PrimaryReason}");
        
        return new EnhancedPurchaseDecision
        {
            Decision = finalDecision,
            Confidence = enhancedFactors.OverallConfidence,
            PrimaryReason = reasoning.PrimaryReason,
            SecondaryFactors = reasoning.SecondaryFactors,
            NegotiationWillingness = enhancedFactors.NegotiationLikelihood,
            AlternativeInterest = enhancedFactors.AlternativeInterest,
            EmotionalResponse = DetermineEmotionalResponse(enhancedFactors),
            SuggestedAction = DetermineSuggestedPlayerAction(finalDecision, enhancedFactors)
        };
    }
    
    /// <summary>
    /// Analyzes multiple decision factors to create a comprehensive decision profile.
    /// </summary>
    private DecisionFactors AnalyzeDecisionFactors(Item item, decimal price, ShopInteractionContext context)
    {
        var factors = new DecisionFactors();
        
        // 1. Price Analysis
        factors.PriceScore = AnalyzePriceAcceptability(item, price);
        factors.AffordabilityScore = CalculateAffordabilityScore(price);
        
        // 2. Quality and Value Analysis
        factors.QualityScore = AnalyzeQualityPreference(item);
        factors.ValueScore = CalculatePerceivedValue(item, price);
        
        // 3. Personality-Based Factors
        factors.ImpulseScore = CalculateImpulsePurchaseScore(item);
        factors.PatientScore = CalculatePatientShoppingScore();
        factors.LoyaltyInfluence = CalculateLoyaltyInfluence(context);
        
        // 4. Social and Interaction Factors
        factors.InteractionSatisfaction = CalculateInteractionSatisfaction(context);
        factors.ShopAmbianceScore = CalculateShopAmbianceScore(context);
        factors.PeerInfluence = CalculatePeerInfluence(context);
        
        // 5. Strategic Factors
        factors.NegotiationLikelihood = CalculateNegotiationLikelihood(item, price);
        factors.AlternativeInterest = CalculateAlternativeInterest(context);
        factors.TimeConstraint = CalculateTimeConstraint();
        
        // 6. Calculate overall confidence
        factors.OverallConfidence = CalculateOverallConfidence(factors);
        
        return factors;
    }
    
    /// <summary>
    /// Applies enhanced logic to refine the base purchase decision.
    /// </summary>
    private PurchaseDecision ApplyEnhancedLogic(PurchaseDecision baseDecision, DecisionFactors factors)
    {
        // If base decision is strong (Buying), check if we should still negotiate
        if (baseDecision == PurchaseDecision.Buying && factors.NegotiationLikelihood > 0.6f)
        {
            return PurchaseDecision.WantsToNegotiate;
        }
        
        // If base decision is weak (NotBuying), check if positive interactions changed mind
        if (baseDecision == PurchaseDecision.NotBuying && factors.InteractionSatisfaction > 0.8f)
        {
            // Good interaction might make customer reconsider
            if (factors.AffordabilityScore > 0.7f)
            {
                return PurchaseDecision.Considering;
            }
        }
        
        // If considering, check confidence to make final decision
        if (baseDecision == PurchaseDecision.Considering)
        {
            if (factors.OverallConfidence > 0.75f)
            {
                return factors.NegotiationLikelihood > 0.5f ? PurchaseDecision.WantsToNegotiate : PurchaseDecision.Buying;
            }
            else if (factors.OverallConfidence < 0.3f)
            {
                return PurchaseDecision.NotBuying;
            }
        }
        
        return baseDecision;
    }
    
    /// <summary>
    /// Generates detailed reasoning for the purchase decision.
    /// </summary>
    private DecisionReasoning GenerateDecisionReasoning(Item item, decimal price, DecisionFactors factors, PurchaseDecision decision)
    {
        var reasons = new List<string>();
        
        // Determine primary reason based on strongest factor
        var factorScores = new Dictionary<string, float>
        {
            ["price"] = factors.PriceScore,
            ["quality"] = factors.QualityScore,
            ["value"] = factors.ValueScore,
            ["interaction"] = factors.InteractionSatisfaction,
            ["impulse"] = factors.ImpulseScore
        };
        
        var primaryFactor = factorScores.OrderByDescending(kvp => kvp.Value).First();
        
        var primaryReason = primaryFactor.Key switch
        {
            "price" when factors.PriceScore > 0.7f => "The price is very reasonable for what I need",
            "price" when factors.PriceScore < 0.3f => "Too expensive for my budget",
            "quality" when factors.QualityScore > 0.7f => "The quality is exactly what I'm looking for",
            "quality" when factors.QualityScore < 0.3f => "The quality doesn't meet my standards",
            "value" when factors.ValueScore > 0.7f => "Great value for the price",
            "interaction" when factors.InteractionSatisfaction > 0.7f => "The shopkeeper was very helpful",
            "impulse" when factors.ImpulseScore > 0.7f => "I just really want this item",
            _ => "It seems like a good choice overall"
        };
        
        // Add secondary factors
        if (factors.AffordabilityScore < 0.5f)
            reasons.Add("budget constraints");
        if (factors.LoyaltyInfluence > 0.6f)
            reasons.Add("trust in this shop");
        if (factors.TimeConstraint > 0.7f)
            reasons.Add("need to decide quickly");
        if (factors.NegotiationLikelihood > 0.6f)
            reasons.Add("might negotiate for better price");
            
        return new DecisionReasoning
        {
            PrimaryReason = primaryReason,
            SecondaryFactors = reasons
        };
    }
    
    /// <summary>
    /// Determines the customer's emotional response to the shopping experience.
    /// </summary>
    private CustomerEmotionalResponse DetermineEmotionalResponse(DecisionFactors factors)
    {
        var positiveScore = (factors.QualityScore + factors.InteractionSatisfaction + factors.ShopAmbianceScore) / 3f;
        var stressScore = (1f - factors.AffordabilityScore) + (1f - factors.OverallConfidence);
        
        if (positiveScore > 0.8f && stressScore < 0.3f)
            return CustomerEmotionalResponse.Delighted;
        else if (positiveScore > 0.6f && stressScore < 0.5f)
            return CustomerEmotionalResponse.Satisfied;
        else if (positiveScore > 0.4f && stressScore < 0.6f)
            return CustomerEmotionalResponse.Neutral;
        else if (positiveScore < 0.4f && stressScore > 0.6f)
            return CustomerEmotionalResponse.Frustrated;
        else if (positiveScore < 0.3f && stressScore > 0.7f)
            return CustomerEmotionalResponse.Upset;
        else
            return CustomerEmotionalResponse.Conflicted;
    }
    
    /// <summary>
    /// Suggests the best action for the player to take based on customer state.
    /// </summary>
    private string DetermineSuggestedPlayerAction(PurchaseDecision decision, DecisionFactors factors)
    {
        return decision switch
        {
            PurchaseDecision.Buying when factors.OverallConfidence > 0.9f => "Complete the sale - customer is ready to buy!",
            PurchaseDecision.Buying => "Finalize the sale, but be prepared for last-minute questions",
            PurchaseDecision.WantsToNegotiate => $"Customer wants to negotiate - they might accept {factors.NegotiationLikelihood * 100:F0}% of asking price",
            PurchaseDecision.Considering when factors.InteractionSatisfaction < 0.5f => "Try engaging the customer with helpful information",
            PurchaseDecision.Considering when factors.QualityScore < 0.5f => "Show the customer higher quality alternatives",
            PurchaseDecision.Considering when factors.PriceScore < 0.5f => "Consider offering a discount or showing cheaper alternatives",
            PurchaseDecision.NotBuying when factors.AlternativeInterest > 0.6f => "Show different items that might interest them",
            PurchaseDecision.NotBuying when factors.InteractionSatisfaction > 0.6f => "Customer likes you but not the item - try alternatives",
            _ => "Let the customer browse - they need more time to decide"
        };
    }
    
    // Helper methods for calculating specific factors
    private float AnalyzePriceAcceptability(Item item, decimal price) =>
        _customer.BudgetRange.CanAfford(price) ? 
            Math.Max(0f, 1f - (float)price / _customer.BudgetRange.MaxSpendingPower) : 0f;
    
    private float CalculateAffordabilityScore(decimal price) =>
        Math.Min(1f, (float)_customer.BudgetRange.MaxSpendingPower / (float)price);
    
    private float AnalyzeQualityPreference(Item item) =>
        _customer.Preferences.GetQualityPreference(item.Quality);
    
    private float CalculatePerceivedValue(Item item, decimal price)
    {
        var expectedValue = _customer.BudgetRange.TypicalPurchaseRange * 
                           (1f + _customer.Preferences.GetQualityPreference(item.Quality));
        return Math.Min(1f, expectedValue / (float)price);
    }
    
    private float CalculateImpulsePurchaseScore(Item item) =>
        _customer.Personality.ImpulsePurchasing * (_random.NextSingle() * 0.5f + 0.5f);
    
    private float CalculatePatientShoppingScore() =>
        1f - _customer.Personality.ImpulsePurchasing;
    
    private float CalculateLoyaltyInfluence(ShopInteractionContext context) =>
        _customer.Loyalty.LoyaltyScore * 0.2f + context.ShopReputationScore;
    
    private float CalculateInteractionSatisfaction(ShopInteractionContext context) =>
        context.InteractionQualityScore;
    
    private float CalculateShopAmbianceScore(ShopInteractionContext context) =>
        context.ShopAmbianceScore * _customer.Personality.AestheticAppreciation;
    
    private float CalculatePeerInfluence(ShopInteractionContext context) =>
        context.OtherCustomersSatisfaction * 0.3f; // Mild social influence
    
    private float CalculateNegotiationLikelihood(Item item, decimal price) =>
        _customer.Personality.NegotiationTendency * 
        (1f - CalculateAffordabilityScore(price)) * 0.5f + 
        _customer.Personality.NegotiationTendency * 0.5f;
    
    private float CalculateAlternativeInterest(ShopInteractionContext context) =>
        context.AlternativeItemsAvailable ? 0.7f : 0.2f;
    
    private float CalculateTimeConstraint() =>
        _random.NextSingle() * 0.3f + 0.1f; // Simulated time pressure
    
    private float CalculateOverallConfidence(DecisionFactors factors) =>
        (factors.PriceScore * PriceWeight +
         factors.QualityScore * QualityWeight +
         factors.InteractionSatisfaction * InteractionWeight +
         factors.ValueScore * PersonalityWeight +
         factors.LoyaltyInfluence * ShopReputationWeight);
    
    private void TrackItemInteraction(Item item)
    {
        _itemViewCounts[item.ItemId] = _itemViewCounts.GetValueOrDefault(item.ItemId, 0) + 1;
        _lastViewTimes[item.ItemId] = DateTime.Now;
    }
}