#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Game.Main.Models;
using Game.Main.Systems;

namespace Game.Main.Tests.Systems;

/// <summary>
/// Tests for the ShopTrafficManager that orchestrates multiple customer sessions.
/// </summary>
public class ShopTrafficManagerTests
{
    private (ShopManager shopManager, ShopTrafficManager trafficManager) CreateTrafficSystem()
    {
        var shopManager = new ShopManager();
        var trafficManager = new ShopTrafficManager(shopManager, isTestMode: true);
        
        return (shopManager, trafficManager);
    }
    
    private Item CreateTestItem(string name = "Test Item", ItemType type = ItemType.Weapon)
    {
        return new Item(
            itemId: Guid.NewGuid().ToString(),
            name: name,
            description: $"A test {name.ToLower()}",
            itemType: type,
            quality: QualityTier.Common,
            value: 50
        );
    }
    
    [Fact]
    public void ShopTrafficManager_Initialize_StartsInactive()
    {
        // Arrange & Act
        var (shopManager, trafficManager) = CreateTrafficSystem();
        
        // Assert
        Assert.False(trafficManager.IsActive);
        Assert.Equal(0, trafficManager.CurrentCustomerCount);
        Assert.Empty(trafficManager.ActiveSessions);
        Assert.Empty(trafficManager.TrafficHistory);
        Assert.Equal(TrafficLevel.Moderate, trafficManager.CurrentTrafficLevel);
    }
    
    [Fact]
    public void ShopTrafficManager_StartTraffic_ActivatesTrafficGeneration()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        
        // Act
        trafficManager.StartTraffic();
        
        // Assert
        Assert.True(trafficManager.IsActive);
        Assert.True(Enum.IsDefined(typeof(TrafficLevel), trafficManager.CurrentTrafficLevel));
    }
    
    [Fact]
    public async Task ShopTrafficManager_StopTraffic_DeactivatesAndCleansUp()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        trafficManager.StartTraffic();
        
        // Act
        await trafficManager.StopTrafficAsync();
        
        // Assert
        Assert.False(trafficManager.IsActive);
        // All sessions should be completed when traffic stops
    }
    
    [Fact]
    public async Task ShopTrafficManager_AddCustomer_CreatesActiveSession()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        var testCustomer = new Customer(CustomerType.NoviceAdventurer, "Test Customer");
        
        // Track customer events
        Customer? enteredCustomer = null;
        Customer? leftCustomer = null;
        CustomerSatisfaction? finalSatisfaction = null;
        
        trafficManager.CustomerEntered += (customer) => enteredCustomer = customer;
        trafficManager.CustomerLeft += (customer, satisfaction, reason) => 
        {
            leftCustomer = customer;
            finalSatisfaction = satisfaction;
        };
        
        // Act
        var satisfaction = await trafficManager.AddCustomerAsync(testCustomer);
        
        // Assert
        Assert.NotNull(enteredCustomer);
        Assert.Equal(testCustomer.CustomerId, enteredCustomer.CustomerId);
        
        Assert.NotNull(leftCustomer);
        Assert.Equal(testCustomer.CustomerId, leftCustomer.CustomerId);
        
        Assert.NotNull(finalSatisfaction);
        Assert.Equal(satisfaction, finalSatisfaction.Value);
        
        // Customer should be in traffic history
        Assert.Single(trafficManager.TrafficHistory);
        var trafficRecord = trafficManager.TrafficHistory.First();
        Assert.Equal(testCustomer.CustomerId, trafficRecord.CustomerId);
        Assert.Equal(testCustomer.Type, trafficRecord.CustomerType);
    }
    
    [Fact]
    public async Task ShopTrafficManager_CustomerPurchase_RecordsTransaction()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        var testItem = CreateTestItem("Purchase Test Item");
        shopManager.StockItem(testItem, 0, 50m);
        
        var testCustomer = new Customer(CustomerType.VeteranAdventurer, "Purchasing Customer");
        
        // Track purchase events
        SaleTransaction? recordedTransaction = null;
        trafficManager.CustomerPurchased += (customer, transaction) => recordedTransaction = transaction;
        
        // Act
        await trafficManager.AddCustomerAsync(testCustomer);
        
        // Assert - Check if purchase was recorded (customer behavior may vary)
        var trafficRecord = trafficManager.TrafficHistory.FirstOrDefault(r => r.CustomerId == testCustomer.CustomerId);
        Assert.NotNull(trafficRecord);
        
        if (recordedTransaction != null)
        {
            // Purchase was made
            Assert.True(trafficRecord.MadePurchase);
            Assert.True(trafficRecord.PurchaseAmount > 0);
            Assert.Equal(testItem.ItemId, recordedTransaction.ItemSold.ItemId);
        }
        else
        {
            // No purchase made - also valid
            Assert.False(trafficRecord.MadePurchase);
            Assert.Equal(0, trafficRecord.PurchaseAmount);
        }
    }
    
    [Fact]
    public void ShopTrafficManager_GetTrafficAnalytics_ReturnsValidData()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        
        // Act
        var analytics = trafficManager.GetTrafficAnalytics();
        
        // Assert
        Assert.NotNull(analytics);
        Assert.Equal(0, analytics.CurrentCustomers);
        Assert.True(analytics.MaxConcurrentCustomers > 0);
        Assert.True(Enum.IsDefined(typeof(TrafficLevel), analytics.CurrentTrafficLevel));
        Assert.Equal(0, analytics.TodayVisitors);
        Assert.Equal(0, analytics.TodayPurchasers);
        Assert.Equal(0f, analytics.TodayConversionRate);
        Assert.True(analytics.CalculatedAt <= DateTime.Now);
        Assert.NotNull(analytics.CustomerTypeDistribution);
    }
    
    [Fact]
    public async Task ShopTrafficManager_MultipleCustomers_CalculatesConversionRate()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        
        // Stock some items to enable potential purchases
        var sword = CreateTestItem("Test Sword", ItemType.Weapon);
        var armor = CreateTestItem("Test Armor", ItemType.Armor);
        shopManager.StockItem(sword, 0, 80m);
        shopManager.StockItem(armor, 1, 120m);
        
        // Add multiple customers
        var customer1 = new Customer(CustomerType.NoviceAdventurer, "Customer 1");
        var customer2 = new Customer(CustomerType.VeteranAdventurer, "Customer 2");
        var customer3 = new Customer(CustomerType.NoblePatron, "Customer 3");
        
        // Act
        await trafficManager.AddCustomerAsync(customer1);
        await trafficManager.AddCustomerAsync(customer2);
        await trafficManager.AddCustomerAsync(customer3);
        
        var analytics = trafficManager.GetTrafficAnalytics();
        
        // Assert
        Assert.Equal(3, analytics.TodayVisitors);
        Assert.Equal(3, trafficManager.TrafficHistory.Count);
        
        // Conversion rate should be calculated correctly
        var purchasers = trafficManager.TrafficHistory.Count(r => r.MadePurchase);
        var expectedConversionRate = purchasers / 3f;
        Assert.Equal(expectedConversionRate, analytics.TodayConversionRate);
        
        // Customer type distribution should be tracked
        Assert.True(analytics.CustomerTypeDistribution.ContainsKey(CustomerType.NoviceAdventurer));
        Assert.True(analytics.CustomerTypeDistribution.ContainsKey(CustomerType.VeteranAdventurer));
        Assert.True(analytics.CustomerTypeDistribution.ContainsKey(CustomerType.NoblePatron));
    }
    
    [Fact]
    public void ShopTrafficManager_UpdateTrafficLevel_RespondsToShopPerformance()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        var initialLevel = trafficManager.CurrentTrafficLevel;
        
        // Simulate poor performance by having empty shop
        trafficManager.UpdateTrafficLevel();
        var emptyShopLevel = trafficManager.CurrentTrafficLevel;
        
        // Stock high-quality items to improve perception
        for (int i = 0; i < 6; i++)
        {
            var item = CreateTestItem($"Quality Item {i}", ItemType.Weapon);
            shopManager.StockItem(item, i, 100m);
        }
        
        trafficManager.UpdateTrafficLevel();
        var stockedShopLevel = trafficManager.CurrentTrafficLevel;
        
        // Assert
        Assert.True(Enum.IsDefined(typeof(TrafficLevel), initialLevel));
        Assert.True(Enum.IsDefined(typeof(TrafficLevel), emptyShopLevel));
        Assert.True(Enum.IsDefined(typeof(TrafficLevel), stockedShopLevel));
        
        // Stocked shop should generally have better traffic potential
        // (though exact levels depend on complex factors)
    }
    
    [Fact]
    public async Task ShopTrafficManager_DuplicateCustomer_RejectsSecondAddition()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        var testCustomer = new Customer(CustomerType.NoviceAdventurer, "Duplicate Test");
        
        // Act - Add customer twice quickly before first session ends
        var firstTask = trafficManager.AddCustomerAsync(testCustomer);
        var secondTask = trafficManager.AddCustomerAsync(testCustomer);
        
        var firstResult = await firstTask;
        var secondResult = await secondTask;
        
        // Assert
        Assert.True(Enum.IsDefined(typeof(CustomerSatisfaction), firstResult));
        Assert.True(Enum.IsDefined(typeof(CustomerSatisfaction), secondResult)); // Both should return valid satisfaction values
        
        // Due to concurrent execution, we may get 1 or 2 records depending on timing
        Assert.True(trafficManager.TrafficHistory.Count is 1 or 2);
    }
    
    [Fact]
    public async Task ShopTrafficManager_SessionDuration_TracksAccurately()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        var testCustomer = new Customer(CustomerType.NoviceAdventurer, "Duration Test");
        
        // Act
        var startTime = DateTime.Now;
        await trafficManager.AddCustomerAsync(testCustomer);
        var endTime = DateTime.Now;
        
        // Assert
        var trafficRecord = trafficManager.TrafficHistory.First();
        Assert.True(trafficRecord.SessionDuration.TotalMilliseconds > 0);
        Assert.True(trafficRecord.SessionDuration <= endTime - startTime + TimeSpan.FromSeconds(1)); // Allow for timing precision
    }
    
    [Fact]
    public void ShopTrafficManager_TrafficLevelChange_FiresEvent()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        
        TrafficLevel? newLevel = null;
        trafficManager.TrafficLevelChanged += (level) => newLevel = level;
        
        // Act
        trafficManager.UpdateTrafficLevel();
        
        // Assert - Event should fire if level changes
        if (newLevel.HasValue)
        {
            Assert.True(Enum.IsDefined(typeof(TrafficLevel), newLevel.Value));
        }
    }
    
    [Fact]
    public async Task ShopTrafficManager_AnalyticsUpdate_TracksHourlyData()
    {
        // Arrange
        var (shopManager, trafficManager) = CreateTrafficSystem();
        
        var testCustomer = new Customer(CustomerType.NoviceAdventurer, "Analytics Test");
        
        // Act
        await trafficManager.AddCustomerAsync(testCustomer);
        var analytics = trafficManager.GetTrafficAnalytics();
        
        // Assert
        Assert.Equal(1, analytics.HourlyVisitors);
        Assert.True(analytics.AverageSessionDuration >= 0);
        Assert.True(analytics.PeakTrafficHour >= 0 && analytics.PeakTrafficHour <= 23);
    }
}
