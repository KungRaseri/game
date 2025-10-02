#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Main.Models;
using Game.Main.Utils;

namespace Game.Main.Models;

/// <summary>
/// Personality traits that influence customer behavior and decision-making.
/// Each trait ranges from 0.0 to 1.0 and affects different aspects of behavior.
/// </summary>
public record CustomerPersonality
{
    /// <summary>How sensitive the customer is to prices (0.0 = doesn't care, 1.0 = very price-conscious).</summary>
    public float PriceSensitivity { get; init; } = 0.5f;
    
    /// <summary>How much the customer values quality over price (0.0 = price-focused, 1.0 = quality-focused).</summary>
    public float QualityFocus { get; init; } = 0.5f;
    
    /// <summary>Likelihood to attempt price negotiation (0.0 = never, 1.0 = always tries).</summary>
    public float NegotiationTendency { get; init; } = 0.3f;
    
    /// <summary>Potential for becoming a repeat customer (0.0 = one-time, 1.0 = very loyal).</summary>
    public float LoyaltyPotential { get; init; } = 0.5f;
    
    /// <summary>Tendency to make quick purchases without much deliberation (0.0 = careful, 1.0 = impulsive).</summary>
    public float ImpulsePurchasing { get; init; } = 0.2f;
    
    /// <summary>How much the customer is influenced by shop aesthetics (0.0 = doesn't care, 1.0 = very influenced).</summary>
    public float AestheticAppreciation { get; init; } = 0.4f;
    
    /// <summary>
    /// Creates personality traits for a specific customer type with appropriate defaults.
    /// </summary>
    public static CustomerPersonality CreateForType(CustomerType type)
    {
        var random = new Random();
        
        return type switch
        {
            CustomerType.NoviceAdventurer => new CustomerPersonality
            {
                PriceSensitivity = 0.7f + random.NextSingle() * 0.2f, // 0.7-0.9
                QualityFocus = 0.2f + random.NextSingle() * 0.3f,     // 0.2-0.5
                NegotiationTendency = 0.4f + random.NextSingle() * 0.3f, // 0.4-0.7
                LoyaltyPotential = 0.6f + random.NextSingle() * 0.3f,  // 0.6-0.9
                ImpulsePurchasing = 0.1f + random.NextSingle() * 0.2f, // 0.1-0.3
                AestheticAppreciation = 0.2f + random.NextSingle() * 0.3f // 0.2-0.5
            },
            
            CustomerType.VeteranAdventurer => new CustomerPersonality
            {
                PriceSensitivity = 0.4f + random.NextSingle() * 0.3f, // 0.4-0.7
                QualityFocus = 0.6f + random.NextSingle() * 0.3f,     // 0.6-0.9
                NegotiationTendency = 0.2f + random.NextSingle() * 0.3f, // 0.2-0.5
                LoyaltyPotential = 0.5f + random.NextSingle() * 0.4f,  // 0.5-0.9
                ImpulsePurchasing = 0.3f + random.NextSingle() * 0.3f, // 0.3-0.6
                AestheticAppreciation = 0.4f + random.NextSingle() * 0.4f // 0.4-0.8
            },
            
            CustomerType.NoblePatron => new CustomerPersonality
            {
                PriceSensitivity = 0.1f + random.NextSingle() * 0.3f, // 0.1-0.4
                QualityFocus = 0.8f + random.NextSingle() * 0.2f,     // 0.8-1.0
                NegotiationTendency = 0.0f + random.NextSingle() * 0.2f, // 0.0-0.2
                LoyaltyPotential = 0.3f + random.NextSingle() * 0.4f,  // 0.3-0.7
                ImpulsePurchasing = 0.5f + random.NextSingle() * 0.4f, // 0.5-0.9
                AestheticAppreciation = 0.7f + random.NextSingle() * 0.3f // 0.7-1.0
            },
            
            CustomerType.MerchantTrader => new CustomerPersonality
            {
                PriceSensitivity = 0.8f + random.NextSingle() * 0.2f, // 0.8-1.0
                QualityFocus = 0.4f + random.NextSingle() * 0.3f,     // 0.4-0.7
                NegotiationTendency = 0.6f + random.NextSingle() * 0.4f, // 0.6-1.0
                LoyaltyPotential = 0.7f + random.NextSingle() * 0.3f,  // 0.7-1.0
                ImpulsePurchasing = 0.0f + random.NextSingle() * 0.2f, // 0.0-0.2
                AestheticAppreciation = 0.1f + random.NextSingle() * 0.3f // 0.1-0.4
            },
            
            CustomerType.CasualTownsperson => new CustomerPersonality
            {
                PriceSensitivity = 0.6f + random.NextSingle() * 0.3f, // 0.6-0.9
                QualityFocus = 0.3f + random.NextSingle() * 0.4f,     // 0.3-0.7
                NegotiationTendency = 0.3f + random.NextSingle() * 0.4f, // 0.3-0.7
                LoyaltyPotential = 0.4f + random.NextSingle() * 0.4f,  // 0.4-0.8
                ImpulsePurchasing = 0.4f + random.NextSingle() * 0.4f, // 0.4-0.8
                AestheticAppreciation = 0.3f + random.NextSingle() * 0.5f // 0.3-0.8
            },
            
            _ => new CustomerPersonality() // Default personality
        };
    }
    
    /// <summary>
    /// Gets a description of this personality for debugging and display.
    /// </summary>
    public string GetDescription()
    {
        var traits = new List<string>();
        
        if (PriceSensitivity > 0.7f) traits.Add("price-conscious");
        if (QualityFocus > 0.7f) traits.Add("quality-focused");
        if (NegotiationTendency > 0.6f) traits.Add("likes to haggle");
        if (LoyaltyPotential > 0.7f) traits.Add("potentially loyal");
        if (ImpulsePurchasing > 0.6f) traits.Add("impulsive buyer");
        if (AestheticAppreciation > 0.7f) traits.Add("aesthetically minded");
        
        return traits.Count > 0 ? string.Join(", ", traits) : "balanced personality";
    }
}