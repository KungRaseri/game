#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Main.Models;
using Game.Main.Utils;

namespace Game.Main.Systems;

/// <summary>
/// Orchestrates a complete customer shopping experience from entry to exit.
/// Manages the interaction between Customer AI and ShopManager for realistic shopping flows.
/// </summary>
public class CustomerShoppingSession
{
    private readonly Customer _customer;
    private readonly ShopManager _shopManager;
    private readonly Random _random;
    private readonly bool _isTestMode;
    
    /// <summary>The customer participating in this shopping session.</summary>
    public Customer Customer => _customer;
    
    /// <summary>Whether the customer is currently active in the shop.</summary>
    public bool IsActive { get; private set; }
    
    /// <summary>Current phase of the shopping session.</summary>
    public ShoppingPhase CurrentPhase { get; private set; } = ShoppingPhase.Entering;
    
    /// <summary>Total time spent in the shop.</summary>
    public TimeSpan SessionDuration => DateTime.Now - _customer.EntryTime;
    
    /// <summary>Items the customer has examined during this session.</summary>
    public IReadOnlyList<(Item Item, CustomerInterest Interest)> ExaminedItems { get; private set; } = 
        new List<(Item, CustomerInterest)>();
    
    /// <summary>The transaction result if purchase was completed.</summary>
    public SaleTransaction? CompletedTransaction { get; private set; }
    
    /// <summary>Final customer satisfaction when leaving.</summary>
    public CustomerSatisfaction FinalSatisfaction { get; private set; } = CustomerSatisfaction.Neutral;
    
    // Events for UI and analytics
    public event Action<Customer, ShoppingPhase>? PhaseChanged;
    public event Action<Customer, Item, CustomerInterest>? ItemExamined;
    public event Action<Customer, Item, decimal>? NegotiationStarted;
    public event Action<Customer, decimal, bool>? NegotiationResult;
    public event Action<Customer, SaleTransaction>? PurchaseCompleted;
    public event Action<Customer, CustomerSatisfaction, string>? SessionEnded;
    
    public CustomerShoppingSession(Customer customer, ShopManager shopManager, bool isTestMode = false)
    {
        _customer = customer ?? throw new ArgumentNullException(nameof(customer));
        _shopManager = shopManager ?? throw new ArgumentNullException(nameof(shopManager));
        _random = new Random();
        _isTestMode = isTestMode;
        IsActive = true;
        
        GameLogger.Info($"Started shopping session for {customer.Name} ({customer.Type})");
    }
    
    /// <summary>
    /// Runs the complete customer shopping experience asynchronously.
    /// </summary>
    public async Task<CustomerSatisfaction> RunShoppingSessionAsync()
    {
        try
        {
            await EnterShopAsync();
            await BrowseItemsAsync();
            
            if (CurrentPhase != ShoppingPhase.Leaving)
            {
                await ConsiderPurchasesAsync();
            }
            
            await ExitShopAsync();
            
            return FinalSatisfaction;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, $"Error during shopping session for {_customer.Name}");
            await ForceExitAsync("Technical difficulties");
            return CustomerSatisfaction.Angry;
        }
        finally
        {
            IsActive = false;
        }
    }
    
    /// <summary>
    /// Customer enters the shop and looks around.
    /// </summary>
    private async Task EnterShopAsync()
    {
        ChangePhase(ShoppingPhase.Entering);
        
        // Brief pause as customer enters and gets oriented
        await SimulateThinkingTime(1000, 2000);
        
        ChangePhase(ShoppingPhase.Browsing);
        GameLogger.Debug($"Customer {_customer.Name} entered shop and started browsing");
    }
    
    /// <summary>
    /// Customer browses available items and evaluates interest.
    /// </summary>
    private async Task BrowseItemsAsync()
    {
        var displayedItems = _shopManager.DisplaySlots
            .Where(slot => slot.IsOccupied && slot.CurrentItem != null)
            .Select(slot => (slot.CurrentItem!, slot.CurrentPrice, slot.SlotId))
            .ToList();
        
        if (!displayedItems.Any())
        {
            GameLogger.Info($"Customer {_customer.Name} found no items to browse");
            ChangePhase(ShoppingPhase.Leaving);
            return;
        }
        
        var examinedItems = new List<(Item, CustomerInterest)>();
        
        // Customer examines items based on their personality and preferences
        var itemsToExamine = DetermineItemsToExamine(displayedItems);
        
        foreach (var (item, price, slotId) in itemsToExamine)
        {
            ChangePhase(ShoppingPhase.Examining);
            
            // Customer evaluates the item
            var interest = _customer.EvaluateItem(item, price);
            examinedItems.Add((item, interest));
            
            ItemExamined?.Invoke(_customer, item, interest);
            
            GameLogger.Debug($"Customer {_customer.Name} examined {item.Name}: {interest}");
            
            // Simulate examination time based on interest level
            var examinationTime = interest switch
            {
                CustomerInterest.VeryInterested => (3000, 5000),
                CustomerInterest.HighlyInterested => (2000, 4000),
                CustomerInterest.ModeratelyInterested => (1500, 3000),
                CustomerInterest.SlightlyInterested => (1000, 2000),
                _ => (500, 1000)
            };
            
            await SimulateThinkingTime(examinationTime.Item1, examinationTime.Item2);
            
            // If customer is very interested, they might focus on this item
            if (interest >= CustomerInterest.HighlyInterested && _random.NextSingle() < 0.7f)
            {
                break; // Focus on this item instead of continuing to browse
            }
        }
        
        ExaminedItems = examinedItems.AsReadOnly();
        
        // Return to browsing state
        ChangePhase(ShoppingPhase.Browsing);
    }
    
    /// <summary>
    /// Customer considers purchases for items they're interested in.
    /// </summary>
    private async Task ConsiderPurchasesAsync()
    {
        var interestedItems = ExaminedItems
            .Where(item => item.Interest >= CustomerInterest.SlightlyInterested)
            .OrderByDescending(item => item.Interest)
            .ToList();
        
        if (!interestedItems.Any())
        {
            GameLogger.Info($"Customer {_customer.Name} wasn't interested in any items");
            _customer.UpdateThought("Nothing here catches my eye...");
            ChangePhase(ShoppingPhase.Leaving);
            return;
        }
        
        // Try to purchase the most interesting item
        var (mostInterestingItem, _) = interestedItems.First();
        var displaySlot = _shopManager.DisplaySlots.FirstOrDefault(
            slot => slot.IsOccupied && slot.CurrentItem?.ItemId == mostInterestingItem.ItemId);
        
        if (displaySlot == null)
        {
            GameLogger.Warning($"Customer {_customer.Name} interested in item not found in display");
            ChangePhase(ShoppingPhase.Leaving);
            return;
        }
        
        await AttemptPurchaseAsync(mostInterestingItem, displaySlot.CurrentPrice, displaySlot.SlotId);
    }
    
    /// <summary>
    /// Customer attempts to purchase a specific item, including potential negotiation.
    /// </summary>
    private async Task AttemptPurchaseAsync(Item item, decimal askingPrice, int slotId)
    {
        ChangePhase(ShoppingPhase.Considering);
        
        // Customer makes purchase decision
        var decision = _customer.MakePurchaseDecision(item, askingPrice);
        
        await SimulateThinkingTime(2000, 4000);
        
        switch (decision)
        {
            case PurchaseDecision.Buying:
                await CompletePurchaseAsync(item, askingPrice, slotId);
                break;
                
            case PurchaseDecision.WantsToNegotiate:
                await HandleNegotiationAsync(item, askingPrice, slotId);
                break;
                
            case PurchaseDecision.Considering:
                await HandleConsiderationAsync(item, askingPrice, slotId);
                break;
                
            case PurchaseDecision.NotBuying:
            default:
                GameLogger.Info($"Customer {_customer.Name} decided not to buy {item.Name}");
                _customer.UpdateThought("I don't think I need this right now.");
                ChangePhase(ShoppingPhase.Leaving);
                break;
        }
    }
    
    /// <summary>
    /// Handles customer negotiation attempts.
    /// </summary>
    private async Task HandleNegotiationAsync(Item item, decimal askingPrice, int slotId)
    {
        ChangePhase(ShoppingPhase.Negotiating);
        
        var negotiationOffer = _customer.AttemptNegotiation(item, askingPrice);
        
        if (negotiationOffer == null)
        {
            // Customer changed their mind about negotiating
            ChangePhase(ShoppingPhase.Leaving);
            return;
        }
        
        NegotiationStarted?.Invoke(_customer, item, negotiationOffer.Value);
        
        await SimulateThinkingTime(1000, 2000);
        
        // Simple negotiation resolution (in a real game, this would involve player input)
        var acceptsNegotiation = DetermineNegotiationAcceptance(askingPrice, negotiationOffer.Value);
        var finalPrice = acceptsNegotiation ? negotiationOffer.Value : askingPrice;
        
        NegotiationResult?.Invoke(_customer, finalPrice, acceptsNegotiation);
        
        if (acceptsNegotiation)
        {
            var customerAccepts = _customer.RespondToNegotiation(finalPrice);
            if (customerAccepts)
            {
                await CompletePurchaseAsync(item, finalPrice, slotId);
            }
            else
            {
                GameLogger.Info($"Customer {_customer.Name} rejected counter-offer of {finalPrice}g");
                ChangePhase(ShoppingPhase.Leaving);
            }
        }
        else
        {
            // Customer might still buy at asking price if they really want it
            if (_customer.RespondToNegotiation(askingPrice))
            {
                await CompletePurchaseAsync(item, askingPrice, slotId);
            }
            else
            {
                GameLogger.Info($"Customer {_customer.Name} left after failed negotiation");
                ChangePhase(ShoppingPhase.Leaving);
            }
        }
    }
    
    /// <summary>
    /// Handles customer taking time to consider a purchase.
    /// </summary>
    private async Task HandleConsiderationAsync(Item item, decimal askingPrice, int slotId)
    {
        _customer.UpdateThought($"Let me think about this {item.Name}...");
        
        // Longer thinking time for consideration
        await SimulateThinkingTime(3000, 6000);
        
        // Make final decision after consideration
        var finalDecision = _random.NextSingle() < 0.6f ? PurchaseDecision.Buying : PurchaseDecision.NotBuying;
        
        if (finalDecision == PurchaseDecision.Buying)
        {
            _customer.UpdateThought("Alright, I'll take it.");
            await CompletePurchaseAsync(item, askingPrice, slotId);
        }
        else
        {
            _customer.UpdateThought("I'll think about it some more...");
            ChangePhase(ShoppingPhase.Leaving);
        }
    }
    
    /// <summary>
    /// Completes the purchase transaction.
    /// </summary>
    private async Task CompletePurchaseAsync(Item item, decimal finalPrice, int slotId)
    {
        ChangePhase(ShoppingPhase.Purchasing);
        
        // Process the transaction through ShopManager
        var satisfaction = _customer.CompletePurchase(item, finalPrice);
        var transaction = _shopManager.ProcessSale(slotId, _customer.CustomerId, satisfaction);
        
        if (transaction != null)
        {
            CompletedTransaction = transaction;
            FinalSatisfaction = satisfaction;
            
            PurchaseCompleted?.Invoke(_customer, transaction);
            
            GameLogger.Info($"Purchase completed: {_customer.Name} bought {item.Name} for {finalPrice}g (satisfaction: {satisfaction})");
            
            // Brief satisfaction/checkout time
            await SimulateThinkingTime(1000, 2000);
        }
        else
        {
            GameLogger.Error($"Failed to process transaction for {_customer.Name} buying {item.Name}");
            _customer.UpdateThought("There seems to be a problem...");
            FinalSatisfaction = CustomerSatisfaction.Angry;
        }
        
        ChangePhase(ShoppingPhase.Leaving);
    }
    
    /// <summary>
    /// Customer exits the shop.
    /// </summary>
    private async Task ExitShopAsync()
    {
        ChangePhase(ShoppingPhase.Leaving);
        
        if (CompletedTransaction == null)
        {
            FinalSatisfaction = _customer.LeaveWithoutPurchase();
        }
        
        // Brief exit time
        await SimulateThinkingTime(500, 1000);
        
        var exitReason = CompletedTransaction != null ? "Purchase completed" : "No purchase made";
        SessionEnded?.Invoke(_customer, FinalSatisfaction, exitReason);
        
        GameLogger.Info($"Customer {_customer.Name} left shop - {exitReason} (satisfaction: {FinalSatisfaction})");
    }
    
    /// <summary>
    /// Forces customer to exit due to error or special circumstances.
    /// </summary>
    private async Task ForceExitAsync(string reason)
    {
        ChangePhase(ShoppingPhase.Leaving);
        FinalSatisfaction = CustomerSatisfaction.Disappointed;
        _customer.UpdateThought(reason);
        
        await SimulateThinkingTime(200, 500);
        
        SessionEnded?.Invoke(_customer, FinalSatisfaction, reason);
        GameLogger.Warning($"Customer {_customer.Name} force-exited: {reason}");
    }
    
    private void ChangePhase(ShoppingPhase newPhase)
    {
        if (CurrentPhase != newPhase)
        {
            CurrentPhase = newPhase;
            PhaseChanged?.Invoke(_customer, newPhase);
        }
    }
    
    private List<(Item Item, decimal Price, int SlotId)> DetermineItemsToExamine(
        List<(Item Item, decimal Price, int SlotId)> availableItems)
    {
        // Customer examines items based on their preferences and personality
        var itemsToExamine = new List<(Item, decimal, int)>();
        
        // Sort items by customer preference
        var sortedItems = availableItems
            .OrderByDescending(item => _customer.Preferences.GetTypePreference(item.Item.ItemType))
            .ThenByDescending(item => _customer.Preferences.GetQualityPreference(item.Item.Quality))
            .ToList();
        
        // Determine how many items to examine based on customer type
        var maxItemsToExamine = _customer.Type switch
        {
            CustomerType.NoblePatron => Math.Min(4, sortedItems.Count),      // Nobles examine more items
            CustomerType.VeteranAdventurer => Math.Min(3, sortedItems.Count), // Veterans are selective
            CustomerType.MerchantTrader => sortedItems.Count,                // Merchants check everything
            CustomerType.NoviceAdventurer => Math.Min(2, sortedItems.Count), // Novices don't browse much
            CustomerType.CasualTownsperson => Math.Min(2, sortedItems.Count), // Casual browsers
            _ => Math.Min(2, sortedItems.Count)
        };
        
        // Add some randomness to browsing patterns
        var examineCount = Math.Max(1, maxItemsToExamine + _random.Next(-1, 2));
        examineCount = Math.Min(examineCount, sortedItems.Count);
        
        return sortedItems.Take(examineCount).ToList();
    }
    
    private bool DetermineNegotiationAcceptance(decimal askingPrice, decimal offer)
    {
        // Simple negotiation logic (in real game, this would be player decision)
        var discount = (askingPrice - offer) / askingPrice;
        
        // Accept negotiations up to 20% discount
        return discount <= 0.2m;
    }
    
    private async Task SimulateThinkingTime(int minMs, int maxMs)
    {
        if (_isTestMode)
        {
            // Skip delays in test mode for faster tests
            return;
        }
        
        var delay = _random.Next(minMs, maxMs);
        await Task.Delay(delay);
    }
}