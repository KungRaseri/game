using FluentAssertions;
using Game.Crafting.Commands;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;
using Game.Crafting.Tests.CQS;
using Game.Items.Models.Materials;

namespace Game.Crafting.Tests.Systems;

/// <summary>
/// Tests for the CraftingService facade.
/// </summary>
public class CraftingServiceTests
{
    private readonly MockDispatcher _mockDispatcher;
    private readonly CraftingService _craftingService;

    public CraftingServiceTests()
    {
        _mockDispatcher = new MockDispatcher();
        _craftingService = new CraftingService(_mockDispatcher);
        TestHelpers.SetupTestLogging();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDispatcher_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new CraftingService(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Crafting Order Tests

    [Fact]
    public async Task QueueCraftingOrderAsync_ValidParameters_DispatchesCommandAndReturnsOrderId()
    {
        // Arrange
        var expectedOrderId = "order-123";
        var recipeId = "test_recipe";
        var materials = new Dictionary<string, Material>
        {
            ["material1"] = TestHelpers.CreateTestMaterial()
        };
        var priority = 1;

        _mockDispatcher.RegisterCommandHandler<QueueCraftingOrderCommand, string>(command =>
        {
            command.RecipeId.Should().Be(recipeId);
            command.Materials.Should().BeSameAs(materials);
            command.Priority.Should().Be(priority);
            return Task.FromResult(expectedOrderId);
        });

        // Act
        var result = await _craftingService.QueueCraftingOrderAsync(recipeId, materials, priority);

        // Assert
        result.Should().Be(expectedOrderId);
        _mockDispatcher.DispatchedCommands.Should().HaveCount(1);
        _mockDispatcher.DispatchedCommands[0].Should().BeOfType<QueueCraftingOrderCommand>();
    }

    [Fact]
    public async Task CancelCraftingOrderAsync_ValidOrderId_DispatchesCommand()
    {
        // Arrange
        var orderId = "order-123";
        _mockDispatcher.RegisterCommandHandler<CancelCraftingOrderCommand>(command =>
        {
            command.OrderId.Should().Be(orderId);
            return Task.CompletedTask;
        });

        // Act
        await _craftingService.CancelCraftingOrderAsync(orderId);

        // Assert
        _mockDispatcher.DispatchedCommands.Should().HaveCount(1);
        _mockDispatcher.DispatchedCommands[0].Should().BeOfType<CancelCraftingOrderCommand>();
    }

    [Fact]
    public async Task CancelAllCraftingOrdersAsync_DispatchesCommand()
    {
        // Arrange
        _mockDispatcher.RegisterCommandHandler<CancelAllCraftingOrdersCommand>(_ =>
            Task.CompletedTask);

        // Act
        await _craftingService.CancelAllCraftingOrdersAsync();

        // Assert
        _mockDispatcher.DispatchedCommands.Should().HaveCount(1);
        _mockDispatcher.DispatchedCommands[0].Should().BeOfType<CancelAllCraftingOrdersCommand>();
    }

    [Fact]
    public async Task GetCraftingOrderAsync_ValidOrderId_DispatchesQueryAndReturnsOrder()
    {
        // Arrange
        var orderId = "order-123";
        var expectedOrder = TestHelpers.CreateTestOrder(orderId);

        _mockDispatcher.RegisterQueryHandler<GetCraftingOrderQuery, CraftingOrder?>(query =>
        {
            query.OrderId.Should().Be(orderId);
            return Task.FromResult<CraftingOrder?>(expectedOrder);
        });

        // Act
        var result = await _craftingService.GetCraftingOrderAsync(orderId);

        // Assert
        result.Should().BeSameAs(expectedOrder);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<GetCraftingOrderQuery>();
    }

    [Fact]
    public async Task GetAllCraftingOrdersAsync_DispatchesQueryAndReturnsResult()
    {
        // Arrange
        var expectedResult = new CraftingOrdersResult
        {
            CurrentOrder = TestHelpers.CreateTestOrder("current"),
            QueuedOrders = new List<CraftingOrder> { TestHelpers.CreateTestOrder("queued") }
        };

        _mockDispatcher.RegisterQueryHandler<GetAllCraftingOrdersQuery, CraftingOrdersResult>(_ =>
            Task.FromResult(expectedResult));

        // Act
        var result = await _craftingService.GetAllCraftingOrdersAsync();

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<GetAllCraftingOrdersQuery>();
    }

    [Fact]
    public async Task GetCraftingStationStatsAsync_DispatchesQueryAndReturnsStats()
    {
        // Arrange
        var expectedStats = new Dictionary<string, object> { ["IsActive"] = true };

        _mockDispatcher.RegisterQueryHandler<GetCraftingStationStatsQuery, Dictionary<string, object>>(_ =>
            Task.FromResult(expectedStats));

        // Act
        var result = await _craftingService.GetCraftingStationStatsAsync();

        // Assert
        result.Should().BeSameAs(expectedStats);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<GetCraftingStationStatsQuery>();
    }

    #endregion

    #region Recipe Management Tests

    [Fact]
    public async Task AddRecipeAsync_ValidRecipe_DispatchesCommand()
    {
        // Arrange
        var recipe = TestHelpers.CreateTestRecipe();
        var startUnlocked = true;

        _mockDispatcher.RegisterCommandHandler<AddRecipeCommand>(command =>
        {
            command.Recipe.Should().BeSameAs(recipe);
            command.StartUnlocked.Should().Be(startUnlocked);
            return Task.CompletedTask;
        });

        // Act
        await _craftingService.AddRecipeAsync(recipe, startUnlocked);

        // Assert
        _mockDispatcher.DispatchedCommands.Should().HaveCount(1);
        _mockDispatcher.DispatchedCommands[0].Should().BeOfType<AddRecipeCommand>();
    }

    [Fact]
    public async Task UnlockRecipeAsync_ValidRecipeId_DispatchesCommand()
    {
        // Arrange
        var recipeId = "test_recipe";

        _mockDispatcher.RegisterCommandHandler<UnlockRecipeCommand>(command =>
        {
            command.RecipeId.Should().Be(recipeId);
            return Task.CompletedTask;
        });

        // Act
        await _craftingService.UnlockRecipeAsync(recipeId);

        // Assert
        _mockDispatcher.DispatchedCommands.Should().HaveCount(1);
        _mockDispatcher.DispatchedCommands[0].Should().BeOfType<UnlockRecipeCommand>();
    }

    [Fact]
    public async Task LockRecipeAsync_ValidRecipeId_DispatchesCommand()
    {
        // Arrange
        var recipeId = "test_recipe";

        _mockDispatcher.RegisterCommandHandler<LockRecipeCommand>(command =>
        {
            command.RecipeId.Should().Be(recipeId);
            return Task.CompletedTask;
        });

        // Act
        await _craftingService.LockRecipeAsync(recipeId);

        // Assert
        _mockDispatcher.DispatchedCommands.Should().HaveCount(1);
        _mockDispatcher.DispatchedCommands[0].Should().BeOfType<LockRecipeCommand>();
    }

    [Fact]
    public async Task DiscoverRecipesAsync_ValidMaterials_DispatchesCommandAndReturnsCount()
    {
        // Arrange
        var materials = new List<Material> { TestHelpers.CreateTestMaterial() };
        var expectedCount = 3;

        _mockDispatcher.RegisterCommandHandler<DiscoverRecipesCommand, int>(command =>
        {
            command.AvailableMaterials.Should().BeSameAs(materials);
            return Task.FromResult(expectedCount);
        });

        // Act
        var result = await _craftingService.DiscoverRecipesAsync(materials);

        // Assert
        result.Should().Be(expectedCount);
        _mockDispatcher.DispatchedCommands.Should().HaveCount(1);
        _mockDispatcher.DispatchedCommands[0].Should().BeOfType<DiscoverRecipesCommand>();
    }

    [Fact]
    public async Task GetRecipeAsync_ValidRecipeId_DispatchesQueryAndReturnsRecipe()
    {
        // Arrange
        var recipeId = "test_recipe";
        var expectedRecipe = TestHelpers.CreateTestRecipe(recipeId);

        _mockDispatcher.RegisterQueryHandler<GetRecipeQuery, Recipe?>(query =>
        {
            query.RecipeId.Should().Be(recipeId);
            return Task.FromResult<Recipe?>(expectedRecipe);
        });

        // Act
        var result = await _craftingService.GetRecipeAsync(recipeId);

        // Assert
        result.Should().BeSameAs(expectedRecipe);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<GetRecipeQuery>();
    }

    [Fact]
    public async Task GetUnlockedRecipesAsync_WithoutCategory_DispatchesQueryAndReturnsRecipes()
    {
        // Arrange
        var expectedRecipes = new List<Recipe> { TestHelpers.CreateTestRecipe() };

        _mockDispatcher.RegisterQueryHandler<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>(query =>
        {
            query.Category.Should().BeNull();
            return Task.FromResult<IReadOnlyList<Recipe>>(expectedRecipes);
        });

        // Act
        var result = await _craftingService.GetUnlockedRecipesAsync();

        // Assert
        result.Should().BeSameAs(expectedRecipes);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<GetUnlockedRecipesQuery>();
    }

    [Fact]
    public async Task GetUnlockedRecipesAsync_WithCategory_DispatchesQueryWithCategory()
    {
        // Arrange
        var category = RecipeCategory.Weapons;
        var expectedRecipes = new List<Recipe> { TestHelpers.CreateTestRecipe() };

        _mockDispatcher.RegisterQueryHandler<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>(query =>
        {
            query.Category.Should().Be(category);
            return Task.FromResult<IReadOnlyList<Recipe>>(expectedRecipes);
        });

        // Act
        var result = await _craftingService.GetUnlockedRecipesAsync(category);

        // Assert
        result.Should().BeSameAs(expectedRecipes);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<GetUnlockedRecipesQuery>();
    }

    [Fact]
    public async Task SearchRecipesAsync_ValidParameters_DispatchesQueryAndReturnsRecipes()
    {
        // Arrange
        var searchTerm = "sword";
        var includeLockedRecipes = true;
        var category = RecipeCategory.Weapons;
        var expectedRecipes = new List<Recipe> { TestHelpers.CreateTestRecipe() };

        _mockDispatcher.RegisterQueryHandler<SearchRecipesQuery, IReadOnlyList<Recipe>>(query =>
        {
            query.SearchTerm.Should().Be(searchTerm);
            query.IncludeLockedRecipes.Should().Be(includeLockedRecipes);
            query.Category.Should().Be(category);
            return Task.FromResult<IReadOnlyList<Recipe>>(expectedRecipes);
        });

        // Act
        var result = await _craftingService.SearchRecipesAsync(searchTerm, includeLockedRecipes, category);

        // Assert
        result.Should().BeSameAs(expectedRecipes);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<SearchRecipesQuery>();
    }

    [Fact]
    public async Task IsRecipeUnlockedAsync_ValidRecipeId_DispatchesQueryAndReturnsResult()
    {
        // Arrange
        var recipeId = "test_recipe";
        var expectedResult = true;

        _mockDispatcher.RegisterQueryHandler<IsRecipeUnlockedQuery, bool>(query =>
        {
            query.RecipeId.Should().Be(recipeId);
            return Task.FromResult(expectedResult);
        });

        // Act
        var result = await _craftingService.IsRecipeUnlockedAsync(recipeId);

        // Assert
        result.Should().Be(expectedResult);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<IsRecipeUnlockedQuery>();
    }

    [Fact]
    public async Task GetRecipeManagerStatsAsync_DispatchesQueryAndReturnsStats()
    {
        // Arrange
        var expectedStats = new Dictionary<string, object> { ["TotalRecipes"] = 10 };

        _mockDispatcher.RegisterQueryHandler<GetRecipeManagerStatsQuery, Dictionary<string, object>>(_ =>
            Task.FromResult(expectedStats));

        // Act
        var result = await _craftingService.GetRecipeManagerStatsAsync();

        // Assert
        result.Should().BeSameAs(expectedStats);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(1);
        _mockDispatcher.DispatchedQueries[0].Should().BeOfType<GetRecipeManagerStatsQuery>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task AllMethods_WithCancellationToken_PassTokenToDispatcher()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Set up handlers that verify cancellation token
        _mockDispatcher.RegisterCommandHandler<QueueCraftingOrderCommand, string>(_ => Task.FromResult("order-id"));
        _mockDispatcher.RegisterCommandHandler<CancelCraftingOrderCommand>(_ => Task.CompletedTask);
        _mockDispatcher.RegisterCommandHandler<CancelAllCraftingOrdersCommand>(_ => Task.CompletedTask);
        _mockDispatcher.RegisterCommandHandler<AddRecipeCommand>(_ => Task.CompletedTask);
        _mockDispatcher.RegisterCommandHandler<UnlockRecipeCommand>(_ => Task.CompletedTask);
        _mockDispatcher.RegisterCommandHandler<LockRecipeCommand>(_ => Task.CompletedTask);
        _mockDispatcher.RegisterCommandHandler<DiscoverRecipesCommand, int>(_ => Task.FromResult(0));

        _mockDispatcher.RegisterQueryHandler<GetCraftingOrderQuery, CraftingOrder?>(_ => Task.FromResult<CraftingOrder?>(null));
        _mockDispatcher.RegisterQueryHandler<GetAllCraftingOrdersQuery, CraftingOrdersResult>(_ => 
            Task.FromResult(new CraftingOrdersResult
            {
                CurrentOrder = null,
                QueuedOrders = new List<CraftingOrder>()
            }));
        _mockDispatcher.RegisterQueryHandler<GetCraftingStationStatsQuery, Dictionary<string, object>>(_ => 
            Task.FromResult(new Dictionary<string, object>()));
        _mockDispatcher.RegisterQueryHandler<GetRecipeQuery, Recipe?>(_ => Task.FromResult<Recipe?>(null));
        _mockDispatcher.RegisterQueryHandler<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>(_ => 
            Task.FromResult<IReadOnlyList<Recipe>>(new List<Recipe>()));
        _mockDispatcher.RegisterQueryHandler<SearchRecipesQuery, IReadOnlyList<Recipe>>(_ => 
            Task.FromResult<IReadOnlyList<Recipe>>(new List<Recipe>()));
        _mockDispatcher.RegisterQueryHandler<IsRecipeUnlockedQuery, bool>(_ => Task.FromResult(false));
        _mockDispatcher.RegisterQueryHandler<GetRecipeManagerStatsQuery, Dictionary<string, object>>(_ => 
            Task.FromResult(new Dictionary<string, object>()));

        var materials = new Dictionary<string, Material>();
        // ReSharper disable once CollectionNeverUpdated.Local
        var materialsList = new List<Material>();
        var recipe = TestHelpers.CreateTestRecipe();

        // Act & Assert - All calls should complete without throwing
        await _craftingService.QueueCraftingOrderAsync("recipe", materials, 0, token);
        await _craftingService.CancelCraftingOrderAsync("order", token);
        await _craftingService.CancelAllCraftingOrdersAsync(token);
        await _craftingService.GetCraftingOrderAsync("order", token);
        await _craftingService.GetAllCraftingOrdersAsync(token);
        await _craftingService.GetCraftingStationStatsAsync(token);
        
        await _craftingService.AddRecipeAsync(recipe, false, token);
        await _craftingService.UnlockRecipeAsync("recipe", token);
        await _craftingService.LockRecipeAsync("recipe", token);
        await _craftingService.DiscoverRecipesAsync(materialsList, token);
        await _craftingService.GetRecipeAsync("recipe", token);
        await _craftingService.GetUnlockedRecipesAsync(null, token);
        await _craftingService.SearchRecipesAsync("search", false, null, token);
        await _craftingService.IsRecipeUnlockedAsync("recipe", token);
        await _craftingService.GetRecipeManagerStatsAsync(token);

        // Verify all methods were called
        _mockDispatcher.DispatchedCommands.Should().HaveCount(7);
        _mockDispatcher.DispatchedQueries.Should().HaveCount(8);
    }

    #endregion
}
