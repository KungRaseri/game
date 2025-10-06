#nullable enable

using Game.Crafting.Commands;
using Game.Crafting.Handlers;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Crafting.Tests.CQS;
using Game.Items.Models.Materials;

namespace Game.Crafting.Tests.Handlers;

/// <summary>
/// Tests for command handlers in the crafting system.
/// </summary>
public class CommandHandlerTests
{
    private readonly Mock<CraftingStation> _mockCraftingStation;
    private readonly Mock<RecipeManager> _mockRecipeManager;

    public CommandHandlerTests()
    {
        _mockCraftingStation = new Mock<CraftingStation>();
        _mockRecipeManager = new Mock<RecipeManager>();
        TestHelpers.SetupTestLogging();
    }

    #region QueueCraftingOrderCommandHandler Tests

    [Fact]
    public void QueueCraftingOrderCommandHandler_Constructor_WithNullCraftingStation_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new QueueCraftingOrderCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task QueueCraftingOrderCommandHandler_HandleAsync_ValidCommand_ReturnsOrderId()
    {
        // Arrange
        var expectedOrder = TestHelpers.CreateTestOrder("order-123");
        var recipe = TestHelpers.CreateTestRecipe();
        var materials = new Dictionary<string, Material>
        {
            ["material1"] = TestHelpers.CreateTestMaterial()
        };

        _mockCraftingStation
            .Setup(x => x.QueueCraftingOrder(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, Material>>()))
            .Returns(expectedOrder);

        var command = new QueueCraftingOrderCommand
        {
            RecipeId = recipe.RecipeId,
            Materials = materials,
            Priority = 1
        };

        var handler = new QueueCraftingOrderCommandHandler(_mockCraftingStation.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedOrder.OrderId);
        _mockCraftingStation.Verify(
            x => x.QueueCraftingOrder(recipe.RecipeId, materials),
            Times.Once);
    }

    [Fact]
    public async Task QueueCraftingOrderCommandHandler_HandleAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var command = new QueueCraftingOrderCommand
        {
            RecipeId = "test_recipe",
            Materials = new Dictionary<string, Material>(),
            Priority = 0
        };

        var expectedOrder = TestHelpers.CreateTestOrder("order-id");
        _mockCraftingStation
            .Setup(x => x.QueueCraftingOrder(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, Material>>()))
            .Returns(expectedOrder);

        var handler = new QueueCraftingOrderCommandHandler(_mockCraftingStation.Object);
        using var cts = new CancellationTokenSource();

        // Act & Assert - Should not throw
        var result = await handler.HandleAsync(command, cts.Token);
        result.Should().Be(expectedOrder.OrderId);
    }

    #endregion

    #region CancelCraftingOrderCommandHandler Tests

    [Fact]
    public void CancelCraftingOrderCommandHandler_Constructor_WithNullCraftingStation_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new CancelCraftingOrderCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task CancelCraftingOrderCommandHandler_HandleAsync_ValidCommand_CancelsOrder()
    {
        // Arrange
        var orderId = "order-123";
        var command = new CancelCraftingOrderCommand { OrderId = orderId };
        var handler = new CancelCraftingOrderCommandHandler(_mockCraftingStation.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockCraftingStation.Verify(x => x.CancelOrder(orderId), Times.Once);
    }

    #endregion

    #region CancelAllCraftingOrdersCommandHandler Tests

    [Fact]
    public void CancelAllCraftingOrdersCommandHandler_Constructor_WithNullCraftingStation_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new CancelAllCraftingOrdersCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task CancelAllCraftingOrdersCommandHandler_HandleAsync_ValidCommand_CancelsAllOrders()
    {
        // Arrange
        var command = new CancelAllCraftingOrdersCommand();
        var handler = new CancelAllCraftingOrdersCommandHandler(_mockCraftingStation.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockCraftingStation.Verify(x => x.CancelAllOrders(), Times.Once);
    }

    #endregion

    #region AddRecipeCommandHandler Tests

    [Fact]
    public void AddRecipeCommandHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new AddRecipeCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRecipeCommandHandler_HandleAsync_ValidCommand_AddsRecipe()
    {
        // Arrange
        var recipe = TestHelpers.CreateTestRecipe();
        var command = new AddRecipeCommand { Recipe = recipe, StartUnlocked = true };
        var handler = new AddRecipeCommandHandler(_mockRecipeManager.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockRecipeManager.Verify(x => x.AddRecipe(recipe, true), Times.Once);
    }

    [Fact]
    public async Task AddRecipeCommandHandler_HandleAsync_WithoutStartUnlocked_AddsRecipeWithDefaultValue()
    {
        // Arrange
        var recipe = TestHelpers.CreateTestRecipe();
        var command = new AddRecipeCommand { Recipe = recipe }; // StartUnlocked defaults to false
        var handler = new AddRecipeCommandHandler(_mockRecipeManager.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockRecipeManager.Verify(x => x.AddRecipe(recipe, false), Times.Once);
    }

    #endregion

    #region UnlockRecipeCommandHandler Tests

    [Fact]
    public void UnlockRecipeCommandHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new UnlockRecipeCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task UnlockRecipeCommandHandler_HandleAsync_ValidCommand_UnlocksRecipe()
    {
        // Arrange
        var recipeId = "test_recipe";
        var command = new UnlockRecipeCommand { RecipeId = recipeId };
        var handler = new UnlockRecipeCommandHandler(_mockRecipeManager.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockRecipeManager.Verify(x => x.UnlockRecipe(recipeId), Times.Once);
    }

    #endregion

    #region LockRecipeCommandHandler Tests

    [Fact]
    public void LockRecipeCommandHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new LockRecipeCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LockRecipeCommandHandler_HandleAsync_ValidCommand_LocksRecipe()
    {
        // Arrange
        var recipeId = "test_recipe";
        var command = new LockRecipeCommand { RecipeId = recipeId };
        var handler = new LockRecipeCommandHandler(_mockRecipeManager.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
        _mockRecipeManager.Verify(x => x.LockRecipe(recipeId), Times.Once);
    }

    #endregion

    #region DiscoverRecipesCommandHandler Tests

    [Fact]
    public void DiscoverRecipesCommandHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new DiscoverRecipesCommandHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task DiscoverRecipesCommandHandler_HandleAsync_ValidCommand_ReturnsDiscoveredCount()
    {
        // Arrange
        var discoveredRecipes = new List<Recipe>
        {
            TestHelpers.CreateTestRecipe("recipe1"),
            TestHelpers.CreateTestRecipe("recipe2"),
            TestHelpers.CreateTestRecipe("recipe3")
        };
        var expectedCount = discoveredRecipes.Count;
        
        var materials = new List<Material>
        {
            TestHelpers.CreateTestMaterial("material1"),
            TestHelpers.CreateTestMaterial("material2")
        };

        _mockRecipeManager
            .Setup(x => x.DiscoverRecipes(It.IsAny<IEnumerable<Material>>()))
            .Returns(discoveredRecipes);

        var command = new DiscoverRecipesCommand { AvailableMaterials = materials };
        var handler = new DiscoverRecipesCommandHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedCount);
        _mockRecipeManager.Verify(x => x.DiscoverRecipes(materials), Times.Once);
    }

    #endregion

    #region General Handler Tests

    [Fact]
    public async Task AllCommandHandlers_HandleAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var testOrder = TestHelpers.CreateTestOrder("order-id");
        _mockCraftingStation.Setup(x => x.QueueCraftingOrder(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, Material>>()))
            .Returns(testOrder);
        _mockRecipeManager.Setup(x => x.DiscoverRecipes(It.IsAny<IEnumerable<Material>>()))
            .Returns(new List<Recipe>());

        var queueHandler = new QueueCraftingOrderCommandHandler(_mockCraftingStation.Object);
        var cancelHandler = new CancelCraftingOrderCommandHandler(_mockCraftingStation.Object);
        var cancelAllHandler = new CancelAllCraftingOrdersCommandHandler(_mockCraftingStation.Object);
        var addRecipeHandler = new AddRecipeCommandHandler(_mockRecipeManager.Object);
        var unlockHandler = new UnlockRecipeCommandHandler(_mockRecipeManager.Object);
        var lockHandler = new LockRecipeCommandHandler(_mockRecipeManager.Object);
        var discoverHandler = new DiscoverRecipesCommandHandler(_mockRecipeManager.Object);

        // Act & Assert - Should not throw
        await queueHandler.HandleAsync(new QueueCraftingOrderCommand 
        { 
            RecipeId = "test", 
            Materials = new Dictionary<string, Material>() 
        }, cancellationToken);

        await cancelHandler.HandleAsync(new CancelCraftingOrderCommand { OrderId = "test" }, cancellationToken);
        await cancelAllHandler.HandleAsync(new CancelAllCraftingOrdersCommand(), cancellationToken);
        await addRecipeHandler.HandleAsync(new AddRecipeCommand { Recipe = TestHelpers.CreateTestRecipe() }, cancellationToken);
        await unlockHandler.HandleAsync(new UnlockRecipeCommand { RecipeId = "test" }, cancellationToken);
        await lockHandler.HandleAsync(new LockRecipeCommand { RecipeId = "test" }, cancellationToken);
        await discoverHandler.HandleAsync(new DiscoverRecipesCommand { AvailableMaterials = new List<Material>() }, cancellationToken);
    }

    #endregion
}
