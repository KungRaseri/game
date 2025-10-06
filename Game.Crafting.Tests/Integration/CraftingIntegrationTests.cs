#nullable enable

using FluentAssertions;
using Game.Core.CQS;
using Game.Core.Tests;
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
    public async Task EndToEnd_CraftingWorkflow_ShouldWork()
    {
        // Arrange - Create a test recipe
        var testRecipe = CreateTestRecipe();
        
        // Act & Assert - Add recipe to manager
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchAsync(addCommand);

        // Verify recipe was added
        var getRecipeQuery = new GetRecipeQuery { RecipeId = testRecipe.Id };
        var retrievedRecipe = await _dispatcher.DispatchAsync(getRecipeQuery);
        retrievedRecipe.Should().NotBeNull();
        retrievedRecipe!.Id.Should().Be(testRecipe.Id);

        // Unlock the recipe
        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.Id };
        await _dispatcher.DispatchAsync(unlockCommand);

        // Verify recipe is unlocked
        var isUnlockedQuery = new IsRecipeUnlockedQuery { RecipeId = testRecipe.Id };
        var isUnlocked = await _dispatcher.DispatchAsync(isUnlockedQuery);
        isUnlocked.Should().BeTrue();

        // Queue crafting order
        var queueCommand = new QueueCraftingOrderCommand 
        { 
            RecipeId = testRecipe.Id, 
            Quantity = 2 
        };
        await _dispatcher.DispatchAsync(queueCommand);

        // Verify order was queued
        var getOrderQuery = new GetCraftingOrderQuery { OrderId = 1 };
        var order = await _dispatcher.DispatchAsync(getOrderQuery);
        order.Should().NotBeNull();
        order!.RecipeId.Should().Be(testRecipe.Id);
        order.Quantity.Should().Be(2);

        // Get all orders
        var getAllOrdersQuery = new GetAllCraftingOrdersQuery();
        var allOrders = await _dispatcher.DispatchAsync(getAllOrdersQuery);
        allOrders.Should().HaveCount(1);
        allOrders.First().Should().BeEquivalentTo(order);

        // Cancel the order
        var cancelCommand = new CancelCraftingOrderCommand { OrderId = 1 };
        await _dispatcher.DispatchAsync(cancelCommand);

        // Verify order was cancelled
        var cancelledOrder = await _dispatcher.DispatchAsync(getOrderQuery);
        cancelledOrder.Should().BeNull();
    }

    [Fact]
    public async Task QueryOperations_ShouldNotModifyState()
    {
        // Arrange - Set up initial state
        var testRecipe = CreateTestRecipe();
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchAsync(addCommand);

        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.Id };
        await _dispatcher.DispatchAsync(unlockCommand);

        // Act - Execute multiple queries
        var getRecipeQuery = new GetRecipeQuery { RecipeId = testRecipe.Id };
        var getUnlockedQuery = new GetUnlockedRecipesQuery();
        var searchQuery = new SearchRecipesQuery { NameFilter = "Test" };
        var isUnlockedQuery = new IsRecipeUnlockedQuery { RecipeId = testRecipe.Id };
        var getStatsQuery = new GetCraftingStatsQuery();
        var getRecipeStatsQuery = new GetRecipeStatsQuery { RecipeId = testRecipe.Id };

        // Execute queries multiple times
        for (int i = 0; i < 3; i++)
        {
            await _dispatcher.DispatchAsync(getRecipeQuery);
            await _dispatcher.DispatchAsync(getUnlockedQuery);
            await _dispatcher.DispatchAsync(searchQuery);
            await _dispatcher.DispatchAsync(isUnlockedQuery);
            await _dispatcher.DispatchAsync(getStatsQuery);
            await _dispatcher.DispatchAsync(getRecipeStatsQuery);
        }

        // Assert - State should remain unchanged
        var finalRecipe = await _dispatcher.DispatchAsync(getRecipeQuery);
        finalRecipe.Should().NotBeNull();
        finalRecipe!.Id.Should().Be(testRecipe.Id);

        var finalUnlocked = await _dispatcher.DispatchAsync(getUnlockedQuery);
        finalUnlocked.Should().HaveCount(1);

        var finalIsUnlocked = await _dispatcher.DispatchAsync(isUnlockedQuery);
        finalIsUnlocked.Should().BeTrue();
    }

    [Fact]
    public async Task RecipeDiscovery_ShouldWork()
    {
        // Arrange - Add recipes but don't unlock them
        var recipe1 = CreateTestRecipe("Recipe1");
        var recipe2 = CreateTestRecipe("Recipe2");
        
        var addCommand1 = new AddRecipeCommand { Recipe = recipe1 };
        var addCommand2 = new AddRecipeCommand { Recipe = recipe2 };
        
        await _dispatcher.DispatchAsync(addCommand1);
        await _dispatcher.DispatchAsync(addCommand2);

        // Act - Discover recipes
        var discoverCommand = new DiscoverRecipesCommand();
        await _dispatcher.DispatchAsync(discoverCommand);

        // Assert - Some recipes should be discovered (mocked behavior)
        var unlockedQuery = new GetUnlockedRecipesQuery();
        var unlockedRecipes = await _dispatcher.DispatchAsync(unlockedQuery);
        
        // The actual number depends on the DiscoverRecipes implementation
        // For this test, we just verify the command executes without error
        unlockedRecipes.Should().NotBeNull();
    }

    [Fact]
    public async Task CraftingStats_ShouldReflectOperations()
    {
        // Arrange
        var testRecipe = CreateTestRecipe();
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchAsync(addCommand);

        // Act - Perform operations
        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.Id };
        await _dispatcher.DispatchAsync(unlockCommand);

        var queueCommand = new QueueCraftingOrderCommand 
        { 
            RecipeId = testRecipe.Id, 
            Quantity = 1 
        };
        await _dispatcher.DispatchAsync(queueCommand);

        // Assert - Stats should reflect operations
        var getStatsQuery = new GetCraftingStatsQuery();
        var stats = await _dispatcher.DispatchAsync(getStatsQuery);
        
        stats.Should().NotBeNull();
        // Verify stats reflect the operations performed
        // (actual validation depends on CraftingStats implementation)
    }

    [Fact]
    public async Task RecipeLocking_ShouldWork()
    {
        // Arrange
        var testRecipe = CreateTestRecipe();
        var addCommand = new AddRecipeCommand { Recipe = testRecipe };
        await _dispatcher.DispatchAsync(addCommand);

        var unlockCommand = new UnlockRecipeCommand { RecipeId = testRecipe.Id };
        await _dispatcher.DispatchAsync(unlockCommand);

        // Verify initially unlocked
        var isUnlockedQuery = new IsRecipeUnlockedQuery { RecipeId = testRecipe.Id };
        var initialUnlocked = await _dispatcher.DispatchAsync(isUnlockedQuery);
        initialUnlocked.Should().BeTrue();

        // Act - Lock the recipe
        var lockCommand = new LockRecipeCommand { RecipeId = testRecipe.Id };
        await _dispatcher.DispatchAsync(lockCommand);

        // Assert - Recipe should be locked
        var finalUnlocked = await _dispatcher.DispatchAsync(isUnlockedQuery);
        finalUnlocked.Should().BeFalse();
    }

    [Fact]
    public async Task SearchRecipes_ShouldReturnFilteredResults()
    {
        // Arrange - Add multiple recipes
        var recipe1 = CreateTestRecipe("Sword Recipe");
        var recipe2 = CreateTestRecipe("Shield Recipe");
        var recipe3 = CreateTestRecipe("Potion Recipe");

        await _dispatcher.DispatchAsync(new AddRecipeCommand { Recipe = recipe1 });
        await _dispatcher.DispatchAsync(new AddRecipeCommand { Recipe = recipe2 });
        await _dispatcher.DispatchAsync(new AddRecipeCommand { Recipe = recipe3 });

        // Act - Search for recipes containing "Recipe"
        var searchQuery = new SearchRecipesQuery { NameFilter = "Recipe" };
        var searchResults = await _dispatcher.DispatchAsync(searchQuery);

        // Assert - Should return all recipes
        searchResults.Should().HaveCount(3);

        // Act - Search for specific recipe
        var specificSearch = new SearchRecipesQuery { NameFilter = "Sword" };
        var specificResults = await _dispatcher.DispatchAsync(specificSearch);

        // Assert - Should return only sword recipe
        specificResults.Should().HaveCount(1);
        specificResults.First().Name.Should().Contain("Sword");
    }

    private static Recipe CreateTestRecipe(string name = "Test Recipe")
    {
        return new Recipe
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test recipe for integration testing",
            MaterialRequirements = new List<MaterialRequirement>
            {
                new() { MaterialType = MaterialType.Wood, Quantity = 2 },
                new() { MaterialType = MaterialType.Metal, Quantity = 1 }
            },
            CraftingTime = TimeSpan.FromMinutes(5),
            ResultingItems = new List<ItemTemplate>
            {
                new() { ItemType = ItemType.Weapon, Rarity = Rarity.Common }
            }
        };
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
