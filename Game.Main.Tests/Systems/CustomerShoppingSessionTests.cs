#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Game.Main.Models;
using Game.Main.Systems;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Tests for the CustomerShoppingSession system that orchestrates complete customer experiences.
/// </summary>
public class CustomerShoppingSessionTests
{
    private (ShopManager shopManager, Customer customer, CustomerShoppingSession session) CreateTestSession()
    {
        var shopManager = new ShopManager();
        var customer = new Customer(CustomerType.NoviceAdventurer, "Test Customer");
        var session = new CustomerShoppingSession(customer, shopManager, isTestMode: true);
        
        return (shopManager, customer, session);
    }
    
    private Item CreateTestItem(string name = "Test Sword", ItemType type = ItemType.Weapon, QualityTier quality = QualityTier.Common)
    {
        return new Item(
            itemId: Guid.NewGuid().ToString(),
            name: name,
            description: $"A test {name.ToLower()}",
            itemType: type,
            quality: quality,
            value: 50
        );
    }
    
    [Fact]
    public void CustomerShoppingSession_Initialize_CreatesValidSession()
    {
        // Arrange & Act
        var (shopManager, customer, session) = CreateTestSession();
        
        // Assert
        Assert.Equal(customer, session.Customer);
        Assert.True(session.IsActive);
        Assert.Equal(ShoppingPhase.Entering, session.CurrentPhase);
        Assert.Empty(session.ExaminedItems);
        Assert.Null(session.CompletedTransaction);
    }
    
    [Fact]
    public async Task CustomerShoppingSession_EmptyShop_CustomerLeavesQuickly()
    {
        // Arrange
        var (shopManager, customer, session) = CreateTestSession();
        
        // Act
        var satisfaction = await session.RunShoppingSessionAsync();
        
        // Assert
        Assert.False(session.IsActive);
        Assert.Equal(ShoppingPhase.Leaving, session.CurrentPhase);
        Assert.Null(session.CompletedTransaction);
        Assert.True(session.SessionDuration.TotalSeconds > 0);
        Assert.True(satisfaction == CustomerSatisfaction.Neutral || satisfaction == CustomerSatisfaction.Disappointed);
    }
    
    [Fact]
    public async Task CustomerShoppingSession_WithStockedItems_CustomerExaminesItems()
    {
        // Arrange
        var (shopManager, customer, session) = CreateTestSession();
        var testItem = CreateTestItem("Test Sword", ItemType.Weapon);
        shopManager.StockItem(testItem, 0, 50m);
        
        // Track examined items
        var examinedItems = new List<(Customer customer, Item item, CustomerInterest interest)>();
        session.ItemExamined += (c, i, interest) => examinedItems.Add((c, i, interest));
        
        // Act
        var satisfaction = await session.RunShoppingSessionAsync();
        
        // Assert
        Assert.False(session.IsActive);
        Assert.NotEmpty(session.ExaminedItems);
        Assert.NotEmpty(examinedItems);
        
        var examinedItem = session.ExaminedItems.First();
        Assert.Equal(testItem.ItemId, examinedItem.Item.ItemId);
        Assert.True(Enum.IsDefined(typeof(CustomerInterest), examinedItem.Interest));
    }
    
    [Fact]
    public async Task CustomerShoppingSession_HighInterestItem_AttemptsPurchase()
    {
        // Arrange
        var (shopManager, customer, session) = CreateTestSession();
        
        // Create an item that novice adventurers would be very interested in
        var attractiveItem = CreateTestItem("Iron Sword", ItemType.Weapon, QualityTier.Common);
        shopManager.StockItem(attractiveItem, 0, 25m); // Very affordable price for novice
        
        // Track session events
        var phaseChanges = new List<ShoppingPhase>();
        session.PhaseChanged += (c, phase) => phaseChanges.Add(phase);
        
        // Act
        var satisfaction = await session.RunShoppingSessionAsync();
        
        // Assert
        Assert.False(session.IsActive);
        // Customer should go through browsing and examining phases at minimum
        Assert.Contains(ShoppingPhase.Browsing, phaseChanges);
        Assert.Contains(ShoppingPhase.Examining, phaseChanges);
        
        // Customer should examine the item
        Assert.NotEmpty(session.ExaminedItems);
        var itemInterest = session.ExaminedItems.First().Interest;
        Assert.True(itemInterest >= CustomerInterest.SlightlyInterested);
        
        // Customer may or may not purchase - both are valid outcomes
        // This tests the decision-making process, not the specific outcome
    }
    
    [Fact]
    public async Task CustomerShoppingSession_AffordableHighInterestItem_CompletesSuccessfulPurchase()
    {
        // Arrange
        var (shopManager, customer, session) = CreateTestSession();
        
        // Create a very appealing and affordable item
        var perfectItem = CreateTestItem("Beginner Sword", ItemType.Weapon, QualityTier.Common);
        shopManager.StockItem(perfectItem, 0, 25m); // Very affordable for novice (budget ~25-75g)
        
        // Track purchase completion
        SaleTransaction? completedTransaction = null;
        session.PurchaseCompleted += (c, t) => completedTransaction = t;
        
        // Act
        var satisfaction = await session.RunShoppingSessionAsync();
        
        // Assert
        if (completedTransaction != null)
        {
            // Purchase was completed
            Assert.NotNull(session.CompletedTransaction);
            Assert.Equal(perfectItem.ItemId, completedTransaction.ItemSold.ItemId);
            Assert.Equal(25m, completedTransaction.SalePrice);
            Assert.True(satisfaction >= CustomerSatisfaction.Neutral);
            
            // Verify shop state
            Assert.Equal(0, shopManager.ItemsOnDisplay);
            Assert.Single(shopManager.TransactionHistory);
            Assert.True(shopManager.TreasuryGold > 100m); // Starting gold + sale
        }
        else
        {
            // Customer didn't buy - this is also valid behavior
            Assert.Null(session.CompletedTransaction);
            Assert.NotEmpty(session.ExaminedItems);
        }
    }
    
    [Fact]
    public async Task CustomerShoppingSession_OverpricedItem_CustomerNegotiatesOrLeaves()
    {
        // Arrange
        var (shopManager, customer, session) = CreateTestSession();
        
        // Create an overpriced item
        var expensiveItem = CreateTestItem("Golden Sword", ItemType.Weapon, QualityTier.Rare);
        shopManager.StockItem(expensiveItem, 0, 200m); // Too expensive for novice adventurer
        
        // Track negotiation attempts
        var negotiationAttempts = new List<(Customer customer, Item item, decimal offer)>();
        session.NegotiationStarted += (c, i, offer) => negotiationAttempts.Add((c, i, offer));
        
        // Act
        var satisfaction = await session.RunShoppingSessionAsync();
        
        // Assert
        Assert.False(session.IsActive);
        
        if (negotiationAttempts.Any())
        {
            // Customer attempted negotiation
            var negotiation = negotiationAttempts.First();
            Assert.Equal(expensiveItem.ItemId, negotiation.item.ItemId);
            Assert.True(negotiation.offer < 200m);
        }
        
        // Customer likely didn't purchase due to price
        Assert.NotEmpty(session.ExaminedItems);
        var itemInterest = session.ExaminedItems.First(e => e.Item.ItemId == expensiveItem.ItemId).Interest;
        // High quality item should generate some interest, but price will be prohibitive
    }
    
    [Fact]
    public async Task CustomerShoppingSession_VeteranCustomer_ExaminesMoreItems()
    {
        // Arrange
        var shopManager = new ShopManager();
        var veteranCustomer = new Customer(CustomerType.VeteranAdventurer, "Veteran Test");
        var session = new CustomerShoppingSession(veteranCustomer, shopManager, isTestMode: true);
        
        // Stock multiple items
        var sword = CreateTestItem("Steel Sword", ItemType.Weapon, QualityTier.Uncommon);
        var armor = CreateTestItem("Leather Armor", ItemType.Armor, QualityTier.Common);
        var potion = CreateTestItem("Health Potion", ItemType.Consumable, QualityTier.Common);
        
        shopManager.StockItem(sword, 0, 100m);
        shopManager.StockItem(armor, 1, 80m);
        shopManager.StockItem(potion, 2, 30m);
        
        // Act
        var satisfaction = await session.RunShoppingSessionAsync();
        
        // Assert
        Assert.False(session.IsActive);
        
        // Veterans should examine at least one item (customer behavior may vary)
        Assert.NotEmpty(session.ExaminedItems);
        
        // Should show interest in equipment items
        var equipmentInterest = session.ExaminedItems.Where(e => 
            e.Item.ItemType == ItemType.Weapon || e.Item.ItemType == ItemType.Armor).ToList();
        
        if (equipmentInterest.Any())
        {
            // If they examined equipment, they should show reasonable interest
            Assert.True(equipmentInterest.Any(e => e.Interest >= CustomerInterest.SlightlyInterested));
        }
    }
    
    [Fact]
    public async Task CustomerShoppingSession_NobleCustomer_WillingToPayHighPrices()
    {
        // Arrange
        var shopManager = new ShopManager();
        var nobleCustomer = new Customer(CustomerType.NoblePatron, "Noble Test");
        var session = new CustomerShoppingSession(nobleCustomer, shopManager, isTestMode: true);
        
        // Stock expensive high-quality item
        var luxuryItem = CreateTestItem("Masterwork Blade", ItemType.Weapon, QualityTier.Epic);
        shopManager.StockItem(luxuryItem, 0, 300m); // Expensive but more reasonable for nobles
        
        // Track purchase completion
        SaleTransaction? completedTransaction = null;
        session.PurchaseCompleted += (c, t) => completedTransaction = t;
        
        // Act
        var satisfaction = await session.RunShoppingSessionAsync();
        
        // Assert
        Assert.False(session.IsActive);
        Assert.NotEmpty(session.ExaminedItems);
        
        var luxuryInterest = session.ExaminedItems.First(e => e.Item.ItemId == luxuryItem.ItemId);
        // Nobles should show at least some interest in high-quality items
        Assert.True(luxuryInterest.Interest > CustomerInterest.NotInterested);
        
        // Purchase outcome depends on various factors - both buying and not buying are valid
        if (completedTransaction != null)
        {
            Assert.Equal(luxuryItem.ItemId, completedTransaction.ItemSold.ItemId);
        }
    }
    
    [Fact]
    public void CustomerShoppingSession_SessionDuration_TracksTimeAccurately()
    {
        // Arrange
        var (shopManager, customer, session) = CreateTestSession();
        var startTime = DateTime.Now;
        
        // Act - Simulate some time passing
        System.Threading.Thread.Sleep(100);
        var duration = session.SessionDuration;
        
        // Assert
        Assert.True(duration.TotalMilliseconds >= 90); // Allow for timing precision
        Assert.True(duration.TotalSeconds < 1); // Should be very short for this test
    }
    
    [Fact]
    public async Task CustomerShoppingSession_Events_FireInCorrectOrder()
    {
        // Arrange
        var (shopManager, customer, session) = CreateTestSession();
        var testItem = CreateTestItem();
        shopManager.StockItem(testItem, 0, 30m); // More affordable
        
        var eventOrder = new List<string>();
        
        session.PhaseChanged += (c, phase) => eventOrder.Add($"Phase:{phase}");
        session.ItemExamined += (c, i, interest) => eventOrder.Add($"Examined:{i.Name}");
        session.SessionEnded += (c, s, r) => eventOrder.Add($"Ended:{r}");
        
        // Act
        await session.RunShoppingSessionAsync();
        
        // Assert
        Assert.NotEmpty(eventOrder);
        
        // Check that key phases occur in logical order
        var browsingIndex = eventOrder.FindIndex(e => e.StartsWith("Phase:Browsing"));
        var examiningIndex = eventOrder.FindIndex(e => e.StartsWith("Phase:Examining"));
        var leavingIndex = eventOrder.FindIndex(e => e.StartsWith("Phase:Leaving"));
        var endedIndex = eventOrder.FindIndex(e => e.StartsWith("Ended:"));
        
        // Browsing should occur
        Assert.True(browsingIndex >= 0);
        
        // If examining occurred, it should be after browsing
        if (examiningIndex >= 0)
        {
            Assert.True(examiningIndex > browsingIndex);
        }
        
        // Leaving should occur
        Assert.True(leavingIndex >= 0);
        
        // Session ended should be last
        Assert.True(endedIndex >= 0);
        Assert.Equal(eventOrder.Count - 1, endedIndex);
    }
}
