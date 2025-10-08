#nullable enable

using FluentAssertions;
using Game.Core.CQS;
using Game.Core.Utils;
using Game.Crafting.Commands;
using Game.Crafting.Extensions;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Game.Crafting.Tests.Integration;

/// <summary>
/// Integration tests that verify the complete CQS workflow with real dependencies.
/// Tests the full lifecycle from command dispatch through handler execution.
/// </summary>
public class CraftingIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IDispatcher _dispatcher;
    private readonly CraftingStation _craftingStation;
    private readonly RecipeManager _recipeManager;

    public CraftingIntegrationTests()
    {
        // Set up test logger backend
        GameLogger.SetBackend(new TestableLoggerBackend());

        // Set up dependency injection container
        var services = new ServiceCollection();
        
        // Add CQS infrastructure
        services.AddSingleton<IDispatcher, Dispatcher>();
        
        // Add crafting services
        services.AddCraftingServices();
        services.AddCraftingCoreSystems();

        _serviceProvider = services.BuildServiceProvider();
        _dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
        _craftingStation = _serviceProvider.GetRequiredService<CraftingStation>();
        _recipeManager = _serviceProvider.GetRequiredService<RecipeManager>();
    }

    [Fact]
    public async Task EndToEnd_RecipeManagement_ShouldWork()
    {
        // Arrange - Create a test recipe
        var testRecipe = CreateTestRecipe();
        
        // Act & Assert - Add recipe to manager
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchCommandAsync(addCommand);

        // Verify recipe was added
        var getRecipeQuery = new GetRecipeQuery { RecipeId = testRecipe.RecipeId };
        var retrievedRecipe = await _dispatcher.DispatchQueryAsync<GetRecipeQuery, Recipe?>(getRecipeQuery);
        retrievedRecipe.Should().NotBeNull();
        retrievedRecipe!.RecipeId.Should().Be(testRecipe.RecipeId);

        // Unlock the recipe
        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.RecipeId };
        await _dispatcher.DispatchCommandAsync(unlockCommand);

        // Verify recipe is unlocked
        var isUnlockedQuery = new IsRecipeUnlockedQuery { RecipeId = testRecipe.RecipeId };
        var isUnlocked = await _dispatcher.DispatchQueryAsync<IsRecipeUnlockedQuery, bool>(isUnlockedQuery);
        isUnlocked.Should().BeTrue();
    }

    [Fact]
    public async Task EndToEnd_CraftingOrder_ShouldWork()
    {
        // Arrange - Create and add a test recipe
        var testRecipe = CreateTestRecipe();
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchCommandAsync(addCommand);

        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.RecipeId };
        await _dispatcher.DispatchCommandAsync(unlockCommand);

        // Create test materials
        var materials = CreateTestMaterials();

        // Act - Queue crafting order
        var queueCommand = new QueueCraftingOrderCommand 
        { 
            RecipeId = testRecipe.RecipeId, 
            Materials = materials
        };
        var orderId = await _dispatcher.DispatchCommandAsync<QueueCraftingOrderCommand, string>(queueCommand);

        // Assert - Verify order was queued
        orderId.Should().NotBeNullOrEmpty();

        var getOrderQuery = new GetCraftingOrderQuery { OrderId = orderId };
        var order = await _dispatcher.DispatchQueryAsync<GetCraftingOrderQuery, CraftingOrder?>(getOrderQuery);
        order.Should().NotBeNull();
        order!.Recipe.RecipeId.Should().Be(testRecipe.RecipeId);

        // Cancel the order
        var cancelCommand = new CancelCraftingOrderCommand { OrderId = orderId };
        await _dispatcher.DispatchCommandAsync(cancelCommand);

        // Verify order was cancelled
        var cancelledOrder = await _dispatcher.DispatchQueryAsync<GetCraftingOrderQuery, CraftingOrder?>(getOrderQuery);
        cancelledOrder.Should().BeNull();
    }

    [Fact]
    public async Task QueryOperations_ShouldNotModifyState()
    {
        // Arrange - Set up initial state
        var testRecipe = CreateTestRecipe();
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchCommandAsync(addCommand);

        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.RecipeId };
        await _dispatcher.DispatchCommandAsync(unlockCommand);

        // Act - Execute multiple queries
        var getRecipeQuery = new GetRecipeQuery { RecipeId = testRecipe.RecipeId };
        var getUnlockedQuery = new GetUnlockedRecipesQuery();
        var isUnlockedQuery = new IsRecipeUnlockedQuery { RecipeId = testRecipe.RecipeId };

        // Execute queries multiple times
        for (int i = 0; i < 3; i++)
        {
            await _dispatcher.DispatchQueryAsync<GetRecipeQuery, Recipe?>(getRecipeQuery);
            await _dispatcher.DispatchQueryAsync<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>(getUnlockedQuery);
            await _dispatcher.DispatchQueryAsync<IsRecipeUnlockedQuery, bool>(isUnlockedQuery);
        }

        // Assert - State should remain unchanged
        var finalRecipe = await _dispatcher.DispatchQueryAsync<GetRecipeQuery, Recipe?>(getRecipeQuery);
        finalRecipe.Should().NotBeNull();
        finalRecipe!.RecipeId.Should().Be(testRecipe.RecipeId);

        var finalUnlocked = await _dispatcher.DispatchQueryAsync<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>(getUnlockedQuery);
        finalUnlocked.Should().HaveCount(1);

        var finalIsUnlocked = await _dispatcher.DispatchQueryAsync<IsRecipeUnlockedQuery, bool>(isUnlockedQuery);
        finalIsUnlocked.Should().BeTrue();
    }

    [Fact]
    public async Task RecipeLocking_ShouldWork()
    {
        // Arrange
        var testRecipe = CreateTestRecipe();
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchCommandAsync(addCommand);

        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.RecipeId };
        await _dispatcher.DispatchCommandAsync(unlockCommand);

        // Verify initially unlocked
        var isUnlockedQuery = new IsRecipeUnlockedQuery { RecipeId = testRecipe.RecipeId };
        var initialUnlocked = await _dispatcher.DispatchQueryAsync<IsRecipeUnlockedQuery, bool>(isUnlockedQuery);
        initialUnlocked.Should().BeTrue();

        // Act - Lock the recipe
        var lockCommand = new LockRecipeCommand { RecipeId = testRecipe.RecipeId };
        await _dispatcher.DispatchCommandAsync(lockCommand);

        // Assert - Recipe should be locked
        var finalUnlocked = await _dispatcher.DispatchQueryAsync<IsRecipeUnlockedQuery, bool>(isUnlockedQuery);
        finalUnlocked.Should().BeFalse();
    }

    [Fact]
    public async Task SearchRecipes_ShouldReturnFilteredResults()
    {
        // Arrange - Add multiple recipes
        var recipe1 = CreateTestRecipe("Sword Recipe", "sword-1");
        var recipe2 = CreateTestRecipe("Shield Recipe", "shield-1");
        var recipe3 = CreateTestRecipe("Potion Recipe", "potion-1");

        await _dispatcher.DispatchCommandAsync(new AddRecipeCommand { Recipe = recipe1 });
        await _dispatcher.DispatchCommandAsync(new AddRecipeCommand { Recipe = recipe2 });
        await _dispatcher.DispatchCommandAsync(new AddRecipeCommand { Recipe = recipe3 });

        // Unlock all recipes
        await _dispatcher.DispatchCommandAsync(new UnlockRecipeCommand { RecipeId = recipe1.RecipeId });
        await _dispatcher.DispatchCommandAsync(new UnlockRecipeCommand { RecipeId = recipe2.RecipeId });
        await _dispatcher.DispatchCommandAsync(new UnlockRecipeCommand { RecipeId = recipe3.RecipeId });

        // Act - Search for recipes containing "sword"
        var searchQuery = new SearchRecipesQuery { SearchTerm = "Sword" };
        var searchResults = await _dispatcher.DispatchQueryAsync<SearchRecipesQuery, IReadOnlyList<Recipe>>(searchQuery);

        // Assert - Should return sword recipe
        searchResults.Should().HaveCount(1);
        searchResults.First().Name.Should().Contain("Sword");
    }

    [Fact]
    public async Task CancelAllOrders_ShouldWork()
    {
        // Arrange - Create orders
        var testRecipe = CreateTestRecipe();
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchCommandAsync(addCommand);

        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.RecipeId };
        await _dispatcher.DispatchCommandAsync(unlockCommand);

        var materials = CreateTestMaterials();
        
        // Queue multiple orders
        var queueCommand1 = new QueueCraftingOrderCommand { RecipeId = testRecipe.RecipeId, Materials = materials };
        var queueCommand2 = new QueueCraftingOrderCommand { RecipeId = testRecipe.RecipeId, Materials = materials };
        
        await _dispatcher.DispatchCommandAsync<QueueCraftingOrderCommand, string>(queueCommand1);
        await _dispatcher.DispatchCommandAsync<QueueCraftingOrderCommand, string>(queueCommand2);

        // Act - Cancel all orders
        var cancelAllCommand = new CancelAllCraftingOrdersCommand();
        await _dispatcher.DispatchCommandAsync(cancelAllCommand);

        // Assert - All orders should be cancelled
        var getAllOrdersQuery = new GetAllCraftingOrdersQuery();
        var allOrders = await _dispatcher.DispatchQueryAsync<GetAllCraftingOrdersQuery, CraftingOrdersResult>(getAllOrdersQuery);
        allOrders.TotalOrderCount.Should().Be(0);
    }

    private static Recipe CreateTestRecipe(string name = "Test Recipe", string id = "test-recipe-1")
    {
        var materialRequirements = new List<MaterialRequirement>
        {
            new(Category.Wood, QualityTier.Common, 2),
            new(Category.Metal, QualityTier.Common, 1)
        };

        var craftingResult = new CraftingResult("test-item-1", "Test Item", ItemType.Weapon, QualityTier.Common, 1, 10);

        return new Recipe(
            recipeId: id,
            name: name,
            description: "A test recipe for integration testing",
            category: RecipeCategory.Tools,
            materialRequirements: materialRequirements,
            result: craftingResult,
            craftingTime: 5.0,
            difficulty: 1,
            prerequisites: null,
            isUnlocked: false,
            experienceReward: 10
        );
    }

    private static IReadOnlyDictionary<string, Material> CreateTestMaterials()
    {
        var materials = new Dictionary<string, Material>
        {
            { "wood-1", new Material("test-wood-1", "Wood Plank", "A wooden plank", QualityTier.Common, 5, Category.Wood) },
            { "wood-2", new Material("test-wood-2", "Wood Plank", "A wooden plank", QualityTier.Common, 5, Category.Wood) },
            { "metal-1", new Material("test-metal-1", "Iron Ingot", "An iron ingot", QualityTier.Common, 10, Category.Metal) }
        };

        return materials;
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
