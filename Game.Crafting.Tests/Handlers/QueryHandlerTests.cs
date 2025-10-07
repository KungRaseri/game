#nullable enable

using Game.Crafting.Handlers;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;
using Game.Crafting.Tests.CQS;

namespace Game.Crafting.Tests.Handlers;

/// <summary>
/// Tests for query handlers in the crafting system.
/// </summary>
public class QueryHandlerTests
{
    private readonly Mock<ICraftingStation> _mockCraftingStation;
    private readonly Mock<IRecipeManager> _mockRecipeManager;

    public QueryHandlerTests()
    {
        _mockCraftingStation = new Mock<ICraftingStation>();
        _mockRecipeManager = new Mock<IRecipeManager>();
        TestHelpers.SetupTestLogging();
    }

    #region GetCraftingOrderQueryHandler Tests

    [Fact]
    public void GetCraftingOrderQueryHandler_Constructor_WithNullCraftingStation_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetCraftingOrderQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetCraftingOrderQueryHandler_HandleAsync_WithExistingOrder_ReturnsOrder()
    {
        // Arrange
        var expectedOrder = TestHelpers.CreateTestOrder("order-123");
        
        _mockCraftingStation
            .Setup(x => x.GetOrder("order-123"))
            .Returns(expectedOrder);

        var query = new GetCraftingOrderQuery { OrderId = "order-123" };
        var handler = new GetCraftingOrderQueryHandler(_mockCraftingStation.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedOrder);
        _mockCraftingStation.Verify(x => x.GetOrder("order-123"), Times.Once);
    }

    [Fact]
    public async Task GetCraftingOrderQueryHandler_HandleAsync_WithNonExistentOrder_ReturnsNull()
    {
        // Arrange
        _mockCraftingStation
            .Setup(x => x.GetOrder("non-existent"))
            .Returns((CraftingOrder?)null);

        var query = new GetCraftingOrderQuery { OrderId = "non-existent" };
        var handler = new GetCraftingOrderQueryHandler(_mockCraftingStation.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
        _mockCraftingStation.Verify(x => x.GetOrder("non-existent"), Times.Once);
    }

    #endregion

    #region GetAllCraftingOrdersQueryHandler Tests

    [Fact]
    public void GetAllCraftingOrdersQueryHandler_Constructor_WithNullCraftingStation_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetAllCraftingOrdersQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAllCraftingOrdersQueryHandler_HandleAsync_ReturnsOrdersResult()
    {
        // Arrange
        var currentOrder = TestHelpers.CreateTestOrder("current");
        var queuedOrders = new List<CraftingOrder>
        {
            TestHelpers.CreateTestOrder("queued1"),
            TestHelpers.CreateTestOrder("queued2")
        };

        _mockCraftingStation.Setup(x => x.CurrentOrder).Returns(currentOrder);
        _mockCraftingStation.Setup(x => x.QueuedOrders).Returns(queuedOrders);

        var query = new GetAllCraftingOrdersQuery();
        var handler = new GetAllCraftingOrdersQueryHandler(_mockCraftingStation.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.CurrentOrder.Should().BeSameAs(currentOrder);
        result.QueuedOrders.Should().BeSameAs(queuedOrders);
    }

    #endregion

    #region GetCraftingStationStatsQueryHandler Tests

    [Fact]
    public void GetCraftingStationStatsQueryHandler_Constructor_WithNullCraftingStation_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetCraftingStationStatsQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetCraftingStationStatsQueryHandler_HandleAsync_ReturnsStatistics()
    {
        // Arrange
        var expectedStats = new Dictionary<string, object>
        {
            ["IsActive"] = true,
            ["TotalOrders"] = 5,
            ["QueuedOrders"] = 3
        };

        _mockCraftingStation
            .Setup(x => x.GetStatistics())
            .Returns(expectedStats);

        var query = new GetCraftingStationStatsQuery();
        var handler = new GetCraftingStationStatsQueryHandler(_mockCraftingStation.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedStats);
        _mockCraftingStation.Verify(x => x.GetStatistics(), Times.Once);
    }

    #endregion

    #region GetRecipeQueryHandler Tests

    [Fact]
    public void GetRecipeQueryHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetRecipeQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetRecipeQueryHandler_HandleAsync_WithExistingRecipe_ReturnsRecipe()
    {
        // Arrange
        var expectedRecipe = TestHelpers.CreateTestRecipe("recipe-123");
        
        _mockRecipeManager
            .Setup(x => x.GetRecipe("recipe-123"))
            .Returns(expectedRecipe);

        var query = new GetRecipeQuery { RecipeId = "recipe-123" };
        var handler = new GetRecipeQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedRecipe);
        _mockRecipeManager.Verify(x => x.GetRecipe("recipe-123"), Times.Once);
    }

    [Fact]
    public async Task GetRecipeQueryHandler_HandleAsync_WithNonExistentRecipe_ReturnsNull()
    {
        // Arrange
        _mockRecipeManager
            .Setup(x => x.GetRecipe("non-existent"))
            .Returns((Recipe?)null);

        var query = new GetRecipeQuery { RecipeId = "non-existent" };
        var handler = new GetRecipeQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
        _mockRecipeManager.Verify(x => x.GetRecipe("non-existent"), Times.Once);
    }

    #endregion

    #region GetUnlockedRecipesQueryHandler Tests

    [Fact]
    public void GetUnlockedRecipesQueryHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetUnlockedRecipesQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetUnlockedRecipesQueryHandler_HandleAsync_WithoutCategory_ReturnsAllUnlockedRecipes()
    {
        // Arrange
        var expectedRecipes = new List<Recipe>
        {
            TestHelpers.CreateTestRecipe("recipe1", unlocked: true),
            TestHelpers.CreateTestRecipe("recipe2", unlocked: true)
        };

        _mockRecipeManager
            .Setup(x => x.UnlockedRecipes)
            .Returns(expectedRecipes);

        var query = new GetUnlockedRecipesQuery();
        var handler = new GetUnlockedRecipesQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedRecipes);
        _mockRecipeManager.Verify(x => x.UnlockedRecipes, Times.Once);
    }

    [Fact]
    public async Task GetUnlockedRecipesQueryHandler_HandleAsync_WithCategory_ReturnsFilteredRecipes()
    {
        // Arrange
        var expectedRecipes = new List<Recipe>
        {
            TestHelpers.CreateTestRecipe("weapon1", unlocked: true),
            TestHelpers.CreateTestRecipe("weapon2", unlocked: true)
        };

        _mockRecipeManager
            .Setup(x => x.GetUnlockedRecipesByCategory(RecipeCategory.Weapons))
            .Returns(expectedRecipes);

        var query = new GetUnlockedRecipesQuery { Category = RecipeCategory.Weapons };
        var handler = new GetUnlockedRecipesQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedRecipes);
        _mockRecipeManager.Verify(x => x.GetUnlockedRecipesByCategory(RecipeCategory.Weapons), Times.Once);
    }

    #endregion

    #region SearchRecipesQueryHandler Tests

    [Fact]
    public void SearchRecipesQueryHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new SearchRecipesQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SearchRecipesQueryHandler_HandleAsync_ValidSearch_ReturnsMatchingRecipes()
    {
        // Arrange
        var expectedRecipes = new List<Recipe>
        {
            TestHelpers.CreateTestRecipe("sword1", unlocked: true),
            TestHelpers.CreateTestRecipe("sword2", unlocked: true)
        };

        _mockRecipeManager
            .Setup(x => x.SearchRecipes("sword", false))
            .Returns(expectedRecipes);

        var query = new SearchRecipesQuery 
        { 
            SearchTerm = "sword",
            IncludeLockedRecipes = false,
            Category = null
        };
        var handler = new SearchRecipesQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeEquivalentTo(expectedRecipes);
        _mockRecipeManager.Verify(x => x.SearchRecipes("sword", false), Times.Once);
    }

    #endregion

    #region IsRecipeUnlockedQueryHandler Tests

    [Fact]
    public void IsRecipeUnlockedQueryHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new IsRecipeUnlockedQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task IsRecipeUnlockedQueryHandler_HandleAsync_WithUnlockedRecipe_ReturnsTrue()
    {
        // Arrange
        _mockRecipeManager
            .Setup(x => x.IsRecipeUnlocked("unlocked-recipe"))
            .Returns(true);

        var query = new IsRecipeUnlockedQuery { RecipeId = "unlocked-recipe" };
        var handler = new IsRecipeUnlockedQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
        _mockRecipeManager.Verify(x => x.IsRecipeUnlocked("unlocked-recipe"), Times.Once);
    }

    [Fact]
    public async Task IsRecipeUnlockedQueryHandler_HandleAsync_WithLockedRecipe_ReturnsFalse()
    {
        // Arrange
        _mockRecipeManager
            .Setup(x => x.IsRecipeUnlocked("locked-recipe"))
            .Returns(false);

        var query = new IsRecipeUnlockedQuery { RecipeId = "locked-recipe" };
        var handler = new IsRecipeUnlockedQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
        _mockRecipeManager.Verify(x => x.IsRecipeUnlocked("locked-recipe"), Times.Once);
    }

    #endregion

    #region GetRecipeManagerStatsQueryHandler Tests

    [Fact]
    public void GetRecipeManagerStatsQueryHandler_Constructor_WithNullRecipeManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new GetRecipeManagerStatsQueryHandler(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetRecipeManagerStatsQueryHandler_HandleAsync_ReturnsStatistics()
    {
        // Arrange
        var expectedStats = new Dictionary<string, object>
        {
            ["TotalRecipes"] = 10,
            ["UnlockedRecipes"] = 7,
            ["LockedRecipes"] = 3
        };

        _mockRecipeManager
            .Setup(x => x.GetStatistics())
            .Returns(expectedStats);

        var query = new GetRecipeManagerStatsQuery();
        var handler = new GetRecipeManagerStatsQueryHandler(_mockRecipeManager.Object);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Should().BeSameAs(expectedStats);
        _mockRecipeManager.Verify(x => x.GetStatistics(), Times.Once);
    }

    #endregion

    #region General Query Handler Tests

    [Fact]
    public async Task AllQueryHandlers_HandleAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        
        _mockCraftingStation.Setup(x => x.GetOrder(It.IsAny<string>())).Returns((CraftingOrder?)null);
        _mockCraftingStation.Setup(x => x.CurrentOrder).Returns((CraftingOrder?)null);
        _mockCraftingStation.Setup(x => x.QueuedOrders).Returns(new List<CraftingOrder>());
        _mockCraftingStation.Setup(x => x.GetStatistics()).Returns(new Dictionary<string, object>());
        
        _mockRecipeManager.Setup(x => x.GetRecipe(It.IsAny<string>())).Returns((Recipe?)null);
        _mockRecipeManager.Setup(x => x.UnlockedRecipes).Returns(new List<Recipe>());
        _mockRecipeManager.Setup(x => x.GetUnlockedRecipesByCategory(It.IsAny<RecipeCategory>())).Returns(new List<Recipe>());
        _mockRecipeManager.Setup(x => x.SearchRecipes(It.IsAny<string>(), It.IsAny<bool>())).Returns(new List<Recipe>());
        _mockRecipeManager.Setup(x => x.IsRecipeUnlocked(It.IsAny<string>())).Returns(false);
        _mockRecipeManager.Setup(x => x.GetStatistics()).Returns(new Dictionary<string, object>());

        var getCraftingOrderHandler = new GetCraftingOrderQueryHandler(_mockCraftingStation.Object);
        var getAllOrdersHandler = new GetAllCraftingOrdersQueryHandler(_mockCraftingStation.Object);
        var getCraftingStatsHandler = new GetCraftingStationStatsQueryHandler(_mockCraftingStation.Object);
        var getRecipeHandler = new GetRecipeQueryHandler(_mockRecipeManager.Object);
        var getUnlockedHandler = new GetUnlockedRecipesQueryHandler(_mockRecipeManager.Object);
        var searchHandler = new SearchRecipesQueryHandler(_mockRecipeManager.Object);
        var isUnlockedHandler = new IsRecipeUnlockedQueryHandler(_mockRecipeManager.Object);
        var getRecipeStatsHandler = new GetRecipeManagerStatsQueryHandler(_mockRecipeManager.Object);

        // Act & Assert - Should not throw
        await getCraftingOrderHandler.HandleAsync(new GetCraftingOrderQuery { OrderId = "test" }, cancellationToken);
        await getAllOrdersHandler.HandleAsync(new GetAllCraftingOrdersQuery(), cancellationToken);
        await getCraftingStatsHandler.HandleAsync(new GetCraftingStationStatsQuery(), cancellationToken);
        await getRecipeHandler.HandleAsync(new GetRecipeQuery { RecipeId = "test" }, cancellationToken);
        await getUnlockedHandler.HandleAsync(new GetUnlockedRecipesQuery(), cancellationToken);
        await searchHandler.HandleAsync(new SearchRecipesQuery { SearchTerm = "test" }, cancellationToken);
        await isUnlockedHandler.HandleAsync(new IsRecipeUnlockedQuery { RecipeId = "test" }, cancellationToken);
        await getRecipeStatsHandler.HandleAsync(new GetRecipeManagerStatsQuery(), cancellationToken);
    }

    #endregion
}
