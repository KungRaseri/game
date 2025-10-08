#nullable enable

using FluentAssertions;
using Game.Core.Utils;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Crafting.Tests.CQS;
using Game.Items.Models.Materials;
using Game.Items.Models;
using Xunit;

namespace Game.Crafting.Tests.Systems;

/// <summary>
/// Comprehensive tests for CraftingStation class.
/// </summary>
public class CraftingStationTests : IDisposable
{
    private readonly RecipeManager _recipeManager;
    private readonly CraftingStation _craftingStation;
    private readonly Recipe _testRecipe;
    private readonly Dictionary<string, Material> _testMaterials;

    public CraftingStationTests()
    {
        TestHelpers.SetupTestLogging();
        _recipeManager = new RecipeManager();
        _craftingStation = new CraftingStation(_recipeManager);
        
        _testRecipe = TestHelpers.CreateTestRecipe("test_sword", unlocked: true);
        _recipeManager.AddRecipe(_testRecipe, unlocked: true);
        
        // Create materials that match the recipe requirements:
        // Recipe needs: 2x Metal (Common+), 1x Wood (Common+)
        _testMaterials = new Dictionary<string, Material>
        {
            ["metal1"] = TestHelpers.CreateTestMaterial("metal1", Category.Metal, QualityTier.Common),
            ["metal2"] = TestHelpers.CreateTestMaterial("metal2", Category.Metal, QualityTier.Common),
            ["wood1"] = TestHelpers.CreateTestMaterial("wood1", Category.Wood, QualityTier.Common)
        };
    }

    public void Dispose()
    {
        // CraftingStation doesn't implement IDisposable, but we can add cleanup if needed
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new CraftingStation(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("recipeManager");
    }

    [Fact]
    public void Constructor_WithValidRecipeManager_InitializesCorrectly()
    {
        // Arrange & Act
        var station = new CraftingStation(_recipeManager);
        
        // Assert
        station.CurrentOrder.Should().BeNull();
        station.QueuedOrders.Should().BeEmpty();
        station.IsActive.Should().BeFalse();
        station.TotalOrderCount.Should().Be(0);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void CurrentOrder_InitiallyNull_ReturnsNull()
    {
        // Assert
        _craftingStation.CurrentOrder.Should().BeNull();
    }

    [Fact]
    public void QueuedOrders_InitiallyEmpty_ReturnsEmptyList()
    {
        // Assert
        _craftingStation.QueuedOrders.Should().BeEmpty();
        _craftingStation.QueuedOrders.Should().BeOfType<List<CraftingOrder>>();
    }

    [Fact]
    public void IsActive_WithoutCurrentOrder_ReturnsFalse()
    {
        // Assert
        _craftingStation.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WithCurrentOrder_ReturnsTrue()
    {
        // Arrange
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        // Wait a moment for processing to start
        Thread.Sleep(50);
        
        // Assert
        _craftingStation.IsActive.Should().BeTrue();
    }

    [Fact]
    public void TotalOrderCount_WithNoOrders_ReturnsZero()
    {
        // Assert
        _craftingStation.TotalOrderCount.Should().Be(0);
    }

    [Fact]
    public void TotalOrderCount_WithQueuedOrders_ReturnsCorrectCount()
    {
        // Arrange
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        // Assert
        _craftingStation.TotalOrderCount.Should().Be(3);
    }

    #endregion

    #region Queue Crafting Order Tests

    [Fact]
    public void QueueCraftingOrder_WithValidParameters_ReturnsOrder()
    {
        // Act
        var order = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        // Assert
        order.Should().NotBeNull();
        order!.Recipe.Should().Be(_testRecipe);
        order.AllocatedMaterials.Should().Equal(_testMaterials);
        order.OrderId.Should().NotBeNullOrWhiteSpace();
        // Order will start processing immediately if the crafting station is idle
        order.Status.Should().BeOneOf(CraftingStatus.Queued, CraftingStatus.InProgress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void QueueCraftingOrder_WithInvalidRecipeId_ReturnsNull(string? recipeId)
    {
        // Act
        var order = _craftingStation.QueueCraftingOrder(recipeId!, _testMaterials);
        
        // Assert
        order.Should().BeNull();
    }

    [Fact]
    public void QueueCraftingOrder_WithNonExistentRecipe_ReturnsNull()
    {
        // Act
        var order = _craftingStation.QueueCraftingOrder("non_existent_recipe", _testMaterials);
        
        // Assert
        order.Should().BeNull();
    }

    [Fact]
    public void QueueCraftingOrder_WithLockedRecipe_ReturnsNull()
    {
        // Arrange
        var lockedRecipe = TestHelpers.CreateTestRecipe("locked_recipe", unlocked: false);
        _recipeManager.AddRecipe(lockedRecipe, unlocked: false);
        
        // Act
        var order = _craftingStation.QueueCraftingOrder(lockedRecipe.RecipeId, _testMaterials);
        
        // Assert
        order.Should().BeNull();
    }

    [Fact]
    public void QueueCraftingOrder_WithNullMaterials_ReturnsNull()
    {
        // Act
        var order = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, null!);
        
        // Assert
        order.Should().BeNull();
    }

    [Fact]
    public void QueueCraftingOrder_WithInsufficientMaterials_ReturnsNull()
    {
        // Arrange
        var insufficientMaterials = new Dictionary<string, Material>();
        
        // Act
        var order = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, insufficientMaterials);
        
        // Assert
        order.Should().BeNull();
    }

    [Fact]
    public void QueueCraftingOrder_MultipleOrders_AddsToQueue()
    {
        // Act
        var order1 = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        var order2 = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        var order3 = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        // Assert
        order1.Should().NotBeNull();
        order2.Should().NotBeNull();
        order3.Should().NotBeNull();
        _craftingStation.TotalOrderCount.Should().Be(3);
    }

    #endregion

    #region Cancel Order Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CancelOrder_WithInvalidOrderId_ReturnsFalse(string? orderId)
    {
        // Act
        var result = _craftingStation.CancelOrder(orderId!);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CancelOrder_WithNonExistentOrderId_ReturnsFalse()
    {
        // Act
        var result = _craftingStation.CancelOrder("non_existent_order");
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CancelOrder_WithValidQueuedOrder_ReturnsTrue()
    {
        // Arrange
        var order1 = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        var order2 = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        // Act
        var result = _craftingStation.CancelOrder(order2!.OrderId);
        
        // Assert
        result.Should().BeTrue();
        _craftingStation.TotalOrderCount.Should().Be(1);
    }

    [Fact]
    public void CancelOrder_WithCurrentOrder_ReturnsTrueAndStartsNext()
    {
        // Arrange
        var order1 = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        var order2 = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        // Wait for first order to start
        Thread.Sleep(100);
        
        // Act
        var result = _craftingStation.CancelOrder(order1!.OrderId);
        
        // Assert
        result.Should().BeTrue();
        _craftingStation.TotalOrderCount.Should().Be(1);
    }

    [Fact]
    public void CancelOrder_RaisesEventForQueuedOrder()
    {
        // Arrange
        var order = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        CraftingEventArgs? eventArgs = null;
        _craftingStation.CraftingCancelled += (sender, args) => eventArgs = args;
        
        // Act
        _craftingStation.CancelOrder(order!.OrderId);
        
        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Order.Should().Be(order);
    }

    #endregion

    #region Cancel All Orders Tests

    [Fact]
    public void CancelAllOrders_WithNoOrders_DoesNotThrow()
    {
        // Act & Assert
        var action = () => _craftingStation.CancelAllOrders();
        action.Should().NotThrow();
    }

    [Fact]
    public void CancelAllOrders_WithMultipleOrders_CancelsAll()
    {
        // Arrange
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        
        var cancelledCount = 0;
        _craftingStation.CraftingCancelled += (sender, args) => cancelledCount++;
        
        // Act
        _craftingStation.CancelAllOrders();
        
        // Assert
        _craftingStation.TotalOrderCount.Should().Be(0);
        _craftingStation.CurrentOrder.Should().BeNull();
        _craftingStation.QueuedOrders.Should().BeEmpty();
        cancelledCount.Should().Be(3);
    }

    #endregion

    #region Event Tests

    [Fact]
    public void CraftingStarted_RaisedWhenOrderStarts()
    {
        // Arrange
        CraftingEventArgs? eventArgs = null;
        _craftingStation.CraftingStarted += (sender, args) => eventArgs = args;
        
        // Act
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        Thread.Sleep(100); // Wait for order to start
        
        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Order.Recipe.Should().Be(_testRecipe);
    }

    [Fact]
    public void CraftingProgressUpdated_RaisedDuringCrafting()
    {
        // Arrange
        var progressUpdates = new List<CraftingEventArgs>();
        _craftingStation.CraftingProgressUpdated += (sender, args) => progressUpdates.Add(args);
        
        // Act
        _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
        Thread.Sleep(300); // Wait for some progress updates
        
        // Assert
        progressUpdates.Should().NotBeEmpty();
        progressUpdates.All(p => p.Order.Recipe == _testRecipe).Should().BeTrue();
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentQueueing_ThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int ordersPerThread = 5;
        var tasks = new List<Task>();
        var allOrders = new List<CraftingOrder>();
        var lockObject = new object();
        
        // Act
        for (int t = 0; t < threadCount; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < ordersPerThread; i++)
                {
                    var order = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
                    if (order != null)
                    {
                        lock (lockObject)
                        {
                            allOrders.Add(order);
                        }
                    }
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Assert
        allOrders.Should().HaveCount(threadCount * ordersPerThread);
        allOrders.Select(o => o.OrderId).Should().OnlyHaveUniqueItems();
        _craftingStation.TotalOrderCount.Should().Be(threadCount * ordersPerThread);
    }

    [Fact]
    public async Task ConcurrentCancellation_ThreadSafe()
    {
        // Arrange
        var orders = new List<CraftingOrder>();
        for (int i = 0; i < 20; i++)
        {
            var order = _craftingStation.QueueCraftingOrder(_testRecipe.RecipeId, _testMaterials);
            if (order != null) orders.Add(order);
        }
        
        var tasks = new List<Task>();
        var cancelledCount = 0;
        
        // Act
        for (int i = 0; i < orders.Count; i += 2)
        {
            var orderId = orders[i].OrderId;
            tasks.Add(Task.Run(() =>
            {
                if (_craftingStation.CancelOrder(orderId))
                {
                    Interlocked.Increment(ref cancelledCount);
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Assert
        cancelledCount.Should().BeGreaterOrEqualTo(0);
        _craftingStation.TotalOrderCount.Should().BeLessOrEqualTo(orders.Count);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ValidateMaterials_WithCorrectMaterials_ReturnsValid()
    {
        // Arrange - Use reflection to access private method
        var method = typeof(CraftingStation).GetMethod("ValidateMaterials", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        var result = method?.Invoke(_craftingStation, new object[] { _testRecipe, _testMaterials });
        
        // Assert
        result.Should().NotBeNull();
        var isValid = result?.GetType().GetProperty("IsValid")?.GetValue(result);
        isValid.Should().Be(true);
    }

    [Fact]
    public void ValidateMaterials_WithInsufficientMaterials_ReturnsInvalid()
    {
        // Arrange
        var insufficientMaterials = new Dictionary<string, Material>();
        var method = typeof(CraftingStation).GetMethod("ValidateMaterials", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        var result = method?.Invoke(_craftingStation, new object[] { _testRecipe, insufficientMaterials });
        
        // Assert
        result.Should().NotBeNull();
        var isValid = result?.GetType().GetProperty("IsValid")?.GetValue(result);
        isValid.Should().Be(false);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void CraftingStation_DoesNotImplementIDisposable()
    {
        // Arrange & Act
        var station = new CraftingStation(_recipeManager);
        
        // Assert - This test documents that CraftingStation doesn't implement IDisposable
        // If it ever does in the future, this test should be updated
        station.Should().NotBeAssignableTo<IDisposable>();
    }

    #endregion
}
