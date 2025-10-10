#nullable enable

using FluentAssertions;
using Game.Crafting.Data.Services;
using Game.Crafting.Models;
using Game.Crafting.Systems;
using Game.Core.Data.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Crafting.Tests.Data.Services;

public class CraftingDataServiceIntegrationTests
{
    [Fact]
    public async Task CraftingDataService_Should_LoadRecipesFromJson()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDataServices();
        services.AddSingleton<CraftingDataService>();
        services.AddSingleton<RecipeInitializationService>();

        var serviceProvider = services.BuildServiceProvider();
        var craftingDataService = serviceProvider.GetRequiredService<CraftingDataService>();

        // Act
        var allRecipes = await craftingDataService.GetAllRecipesAsync();

        // Assert
        allRecipes.Should().NotBeNull();
        allRecipes.Should().NotBeEmpty();
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_iron_sword");
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_steel_sword");
    }

    [Fact]
    public async Task RecipeInitializationService_Should_InitializeRecipeManagerFromJson()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDataServices();
        services.AddSingleton<CraftingDataService>();
        services.AddSingleton<RecipeInitializationService>();

        var serviceProvider = services.BuildServiceProvider();
        var recipeManager = new RecipeManager();
        var initializationService = serviceProvider.GetRequiredService<RecipeInitializationService>();

        // Act
        await initializationService.InitializeRecipeManagerAsync(recipeManager);

        // Assert
        recipeManager.AllRecipes.Should().NotBeEmpty();
        recipeManager.GetRecipe("recipe_iron_sword").Should().NotBeNull();
        recipeManager.GetRecipe("recipe_steel_sword").Should().NotBeNull();
    }

    [Fact]
    public async Task JsonLoadedRecipes_Should_ContainExpectedRecipes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDataServices();
        services.AddSingleton<CraftingDataService>();
        services.AddSingleton<RecipeInitializationService>();

        var serviceProvider = services.BuildServiceProvider();
        var recipeManager = new RecipeManager();
        var initializationService = serviceProvider.GetRequiredService<RecipeInitializationService>();

        // Initialize from JSON
        await initializationService.InitializeRecipeManagerAsync(recipeManager);

        // Assert - Check that all expected recipe categories are loaded
        var allRecipes = recipeManager.AllRecipes.ToList();
        allRecipes.Should().NotBeEmpty();
        
        // Verify we have starter recipes
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_iron_sword");
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_wooden_shield");
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_health_potion");
        
        // Verify we have advanced recipes
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_steel_sword");
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_iron_shield");
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_greater_health_potion");
        
        // Verify we have phase1 recipes
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_leather_armor");
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_simple_health_potion");
        allRecipes.Should().Contain(r => r.RecipeId == "recipe_basic_sword");
        
        // Verify recipe properties are properly loaded
        var ironSword = recipeManager.GetRecipe("recipe_iron_sword");
        ironSword.Should().NotBeNull();
        ironSword!.Name.Should().Be("Iron Sword");
        ironSword.Category.Should().Be(RecipeCategory.Weapons);
        ironSword.MaterialRequirements.Should().NotBeEmpty();
        ironSword.Result.Should().NotBeNull();
    }
}
