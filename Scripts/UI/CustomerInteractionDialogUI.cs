#nullable enable

using Game.Core.Models;
using Game.Main.Systems;
using Game.Main.Utils;
using Godot;

namespace Game.Scripts.UI;

/// <summary>
/// UI for detailed customer interactions including negotiation, preferences, and purchase decisions.
/// Provides an immersive interface for engaging with customers during their shopping experience.
/// </summary>
public partial class CustomerInteractionDialogUI : AcceptDialog
{
    [Signal]
    public delegate void CustomerActionTakenEventHandler(string customerId, string action, string itemId);

    // UI References
    private Label? _customerName;
    private Label? _customerType;
    private Label? _budget;
    private Label? _mood;
    private RichTextLabel? _customerThoughts;
    private Label? _itemName;
    private Label? _itemPrice;
    
    private Button? _offerDiscountButton;
    private Button? _negotiateButton;
    private Button? _showAlternativeButton;
    private Button? _closeButton;
    
    // State
    private Customer? _currentCustomer;
    private Item? _currentItem;
    private ShopManager? _shopManager;
    private EnhancedCustomerAI? _customerAI;
    private ShopInteractionContext _interactionContext = new();
    private Random _random = new();
    
    public override void _Ready()
    {
        GameLogger.SetBackend(new GodotLoggerBackend());
        CacheUIReferences();
        ConnectButtons();
        
        // Set initial dialog properties
        Title = "Customer Interaction";
        GetOkButton().Text = "Close";
        
        GameLogger.Debug("CustomerInteractionDialog initialized");
    }
    
    private void CacheUIReferences()
    {
        _customerName = GetNode<Label>("VBoxContainer/CustomerInfo/CustomerDetails/CustomerName");
        _customerType = GetNode<Label>("VBoxContainer/CustomerInfo/CustomerDetails/CustomerType");
        _budget = GetNode<Label>("VBoxContainer/CustomerInfo/CustomerDetails/Budget");
        _mood = GetNode<Label>("VBoxContainer/CustomerInfo/CustomerDetails/Mood");
        _customerThoughts = GetNode<RichTextLabel>("VBoxContainer/DialogContent/CustomerThoughts");
        _itemName = GetNode<Label>("VBoxContainer/DialogContent/ItemOfInterest/ItemName");
        _itemPrice = GetNode<Label>("VBoxContainer/DialogContent/ItemOfInterest/ItemPrice");
        
        _offerDiscountButton = GetNode<Button>("VBoxContainer/ActionButtons/OfferDiscountButton");
        _negotiateButton = GetNode<Button>("VBoxContainer/ActionButtons/NegotiateButton");
        _showAlternativeButton = GetNode<Button>("VBoxContainer/ActionButtons/ShowAlternativeButton");
        _closeButton = GetNode<Button>("VBoxContainer/ActionButtons/CloseButton");
    }
    
    private void ConnectButtons()
    {
        if (_offerDiscountButton != null)
            _offerDiscountButton.Pressed += OnOfferDiscountPressed;
            
        if (_negotiateButton != null)
            _negotiateButton.Pressed += OnNegotiatePressed;
            
        if (_showAlternativeButton != null)
            _showAlternativeButton.Pressed += OnShowAlternativePressed;
            
        if (_closeButton != null)
            _closeButton.Pressed += OnClosePressed;
    }
    
    /// <summary>
    /// Shows the customer interaction dialog for a specific customer and item.
    /// </summary>
    public void ShowCustomerInteraction(Customer customer, Item? item, ShopManager shopManager)
    {
        _currentCustomer = customer;
        _currentItem = item;
        _shopManager = shopManager;
        
        // Initialize Enhanced AI for this customer
        _customerAI = new EnhancedCustomerAI(customer);
        
        // Initialize interaction context
        _interactionContext = new ShopInteractionContext
        {
            InteractionQualityScore = 0.5f,
            ShopReputationScore = 0.7f, // Base shop reputation
            ShopAmbianceScore = 0.6f,
            OtherCustomersSatisfaction = 0.5f,
            AlternativeItemsAvailable = shopManager.DisplaySlots.Count(slot => slot != null && slot.CurrentItem != null) > 1,
            TotalInteractions = 0,
            DiscountOffered = false,
            NegotiationAttempted = false
        };
        
        UpdateCustomerDisplay();
        UpdateItemDisplay();
        UpdateCustomerThoughts();
        UpdateActionButtons();
        
        PopupCentered();
        GameLogger.Info($"Showing enhanced customer interaction for {customer.Name}");
    }
    
    private void UpdateCustomerDisplay()
    {
        if (_currentCustomer == null) return;
        
        if (_customerName != null)
            _customerName.Text = _currentCustomer.Name;
            
        if (_customerType != null)
            _customerType.Text = $"{GetCustomerTypeDisplay(_currentCustomer.Type)}";
            
        if (_budget != null)
            _budget.Text = $"Budget: {_currentCustomer.BudgetRange.MinSpendingPower}-{_currentCustomer.BudgetRange.MaxSpendingPower}g";
            
        if (_mood != null)
            _mood.Text = $"Mood: {GetCustomerMoodDisplay()}";
    }
    
    private void UpdateItemDisplay()
    {
        if (_currentItem != null)
        {
            if (_itemName != null)
                _itemName.Text = _currentItem.Name;
                
            if (_itemPrice != null)
                _itemPrice.Text = $"{_currentItem.Value}g";
        }
        else
        {
            if (_itemName != null)
                _itemName.Text = "Browsing general inventory";
                
            if (_itemPrice != null)
                _itemPrice.Text = "";
        }
    }
    
    private void UpdateCustomerThoughts()
    {
        if (_currentCustomer == null || _customerThoughts == null || _customerAI == null) return;
        
        string thoughts = GenerateEnhancedCustomerThoughts();
        _customerThoughts.Text = thoughts;
    }
    
    private string GenerateEnhancedCustomerThoughts()
    {
        if (_currentCustomer == null || _customerAI == null) return "";
        
        // If we have a specific item, get Enhanced AI decision
        if (_currentItem != null)
        {
            var decision = _customerAI.MakeEnhancedPurchaseDecision(_currentItem, _currentItem.Value, _interactionContext);
            
            var thoughts = new System.Collections.Generic.List<string>
            {
                $"💭 \"{decision.PrimaryReason}\"",
                $"🧠 Confidence: {decision.Confidence:P0}"
            };
            
            // Add emotional response
            var emotionIcon = decision.EmotionalResponse switch
            {
                CustomerEmotionalResponse.Delighted => "😍",
                CustomerEmotionalResponse.Satisfied => "😊",
                CustomerEmotionalResponse.Neutral => "😐",
                CustomerEmotionalResponse.Frustrated => "😤",
                CustomerEmotionalResponse.Upset => "😠",
                CustomerEmotionalResponse.Conflicted => "🤔",
                _ => "😐"
            };
            
            thoughts.Add($"{emotionIcon} Feeling: {decision.EmotionalResponse}");
            
            // Add decision-specific thoughts
            thoughts.AddRange(decision.Decision switch
            {
                PurchaseDecision.Buying => new[] { "✅ \"I'll take it!\"" },
                PurchaseDecision.WantsToNegotiate => new[] { $"💰 \"Maybe we can discuss the price? ({decision.NegotiationWillingness:P0} willing to negotiate)\"" },
                PurchaseDecision.Considering => new[] { "⏳ \"Let me think about this...\"" },
                PurchaseDecision.NotBuying => new[] { "❌ \"Not interested in this item.\"" },
                _ => new[] { "🤷 \"I'm not sure about this...\"" }
            });
            
            // Add secondary factors
            if (decision.SecondaryFactors.Any())
            {
                thoughts.Add($"📝 Also considering: {string.Join(", ", decision.SecondaryFactors)}");
            }
            
            // Add suggested action for the player
            if (!string.IsNullOrEmpty(decision.SuggestedAction))
            {
                thoughts.Add($"💡 [Player tip: {decision.SuggestedAction}]");
            }
            
            return string.Join("\n\n", thoughts);
        }
        else
        {
            // General browsing thoughts
            return GenerateBrowsingThoughts();
        }
    }
    
    private string GenerateBrowsingThoughts()
    {
        if (_currentCustomer == null) return "";
        
        var thoughts = new System.Collections.Generic.List<string>();
        
        // Base thoughts based on customer type
        thoughts.AddRange(GetTypeBasedThoughts(_currentCustomer.Type));
        
        // Personality-based thoughts
        thoughts.AddRange(GetPersonalityBasedThoughts(_currentCustomer.Personality));
        
        // Select 2-3 random thoughts
        var selectedThoughts = new System.Collections.Generic.List<string>();
        for (int i = 0; i < Math.Min(3, thoughts.Count); i++)
        {
            var randomThought = thoughts[_random.Next(thoughts.Count)];
            if (!selectedThoughts.Contains(randomThought))
            {
                selectedThoughts.Add(randomThought);
            }
        }
        
        return string.Join("\n\n", selectedThoughts);
    }
    
    private System.Collections.Generic.List<string> GetTypeBasedThoughts(CustomerType type)
    {
        return type switch
        {
            CustomerType.NoviceAdventurer => new()
            {
                "💭 \"I'm new to adventuring and need basic gear...\"",
                "💭 \"I hope the prices aren't too expensive.\"",
                "💭 \"Maybe something simple but effective?\""
            },
            CustomerType.VeteranAdventurer => new()
            {
                "💭 \"I need quality equipment that won't fail me.\"",
                "💭 \"Experience taught me to invest in good gear.\"",
                "💭 \"This better be worth the price.\""
            },
            CustomerType.NoblePatron => new()
            {
                "💭 \"Only the finest quality will do.\"",
                "💭 \"Price is no object for superior craftsmanship.\"",
                "💭 \"I expect exceptional service.\""
            },
            CustomerType.MerchantTrader => new()
            {
                "💭 \"Can I resell this for a profit?\"",
                "💭 \"Bulk discount available?\"",
                "💭 \"Market value seems reasonable...\""
            },
            CustomerType.CasualTownsperson => new()
            {
                "💭 \"Just browsing around today.\"",
                "💭 \"Maybe something useful for daily life?\"",
                "💭 \"Budget is pretty tight this month.\""
            },
            _ => new() { "💭 \"Interesting selection here...\"" }
        };
    }
    
    private System.Collections.Generic.List<string> GetItemSpecificThoughts(Item item)
    {
        var thoughts = new System.Collections.Generic.List<string>();
        
        // Quality-based thoughts
        switch (item.Quality)
        {
            case QualityTier.Common:
                thoughts.Add("💭 \"Standard quality, nothing fancy.\"");
                break;
            case QualityTier.Uncommon:
                thoughts.Add("💭 \"This has some nice craftsmanship.\"");
                break;
            case QualityTier.Rare:
                thoughts.Add("💭 \"Impressive work! This is well-made.\"");
                break;
            case QualityTier.Epic:
                thoughts.Add("💭 \"Exceptional quality! This is masterwork.\"");
                break;
            case QualityTier.Legendary:
                thoughts.Add("💭 \"Legendary craftsmanship! A true masterpiece.\"");
                break;
        }
        
        // Price-based thoughts
        if (_currentCustomer != null)
        {
            var priceRatio = (float)item.Value / _currentCustomer.BudgetRange.MaxSpendingPower;
            if (priceRatio < 0.3f)
            {
                thoughts.Add("💭 \"Very affordable pricing.\"");
            }
            else if (priceRatio < 0.7f)
            {
                thoughts.Add("💭 \"The price seems fair.\"");
            }
            else if (priceRatio < 1.0f)
            {
                thoughts.Add("💭 \"A bit pricey, but might be worth it.\"");
            }
            else
            {
                thoughts.Add("💭 \"This is quite expensive for my budget...\"");
            }
        }
        
        return thoughts;
    }
    
    private System.Collections.Generic.List<string> GetPersonalityBasedThoughts(CustomerPersonality personality)
    {
        var thoughts = new System.Collections.Generic.List<string>();
        
        if (personality.PriceSensitivity > 0.7f)
        {
            thoughts.Add("💭 \"I need to watch my spending carefully.\"");
        }
        
        if (personality.QualityFocus > 0.7f)
        {
            thoughts.Add("💭 \"Quality is more important than price.\"");
        }
        
        if (personality.NegotiationTendency > 0.5f)
        {
            thoughts.Add("💭 \"Maybe I can negotiate the price down...\"");
        }
        
        if (personality.ImpulsePurchasing > 0.7f)
        {
            thoughts.Add("💭 \"I should just buy it now!\"");
        }
        
        return thoughts;
    }
    
    private void UpdateActionButtons()
    {
        if (_currentCustomer == null) return;
        
        // Update button availability based on customer personality and situation
        if (_offerDiscountButton != null)
        {
            _offerDiscountButton.Disabled = _currentItem == null;
            _offerDiscountButton.Text = $"Offer 10% Discount";
        }
        
        if (_negotiateButton != null)
        {
            _negotiateButton.Disabled = _currentCustomer.Personality.NegotiationTendency < 0.3f || _currentItem == null;
        }
        
        if (_showAlternativeButton != null)
        {
            _showAlternativeButton.Disabled = _shopManager?.DisplaySlots.Count <= 1;
        }
    }
    
    private string GetCustomerTypeDisplay(CustomerType type)
    {
        return type switch
        {
            CustomerType.NoviceAdventurer => "🗡️ Novice Adventurer",
            CustomerType.VeteranAdventurer => "⚔️ Veteran Adventurer", 
            CustomerType.NoblePatron => "👑 Noble Patron",
            CustomerType.MerchantTrader => "💰 Merchant Trader",
            CustomerType.CasualTownsperson => "🏘️ Townsperson",
            _ => "❓ Unknown"
        };
    }
    
    private string GetCustomerMoodDisplay()
    {
        if (_currentCustomer == null) return "Unknown";
        
        return _currentCustomer.CurrentState switch
        {
            CustomerState.Browsing => "👀 Browsing",
            CustomerState.Examining => "🤔 Examining",
            CustomerState.Considering => "⚖️ Considering",
            CustomerState.Negotiating => "💬 Negotiating",
            CustomerState.ReadyToBuy => "💳 Ready to Buy",
            CustomerState.Purchasing => "💰 Purchasing",
            CustomerState.Leaving => "🚪 Leaving",
            _ => "Unknown"
        };
    }
    
    private void OnOfferDiscountPressed()
    {
        if (_currentCustomer == null || _currentItem == null || _customerAI == null) return;
        
        GameLogger.Info($"Offering discount to {_currentCustomer.Name} for {_currentItem.Name}");
        
        // Update interaction context
        _interactionContext.DiscountOffered = true;
        _interactionContext.InteractionQualityScore += 0.2f; // Positive interaction
        _interactionContext.TotalInteractions++;
        
        // Get Enhanced AI response to discount offer
        var response = _customerAI.MakeEnhancedPurchaseDecision(_currentItem, _currentItem.Value * 0.9m, _interactionContext);
        
        // Generate realistic customer reaction
        var reactionText = response.Decision switch
        {
            PurchaseDecision.Buying => "� \"That's perfect! I'll take it at that price!\"",
            PurchaseDecision.WantsToNegotiate => "🤔 \"That helps, but could we go a bit lower?\"",
            PurchaseDecision.Considering => "� \"That's more reasonable... let me think.\"",
            PurchaseDecision.NotBuying => "� \"I appreciate the gesture, but it's still not quite right for me.\"",
            _ => "🙂 \"Thank you for considering a discount.\""
        };
        
        var fullResponse = $"💰 Discount Offered!\n\n{reactionText}\n\n💭 \"{response.PrimaryReason}\"\n\n💡 {response.SuggestedAction}";
        _customerThoughts!.Text = fullResponse;
        
        EmitSignal(Main.UI.CustomerInteractionDialogUI.SignalName.CustomerActionTaken, _currentCustomer.CustomerId, "discount_offered", _currentItem.ItemId);
        
        // Update action buttons based on new state
        UpdateActionButtons();
    }
    
    private void OnNegotiatePressed()
    {
        if (_currentCustomer == null || _currentItem == null || _customerAI == null) return;
        
        GameLogger.Info($"Starting negotiation with {_currentCustomer.Name} for {_currentItem.Name}");
        
        // Update interaction context
        _interactionContext.NegotiationAttempted = true;
        _interactionContext.InteractionQualityScore += 0.1f; // Slight positive for engagement
        _interactionContext.TotalInteractions++;
        
        // Simulate negotiation with Enhanced AI
        var originalPrice = _currentItem.Value;
        var proposedPrice = _currentCustomer.AttemptNegotiation(_currentItem, originalPrice);
        
        if (proposedPrice.HasValue)
        {
            // Get Enhanced AI response to negotiation
            var response = _customerAI.MakeEnhancedPurchaseDecision(_currentItem, proposedPrice.Value, _interactionContext);
            
            var negotiationText = $"💰 \"How about {proposedPrice.Value}g? That seems fair.\"";
            
            var fullResponse = $"🤝 Negotiation Started!\n\n{negotiationText}\n\n💭 \"{response.PrimaryReason}\"\n\nConfidence: {response.Confidence:P0}\n\n💡 {response.SuggestedAction}";
            _customerThoughts!.Text = fullResponse;
        }
        else
        {
            _customerThoughts!.Text = "😔 \"I appreciate the conversation, but I prefer not to negotiate on prices.\"";
        }
        
        EmitSignal(Main.UI.CustomerInteractionDialogUI.SignalName.CustomerActionTaken, _currentCustomer.CustomerId, "negotiation_started", _currentItem.ItemId);
        
        // Update action buttons
        UpdateActionButtons();
    }
    
    private void OnShowAlternativePressed()
    {
        if (_currentCustomer == null || _shopManager == null || _customerAI == null) return;
        
        GameLogger.Info($"Showing alternatives to {_currentCustomer.Name}");
        
        // Update interaction context
        _interactionContext.InteractionQualityScore += 0.15f; // Positive - showing helpfulness
        _interactionContext.TotalInteractions++;
        
        // Find alternative items from the shop
        var availableItems = _shopManager.DisplaySlots
            .Where(s => s.CurrentItem != null && s.CurrentItem != _currentItem)
            .Select(s => s.CurrentItem!)
            .ToList();
            
        if (availableItems.Any())
        {
            // Use customer preferences to find the best alternative
            var bestAlternative = availableItems
                .OrderByDescending(item => _currentCustomer.EvaluateItem(item, item.Value))
                .First();
            
            // Get Enhanced AI analysis of the alternative
            var response = _customerAI.MakeEnhancedPurchaseDecision(bestAlternative, bestAlternative.Value, _interactionContext);
            
            var alternativeText = response.Decision switch
            {
                PurchaseDecision.Buying => $"😍 \"Oh! That {bestAlternative.Name} looks perfect!\"",
                PurchaseDecision.WantsToNegotiate => $"🤔 \"That {bestAlternative.Name} is interesting. What's your best price?\"",
                PurchaseDecision.Considering => $"💭 \"Hmm, that {bestAlternative.Name} might work for me...\"",
                PurchaseDecision.NotBuying => $"😐 \"The {bestAlternative.Name} isn't quite what I need.\"",
                _ => $"🤷 \"Let me think about that {bestAlternative.Name}...\""
            };
            
            var fullResponse = $"🔄 Alternative Shown!\n\n{alternativeText}\n\n💭 \"{response.PrimaryReason}\"\n\nInterest Level: {response.Confidence:P0}\n\n💡 {response.SuggestedAction}";
            _customerThoughts!.Text = fullResponse;
            
            // Update the item display to show the alternative
            _currentItem = bestAlternative;
            UpdateItemDisplay();
            UpdateActionButtons();
        }
        else
        {
            _customerThoughts!.Text = "💭 \"I appreciate you trying to help, but I'll stick with what I was looking at.\"";
        }
        
        EmitSignal(Main.UI.CustomerInteractionDialogUI.SignalName.CustomerActionTaken, _currentCustomer.CustomerId, "show_alternatives", _currentItem?.ItemId ?? "");
    }
    
    private void OnClosePressed()
    {
        if (_currentCustomer != null)
        {
            GameLogger.Info($"Letting {_currentCustomer.Name} continue browsing");
            EmitSignal(Main.UI.CustomerInteractionDialogUI.SignalName.CustomerActionTaken, _currentCustomer.CustomerId, "continue_browsing", "");
        }
        
        Hide();
    }
    
    public override void _ExitTree()
    {
        // Clean up button connections
        if (_offerDiscountButton != null)
            _offerDiscountButton.Pressed -= OnOfferDiscountPressed;
            
        if (_negotiateButton != null)
            _negotiateButton.Pressed -= OnNegotiatePressed;
            
        if (_showAlternativeButton != null)
            _showAlternativeButton.Pressed -= OnShowAlternativePressed;
            
        if (_closeButton != null)
            _closeButton.Pressed -= OnClosePressed;
    }
}
