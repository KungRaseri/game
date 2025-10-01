#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Main.Models;
using Game.Main.Utils;

namespace Game.Main.Models;

/// <summary>
/// Types of customers that visit the shop, each with distinct behaviors and preferences.
/// </summary>
public enum CustomerType
{
    /// <summary>New adventurers with limited budgets and basic equipment needs.</summary>
    NoviceAdventurer = 0,
    
    /// <summary>Experienced adventurers with moderate budgets who value quality.</summary>
    VeteranAdventurer = 1,
    
    /// <summary>Wealthy patrons who prefer luxury items and don't mind high prices.</summary>
    NoblePatron = 2,
    
    /// <summary>Merchants focused on bulk purchases for resale opportunities.</summary>
    MerchantTrader = 3,
    
    /// <summary>Regular townspeople making occasional purchases, very price-sensitive.</summary>
    CasualTownsperson = 4
}

/// <summary>
/// Represents the customer's interest level in a specific item.
/// Used by AI to determine purchase likelihood.
/// </summary>
public enum CustomerInterest
{
    /// <summary>No interest - will not purchase this item.</summary>
    NotInterested = 0,
    
    /// <summary>Slight interest - might purchase if price is very good.</summary>
    SlightlyInterested = 1,
    
    /// <summary>Moderate interest - will consider purchasing at fair price.</summary>
    ModeratelyInterested = 2,
    
    /// <summary>High interest - likely to purchase unless overpriced.</summary>
    HighlyInterested = 3,
    
    /// <summary>Very high interest - will purchase unless extremely overpriced.</summary>
    VeryInterested = 4
}

/// <summary>
/// Represents a customer's purchase decision after evaluating an item.
/// </summary>
public enum PurchaseDecision
{
    /// <summary>Will not purchase the item.</summary>
    NotBuying = 0,
    
    /// <summary>Will purchase the item at asking price.</summary>
    Buying = 1,
    
    /// <summary>Wants to negotiate for a lower price first.</summary>
    WantsToNegotiate = 2,
    
    /// <summary>Will consider purchasing if specific conditions are met.</summary>
    Considering = 3
}

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

/// <summary>
/// Represents a customer's budget and spending capacity.
/// </summary>
public record Budget
{
    /// <summary>Minimum amount the customer is willing to spend.</summary>
    public int MinSpendingPower { get; init; }
    
    /// <summary>Maximum amount the customer can afford to spend.</summary>
    public int MaxSpendingPower { get; init; }
    
    /// <summary>Typical purchase range for this customer.</summary>
    public int TypicalPurchaseRange { get; init; }
    
    /// <summary>Current available funds for this shopping session.</summary>
    public int CurrentFunds { get; init; }
    
    /// <summary>
    /// Creates a budget appropriate for the given customer type.
    /// </summary>
    public static Budget CreateForType(CustomerType type)
    {
        var random = new Random();
        
        return type switch
        {
            CustomerType.NoviceAdventurer => new Budget
            {
                MinSpendingPower = 10,
                MaxSpendingPower = 100,
                TypicalPurchaseRange = 50,
                CurrentFunds = 25 + random.Next(50) // 25-75 gold
            },
            
            CustomerType.VeteranAdventurer => new Budget
            {
                MinSpendingPower = 50,
                MaxSpendingPower = 500,
                TypicalPurchaseRange = 200,
                CurrentFunds = 150 + random.Next(300) // 150-450 gold
            },
            
            CustomerType.NoblePatron => new Budget
            {
                MinSpendingPower = 200,
                MaxSpendingPower = 2000,
                TypicalPurchaseRange = 800,
                CurrentFunds = 500 + random.Next(1000) // 500-1500 gold
            },
            
            CustomerType.MerchantTrader => new Budget
            {
                MinSpendingPower = 100,
                MaxSpendingPower = 1000,
                TypicalPurchaseRange = 400,
                CurrentFunds = 200 + random.Next(600) // 200-800 gold
            },
            
            CustomerType.CasualTownsperson => new Budget
            {
                MinSpendingPower = 5,
                MaxSpendingPower = 75,
                TypicalPurchaseRange = 25,
                CurrentFunds = 10 + random.Next(40) // 10-50 gold
            },
            
            _ => new Budget
            {
                MinSpendingPower = 10,
                MaxSpendingPower = 100,
                TypicalPurchaseRange = 50,
                CurrentFunds = 50
            }
        };
    }
    
    /// <summary>
    /// Checks if the customer can afford the given price.
    /// </summary>
    public bool CanAfford(decimal price)
    {
        return (decimal)CurrentFunds >= price;
    }
    
    /// <summary>
    /// Determines if the price is within the customer's comfortable spending range.
    /// </summary>
    public bool IsComfortablePrice(decimal price)
    {
        return price <= (decimal)TypicalPurchaseRange;
    }
    
    /// <summary>
    /// Gets the maximum negotiation offer this customer would make.
    /// </summary>
    public decimal GetMaxNegotiationOffer(decimal askingPrice)
    {
        // Customers typically won't negotiate above 90% of asking price
        var maxOffer = Math.Min((decimal)CurrentFunds, askingPrice * 0.9m);
        
        // But they also won't go below their typical range if they can help it
        if (maxOffer < (decimal)TypicalPurchaseRange * 0.5m)
        {
            maxOffer = Math.Min((decimal)CurrentFunds, (decimal)TypicalPurchaseRange * 0.7m);
        }
        
        return maxOffer;
    }
}
