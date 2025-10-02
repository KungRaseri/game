#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Main.Models;
using Game.Main.Utils;

namespace Game.Main.Models;

/// <summary>
/// Represents a customer visiting the shop with AI-driven behavior and decision-making.
/// Each customer has unique personality traits, preferences, and purchasing patterns.
/// </summary>
public class Customer
{
    private readonly Random _random;
    private readonly List<string> _viewedItems;
    private readonly Dictionary<string, CustomerInterest> _itemInterests;
    
    /// <summary>Unique identifier for this customer.</summary>
    public string CustomerId { get; }
    
    /// <summary>Customer's display name.</summary>
    public string Name { get; }
    
    /// <summary>Type of customer determining base behavior patterns.</summary>
    public CustomerType Type { get; }
    
    /// <summary>Personality traits influencing decision-making.</summary>
    public CustomerPersonality Personality { get; }
    
    /// <summary>Budget and spending capacity.</summary>
    public Budget BudgetRange { get; }
    
    /// <summary>Item type preferences for this customer.</summary>
    public ItemPreferences Preferences { get; }
    
    /// <summary>Current relationship level with the shop.</summary>
    public CustomerLoyalty Loyalty { get; private set; }
    
    /// <summary>When this customer entered the shop.</summary>
    public DateTime EntryTime { get; }
    
    /// <summary>Items this customer has looked at during this visit.</summary>
    public IReadOnlyList<string> ViewedItems => _viewedItems.AsReadOnly();
    
    /// <summary>Current state of the customer's shopping process.</summary>
    public CustomerState CurrentState { get; private set; } = CustomerState.Browsing;
    
    /// <summary>Item currently being considered for purchase.</summary>
    public Item? ItemBeingConsidered { get; private set; }
    
    /// <summary>Current thoughts or reactions (for UI display).</summary>
    public string CurrentThought { get; private set; } = "";
    
    public Customer(CustomerType type, string? customName = null)
    {
        _random = new Random();
        _viewedItems = new List<string>();
        _itemInterests = new Dictionary<string, CustomerInterest>();
        
        CustomerId = Guid.NewGuid().ToString();
        Name = customName ?? GenerateCustomerName(type);
        Type = type;
        Personality = CustomerPersonality.CreateForType(type);
        BudgetRange = Budget.CreateForType(type);
        Preferences = ItemPreferences.CreateForType(type);
        Loyalty = CustomerLoyalty.CreateNew();
        EntryTime = DateTime.Now;
        
        CurrentThought = GenerateEntryThought();
        
        GameLogger.Debug($"Customer {Name} ({Type}) entered shop with {BudgetRange.CurrentFunds} gold");
    }
    
    /// <summary>
    /// Evaluates the customer's interest in a specific item.
    /// </summary>
    public CustomerInterest EvaluateItem(Item item, decimal price)
    {
        // Check if already evaluated
        if (_itemInterests.TryGetValue(item.ItemId, out var cachedInterest))
        {
            return cachedInterest;
        }
        
        // Track that customer viewed this item
        if (!_viewedItems.Contains(item.ItemId))
        {
            _viewedItems.Add(item.ItemId);
        }
        
        var interest = CalculateInterest(item, price);
        _itemInterests[item.ItemId] = interest;
        
        UpdateThoughtBasedOnItem(item, price, interest);
        
        GameLogger.Debug($"Customer {Name} evaluated {item.Name}: {interest} (price: {price}g)");
        
        return interest;
    }
    
    /// <summary>
    /// Makes a purchase decision for a specific item after evaluation.
    /// </summary>
    public PurchaseDecision MakePurchaseDecision(Item item, decimal price)
    {
        var interest = EvaluateItem(item, price);
        
        // Set the item being considered
        ItemBeingConsidered = item;
        CurrentState = CustomerState.Considering;
        
        var decision = DeterminePurchaseDecision(item, price, interest);
        
        UpdateThoughtBasedOnDecision(item, price, decision);
        
        GameLogger.Info($"Customer {Name} decision for {item.Name}: {decision}");
        
        return decision;
    }
    
    /// <summary>
    /// Generates a negotiation offer if the customer wants to haggle.
    /// </summary>
    public decimal? AttemptNegotiation(Item item, decimal askingPrice)
    {
        if (Personality.NegotiationTendency < 0.3f)
        {
            return null; // Customer doesn't like to negotiate
        }
        
        var maxOffer = BudgetRange.GetMaxNegotiationOffer(askingPrice);
        
        // Calculate negotiation offer based on personality and interest
        var interest = _itemInterests.GetValueOrDefault(item.ItemId, CustomerInterest.NotInterested);
        var interestMultiplier = interest switch
        {
            CustomerInterest.VeryInterested => 0.9f,     // Offer up to 90% of asking
            CustomerInterest.HighlyInterested => 0.8f,   // Offer up to 80% of asking
            CustomerInterest.ModeratelyInterested => 0.7f, // Offer up to 70% of asking
            CustomerInterest.SlightlyInterested => 0.6f,  // Offer up to 60% of asking
            _ => 0.5f // Offer up to 50% of asking
        };
        
        var offer = Math.Min(maxOffer, askingPrice * (decimal)interestMultiplier);
        
        // Add some personality-based variation
        var negotiationSkill = Personality.NegotiationTendency;
        offer += (decimal)(_random.NextSingle() - 0.5f) * askingPrice * 0.1m * (decimal)negotiationSkill;
        
        // Ensure offer doesn't exceed budget or asking price
        offer = Math.Max(1m, Math.Min(offer, Math.Min(maxOffer, askingPrice * 0.95m)));
        
        CurrentThought = $"How about {offer:F0} gold instead?";
        CurrentState = CustomerState.Negotiating;
        
        GameLogger.Info($"Customer {Name} negotiated {askingPrice}g -> {offer}g for {item.Name}");
        
        return offer;
    }
    
    /// <summary>
    /// Reacts to the shop owner's response to negotiation.
    /// </summary>
    public bool RespondToNegotiation(decimal counterOffer)
    {
        if (ItemBeingConsidered == null)
        {
            return false;
        }
        
        var canAfford = BudgetRange.CanAfford(counterOffer);
        var isReasonable = counterOffer <= BudgetRange.GetMaxNegotiationOffer(counterOffer * 1.2m);
        
        var acceptance = canAfford && isReasonable;
        
        if (acceptance)
        {
            CurrentThought = "That seems fair!";
            CurrentState = CustomerState.ReadyToBuy;
        }
        else
        {
            CurrentThought = canAfford ? "Still too expensive..." : "I can't afford that much.";
            CurrentState = CustomerState.NotInterested;
        }
        
        GameLogger.Debug($"Customer {Name} response to {counterOffer}g: {(acceptance ? "accepted" : "declined")}");
        
        return acceptance;
    }
    
    /// <summary>
    /// Completes a purchase and updates customer satisfaction.
    /// </summary>
    public CustomerSatisfaction CompletePurchase(Item item, decimal finalPrice)
    {
        if (ItemBeingConsidered?.ItemId != item.ItemId)
        {
            throw new InvalidOperationException("Customer is not considering this item for purchase");
        }
        
        CurrentState = CustomerState.Purchasing;
        
        var satisfaction = CalculateSatisfaction(item, finalPrice);
        
        // Update loyalty based on satisfaction
        Loyalty = Loyalty.UpdateAfterPurchase(satisfaction);
        
        CurrentThought = GeneratePurchaseThought(satisfaction);
        
        // Complete the purchase
        ItemBeingConsidered = null;
        CurrentState = CustomerState.Satisfied;
        
        GameLogger.Info($"Customer {Name} purchased {item.Name} for {finalPrice}g - {satisfaction}");
        
        return satisfaction;
    }
    
    /// <summary>
    /// Customer leaves without purchasing anything.
    /// </summary>
    public CustomerSatisfaction LeaveWithoutPurchase(string reason = "Nothing caught my interest")
    {
        CurrentState = CustomerState.Leaving;
        CurrentThought = reason;
        
        // Leaving without purchase slightly reduces loyalty
        var satisfaction = ViewedItems.Count > 0 ? CustomerSatisfaction.Neutral : CustomerSatisfaction.Unsatisfied;
        Loyalty = Loyalty.UpdateAfterVisit(satisfaction);
        
        GameLogger.Info($"Customer {Name} left without purchase: {reason}");
        
        return satisfaction;
    }
    
    /// <summary>
    /// Updates the customer's current thought (for external systems like shopping sessions).
    /// </summary>
    public void UpdateThought(string thought)
    {
        CurrentThought = thought;
    }
    
    private CustomerInterest CalculateInterest(Item item, decimal price)
    {
        var baseInterest = 0f;
        
        // Item type preference
        var typePreference = Preferences.GetTypePreference(item.ItemType);
        baseInterest += typePreference * 40f; // 0-40 points
        
        // Quality preference
        var qualityPreference = Preferences.GetQualityPreference(item.Quality);
        baseInterest += qualityPreference * 30f; // 0-30 points
        
        // Price evaluation
        var priceScore = EvaluatePrice(item, price);
        baseInterest += priceScore * 30f; // 0-30 points (or negative if overpriced)
        
        // Add some randomness for realism
        baseInterest += (_random.NextSingle() - 0.5f) * 20f; // ±10 points
        
        // Convert to interest level
        return baseInterest switch
        {
            >= 80f => CustomerInterest.VeryInterested,
            >= 60f => CustomerInterest.HighlyInterested,
            >= 40f => CustomerInterest.ModeratelyInterested,
            >= 20f => CustomerInterest.SlightlyInterested,
            _ => CustomerInterest.NotInterested
        };
    }
    
    private float EvaluatePrice(Item item, decimal price)
    {
        // Check affordability
        if (!BudgetRange.CanAfford(price))
        {
            return -50f; // Strong negative for unaffordable items
        }
        
        // Calculate value perception based on customer type and personality
        var expectedValue = CalculateExpectedValue(item);
        var priceRatio = (float)price / expectedValue;
        
        // Price sensitivity affects how much price matters
        var sensitivity = Personality.PriceSensitivity;
        
        return priceRatio switch
        {
            <= 0.7f => 1.0f - sensitivity * 0.3f,        // Great deal
            <= 0.9f => 1.0f - sensitivity * 0.1f,        // Good price
            <= 1.1f => 1.0f - sensitivity * 0.5f,        // Fair price
            <= 1.3f => 0.5f - sensitivity * 0.7f,        // Expensive
            <= 1.5f => 0.2f - sensitivity * 0.8f,        // Very expensive
            _ => -0.5f - sensitivity                      // Overpriced
        };
    }
    
    private float CalculateExpectedValue(Item item)
    {
        // Base values by type (what customer expects to pay)
        var baseValue = item.ItemType switch
        {
            ItemType.Weapon => 60f,
            ItemType.Armor => 80f,
            ItemType.Material => 15f,
            ItemType.Consumable => 20f,
            _ => 40f
        };
        
        // Quality multiplier
        var qualityMultiplier = item.Quality switch
        {
            QualityTier.Common => 1.0f,
            QualityTier.Uncommon => 1.4f,
            QualityTier.Rare => 2.0f,
            QualityTier.Epic => 3.0f,
            QualityTier.Legendary => 4.5f,
            _ => 1.0f
        };
        
        // Customer type affects perceived value
        var customerMultiplier = Type switch
        {
            CustomerType.NoblePatron => 1.5f,      // Nobles expect to pay more
            CustomerType.VeteranAdventurer => 1.2f, // Veterans know quality costs
            CustomerType.MerchantTrader => 0.8f,    // Merchants want wholesale prices
            CustomerType.CasualTownsperson => 0.9f, // Townspeople want deals
            CustomerType.NoviceAdventurer => 0.7f,  // Novices expect cheap prices
            _ => 1.0f
        };
        
        return baseValue * qualityMultiplier * customerMultiplier;
    }
    
    private PurchaseDecision DeterminePurchaseDecision(Item item, decimal price, CustomerInterest interest)
    {
        // Not interested = no purchase
        if (interest == CustomerInterest.NotInterested)
        {
            return PurchaseDecision.NotBuying;
        }
        
        // Can't afford = no purchase
        if (!BudgetRange.CanAfford(price))
        {
            return PurchaseDecision.NotBuying;
        }
        
        // Very interested + can afford = likely to buy or negotiate
        if (interest == CustomerInterest.VeryInterested)
        {
            if (BudgetRange.IsComfortablePrice(price) || Personality.ImpulsePurchasing > 0.6f)
            {
                return PurchaseDecision.Buying;
            }
            else if (Personality.NegotiationTendency > 0.4f)
            {
                return PurchaseDecision.WantsToNegotiate;
            }
            else
            {
                return PurchaseDecision.Considering;
            }
        }
        
        // Moderate to high interest
        if (interest >= CustomerInterest.ModeratelyInterested)
        {
            var priceComfort = BudgetRange.IsComfortablePrice(price);
            var impulseThreshold = 0.7f - (float)interest * 0.1f;
            
            if (priceComfort && (Personality.ImpulsePurchasing > impulseThreshold || _random.NextSingle() < 0.3f))
            {
                return PurchaseDecision.Buying;
            }
            else if (Personality.NegotiationTendency > 0.5f && !priceComfort)
            {
                return PurchaseDecision.WantsToNegotiate;
            }
            else
            {
                return PurchaseDecision.Considering;
            }
        }
        
        // Slight interest - only if very good deal or impulsive
        if (interest == CustomerInterest.SlightlyInterested)
        {
            if (BudgetRange.IsComfortablePrice(price) && 
                (Personality.ImpulsePurchasing > 0.8f || _random.NextSingle() < 0.1f))
            {
                return PurchaseDecision.Buying;
            }
            else if (Personality.NegotiationTendency > 0.7f)
            {
                return PurchaseDecision.WantsToNegotiate;
            }
        }
        
        return PurchaseDecision.NotBuying;
    }
    
    private CustomerSatisfaction CalculateSatisfaction(Item item, decimal finalPrice)
    {
        var satisfaction = 50f; // Base satisfaction
        
        // Interest in item affects satisfaction
        var interest = _itemInterests.GetValueOrDefault(item.ItemId, CustomerInterest.ModeratelyInterested);
        satisfaction += (float)interest * 10f; // 0-40 points
        
        // Price satisfaction
        var expectedValue = CalculateExpectedValue(item);
        var priceRatio = (float)finalPrice / expectedValue;
        
        satisfaction += priceRatio switch
        {
            <= 0.8f => 30f,  // Great deal
            <= 0.95f => 15f, // Good price
            <= 1.05f => 0f,  // Fair price
            <= 1.2f => -15f, // Expensive
            _ => -30f         // Overpriced
        };
        
        // Shop aesthetics (if customer cares)
        satisfaction += Personality.AestheticAppreciation * 10f; // 0-10 points
        
        // Add some randomness
        satisfaction += (_random.NextSingle() - 0.5f) * 20f; // ±10 points
        
        return satisfaction switch
        {
            >= 80f => CustomerSatisfaction.VerySatisfied,
            >= 60f => CustomerSatisfaction.Satisfied,
            >= 40f => CustomerSatisfaction.Neutral,
            >= 20f => CustomerSatisfaction.Unsatisfied,
            _ => CustomerSatisfaction.VeryUnsatisfied
        };
    }
    
    private void UpdateThoughtBasedOnItem(Item item, decimal price, CustomerInterest interest)
    {
        CurrentThought = interest switch
        {
            CustomerInterest.VeryInterested => $"I really want this {item.Name}!",
            CustomerInterest.HighlyInterested => $"This {item.Name} looks good...",
            CustomerInterest.ModeratelyInterested => $"Hmm, {price}g for this {item.Name}?",
            CustomerInterest.SlightlyInterested => $"Not sure about this {item.Name}...",
            _ => $"This {item.Name} isn't for me."
        };
    }
    
    private void UpdateThoughtBasedOnDecision(Item item, decimal price, PurchaseDecision decision)
    {
        CurrentThought = decision switch
        {
            PurchaseDecision.Buying => $"I'll take this {item.Name}!",
            PurchaseDecision.WantsToNegotiate => $"Can we negotiate on this {item.Name}?",
            PurchaseDecision.Considering => $"Let me think about this {item.Name}...",
            _ => $"I don't think I need this {item.Name}."
        };
    }
    
    private string GenerateEntryThought()
    {
        var thoughts = Type switch
        {
            CustomerType.NoviceAdventurer => new[] { "I need better gear...", "Hope I can find something affordable", "Let's see what they have" },
            CustomerType.VeteranAdventurer => new[] { "Looking for quality equipment", "I know good gear when I see it", "What's worth buying here?" },
            CustomerType.NoblePatron => new[] { "I want only the finest", "Money is no object", "Show me your best items" },
            CustomerType.MerchantTrader => new[] { "What can I resell?", "Looking for good deals", "Any bulk discounts?" },
            CustomerType.CasualTownsperson => new[] { "Just browsing around", "Maybe something useful?", "Don't need much..." },
            _ => new[] { "Let's have a look around" }
        };
        
        return thoughts[_random.Next(thoughts.Length)];
    }
    
    private string GeneratePurchaseThought(CustomerSatisfaction satisfaction)
    {
        return satisfaction switch
        {
            CustomerSatisfaction.VerySatisfied => "Excellent! Exactly what I needed!",
            CustomerSatisfaction.Satisfied => "Good purchase, I'm happy with this.",
            CustomerSatisfaction.Neutral => "This will do fine.",
            CustomerSatisfaction.Unsatisfied => "Not quite what I hoped for...",
            CustomerSatisfaction.VeryUnsatisfied => "I think I overpaid for this...",
            _ => "Thanks for the transaction."
        };
    }
    
    private string GenerateCustomerName(CustomerType type)
    {
        var random = new Random();
        
        var firstNames = new[] { "Alex", "Morgan", "Casey", "Jordan", "Taylor", "Riley", "Avery", "Quinn", "Sage", "Rowan" };
        var lastNames = type switch
        {
            CustomerType.NoviceAdventurer => new[] { "Greenhorn", "Hopeful", "Eager", "Brave", "Young" },
            CustomerType.VeteranAdventurer => new[] { "Ironwill", "Battleborn", "Seasoned", "Steelhart", "Veteran" },
            CustomerType.NoblePatron => new[] { "Goldleaf", "Silverton", "Richmont", "Nobleheart", "Highborn" },
            CustomerType.MerchantTrader => new[] { "Coinsworth", "Tradwell", "Merchant", "Dealer", "Profitable" },
            CustomerType.CasualTownsperson => new[] { "Common", "Simple", "Ordinary", "Townfolk", "Regular" },
            _ => new[] { "Visitor" }
        };
        
        return $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
    }
}

/// <summary>
/// Represents the current state of a customer's shopping process.
/// </summary>
public enum CustomerState
{
    /// <summary>Customer is looking around the shop.</summary>
    Browsing,
    
    /// <summary>Customer is examining a specific item.</summary>
    Examining,
    
    /// <summary>Customer is thinking about purchasing an item.</summary>
    Considering,
    
    /// <summary>Customer is attempting to negotiate price.</summary>
    Negotiating,
    
    /// <summary>Customer has decided to buy and is ready for transaction.</summary>
    ReadyToBuy,
    
    /// <summary>Customer is completing the purchase.</summary>
    Purchasing,
    
    /// <summary>Customer has completed purchase and is satisfied.</summary>
    Satisfied,
    
    /// <summary>Customer has lost interest in current item.</summary>
    NotInterested,
    
    /// <summary>Customer is leaving the shop.</summary>
    Leaving
}
