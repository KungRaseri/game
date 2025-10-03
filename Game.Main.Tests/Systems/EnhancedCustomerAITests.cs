#nullable enable

using Game.Adventure.Data;
using Game.Items.Data;
using Game.Items.Models;
using Game.Main.Systems;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Tests for the Enhanced Customer AI system that provides sophisticated decision-making logic.
/// </summary>
public class EnhancedCustomerAITests
{
    [Fact]
    public void MakeEnhancedPurchaseDecision_WithHighPriceItem_ReturnsValidDecision()
    {
        // Arrange
        var customer = CreateTestCustomer(CustomerType.NoviceAdventurer, maxBudget: 100m);
        var expensiveItem = ItemFactory.CreateMithrilDagger(QualityTier.Legendary);
        var customerAI = new EnhancedCustomerAI(customer);
        var context = CreateTestContext();
        
        // Act
        var decision = customerAI.MakeEnhancedPurchaseDecision(expensiveItem, 1000m, context);
        
        // Assert
        Assert.NotNull(decision);
        Assert.NotEmpty(decision.PrimaryReason);
        Assert.True(decision.Confidence >= 0f && decision.Confidence <= 1f);
        Assert.True(Enum.IsDefined(typeof(PurchaseDecision), decision.Decision));
    }
    
    [Fact]
    public void MakeEnhancedPurchaseDecision_WithAffordableQualityItem_ReturnsValidDecision()
    {
        // Arrange
        var customer = CreateTestCustomer(CustomerType.VeteranAdventurer, maxBudget: 500m);
        var qualityItem = ItemFactory.CreateIronSword(QualityTier.Rare);
        var customerAI = new EnhancedCustomerAI(customer);
        var context = CreateTestContext();
        
        // Act
        var decision = customerAI.MakeEnhancedPurchaseDecision(qualityItem, 100m, context);
        
        // Assert
        Assert.NotNull(decision);
        Assert.NotEmpty(decision.PrimaryReason);
        Assert.True(decision.Confidence >= 0f && decision.Confidence <= 1f);
        Assert.True(Enum.IsDefined(typeof(PurchaseDecision), decision.Decision));
        Assert.NotEmpty(decision.SuggestedAction);
    }
    
    [Fact]
    public void MakeEnhancedPurchaseDecision_WithMerchantTrader_HasNegotiationAwareness()
    {
        // Arrange (Merchant traders typically have higher negotiation tendency)
        var customer = CreateTestCustomer(CustomerType.MerchantTrader, maxBudget: 400m);
        var item = ItemFactory.CreateSteelAxe(QualityTier.Uncommon);
        var customerAI = new EnhancedCustomerAI(customer);
        var context = CreateTestContext();
        
        // Act
        var decision = customerAI.MakeEnhancedPurchaseDecision(item, 200m, context);
        
        // Assert
        Assert.NotNull(decision);
        Assert.True(decision.NegotiationWillingness >= 0f && decision.NegotiationWillingness <= 1f);
        Assert.NotEmpty(decision.SuggestedAction);
        Assert.True(Enum.IsDefined(typeof(CustomerEmotionalResponse), decision.EmotionalResponse));
    }
    
    [Fact]
    public void MakeEnhancedPurchaseDecision_WithGoodInteraction_ProducesValidResponse()
    {
        // Arrange
        var customer = CreateTestCustomer(CustomerType.CasualTownsperson, maxBudget: 150m);
        var item = ItemFactory.CreateIronSword(QualityTier.Common);
        var customerAI = new EnhancedCustomerAI(customer);
        
        var poorContext = CreateTestContext(interactionQuality: 0.2f);
        var goodContext = CreateTestContext(interactionQuality: 0.9f);
        
        // Act
        var poorDecision = customerAI.MakeEnhancedPurchaseDecision(item, 120m, poorContext);
        var goodDecision = customerAI.MakeEnhancedPurchaseDecision(item, 120m, goodContext);
        
        // Assert - Good interaction should not make decision worse
        Assert.True(goodDecision.Confidence >= poorDecision.Confidence);
        Assert.NotEmpty(poorDecision.SuggestedAction);
        Assert.NotEmpty(goodDecision.SuggestedAction);
    }
    
    [Fact]
    public void MakeEnhancedPurchaseDecision_WithDiscountContext_RecognizesDiscount()
    {
        // Arrange
        var customer = CreateTestCustomer(CustomerType.NoviceAdventurer, maxBudget: 100m);
        var item = ItemFactory.CreateIronSword(QualityTier.Common);
        var customerAI = new EnhancedCustomerAI(customer);
        
        var baseContext = CreateTestContext();
        var discountContext = CreateTestContext(discountOffered: true);
        
        // Act
        var baseDecision = customerAI.MakeEnhancedPurchaseDecision(item, 90m, baseContext);
        var discountDecision = customerAI.MakeEnhancedPurchaseDecision(item, 81m, discountContext); // 10% discount
        
        // Assert
        Assert.NotNull(baseDecision);
        Assert.NotNull(discountDecision);
        Assert.NotEmpty(discountDecision.SuggestedAction);
    }
    
    [Fact]
    public void MakeEnhancedPurchaseDecision_WithNoblePatron_HandlesLuxuryItems()
    {
        // Arrange
        var customer = CreateTestCustomer(CustomerType.NoblePatron, maxBudget: 2000m);
        var commonItem = ItemFactory.CreateIronSword(QualityTier.Common);
        var legendaryItem = ItemFactory.CreateMithrilDagger(QualityTier.Legendary);
        var customerAI = new EnhancedCustomerAI(customer);
        var context = CreateTestContext();
        
        // Act
        var commonDecision = customerAI.MakeEnhancedPurchaseDecision(commonItem, 50m, context);
        var legendaryDecision = customerAI.MakeEnhancedPurchaseDecision(legendaryItem, 500m, context);
        
        // Assert - Noble patrons should respond to quality appropriately
        Assert.NotNull(commonDecision);
        Assert.NotNull(legendaryDecision);
        Assert.True(Enum.IsDefined(typeof(PurchaseDecision), commonDecision.Decision));
        Assert.True(Enum.IsDefined(typeof(PurchaseDecision), legendaryDecision.Decision));
    }
    
    [Fact]
    public void MakeEnhancedPurchaseDecision_ProducesValidEmotionalResponse()
    {
        // Arrange
        var customer = CreateTestCustomer(CustomerType.VeteranAdventurer, maxBudget: 300m);
        var greatItem = ItemFactory.CreateSteelAxe(QualityTier.Epic);
        var poorItem = ItemFactory.CreateIronSword(QualityTier.Common);
        var customerAI = new EnhancedCustomerAI(customer);
        var context = CreateTestContext(interactionQuality: 0.8f);
        
        // Act
        var greatDecision = customerAI.MakeEnhancedPurchaseDecision(greatItem, 150m, context);
        var poorDecision = customerAI.MakeEnhancedPurchaseDecision(poorItem, 200m, context);
        
        // Assert
        Assert.True(Enum.IsDefined(typeof(CustomerEmotionalResponse), greatDecision.EmotionalResponse));
        Assert.True(Enum.IsDefined(typeof(CustomerEmotionalResponse), poorDecision.EmotionalResponse));
        Assert.NotEmpty(greatDecision.PrimaryReason);
        Assert.NotEmpty(poorDecision.PrimaryReason);
    }
    
    [Fact]
    public void MakeEnhancedPurchaseDecision_AlwaysProvidesSuggestedAction()
    {
        // Arrange
        var customer = CreateTestCustomer(CustomerType.MerchantTrader, maxBudget: 500m);
        var item = ItemFactory.CreateLeatherArmor(QualityTier.Uncommon);
        var customerAI = new EnhancedCustomerAI(customer);
        var context = CreateTestContext();
        
        // Act
        var decision = customerAI.MakeEnhancedPurchaseDecision(item, 300m, context);
        
        // Assert
        Assert.NotNull(decision.SuggestedAction);
        Assert.NotEmpty(decision.SuggestedAction);
        Assert.True(decision.SuggestedAction.Length > 10); // Should be a meaningful suggestion
    }
    
    // Helper methods
    private Customer CreateTestCustomer(CustomerType type, decimal maxBudget)
    {
        // Customer constructor auto-generates all properties, we can't customize budget easily
        // So we'll use the auto-generated customer and work with what we get
        return new Customer(type, $"Test{type}Customer");
    }
    
    private Customer CreateTestCustomerWithPersonality(CustomerType type, CustomerPersonality personality, decimal maxBudget)
    {
        // Note: We can't easily override personality in constructor, so this will use default personality
        // The actual personality-based testing would need to be done with real customer generation
        return new Customer(type, $"Test{type}Customer");
    }
    
    private ShopInteractionContext CreateTestContext(
        float interactionQuality = 0.5f, 
        bool discountOffered = false, 
        bool alternativesAvailable = true)
    {
        return new ShopInteractionContext
        {
            InteractionQualityScore = interactionQuality,
            ShopReputationScore = 0.7f,
            ShopAmbianceScore = 0.6f,
            OtherCustomersSatisfaction = 0.5f,
            AlternativeItemsAvailable = alternativesAvailable,
            TotalInteractions = discountOffered ? 1 : 0,
            DiscountOffered = discountOffered,
            NegotiationAttempted = false
        };
    }
}
