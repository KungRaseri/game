#nullable enable

using Game.Core.CQS;
using Game.Core.Data;
using Game.Core.Data.Extensions;
using Game.Core.Data.Interfaces;
using Game.Core.Data.Services;
using Game.Crafting.Commands;
using Game.Crafting.Data;
using Game.Crafting.Data.Models;
using Game.Crafting.Data.Services;
using Game.Crafting.Handlers;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Crafting.Extensions;

/// <summary>
/// Extension methods for configuring crafting services with dependency injection.
/// </summary>
public static class CraftingServiceCollectionExtensions
{
    /// <summary>
    /// Adds all crafting services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddCraftingServices(this IServiceCollection services)
    {
        // Register data infrastructure
        services.AddDataServices();

        // Register crafting data services (includes data loaders and backward compatibility)
        services.AddCraftingDataServices();

        // Register core systems
        services.AddSingleton<RecipeManager>();
        services.AddSingleton<IRecipeManager>(provider => provider.GetRequiredService<RecipeManager>());
        services.AddSingleton<CraftingStation>();
        services.AddSingleton<ICraftingStation>(provider => provider.GetRequiredService<CraftingStation>());

        // Register facade service
        services.AddScoped<CraftingService>();

        // Register command handlers
        services.AddScoped<ICommandHandler<QueueCraftingOrderCommand, string>, QueueCraftingOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelCraftingOrderCommand>, CancelCraftingOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelAllCraftingOrdersCommand>, CancelAllCraftingOrdersCommandHandler>();
        services.AddScoped<ICommandHandler<AddRecipeCommand>, AddRecipeCommandHandler>();
        services.AddScoped<ICommandHandler<UnlockRecipeCommand>, UnlockRecipeCommandHandler>();
        services.AddScoped<ICommandHandler<LockRecipeCommand>, LockRecipeCommandHandler>();
        services.AddScoped<ICommandHandler<DiscoverRecipesCommand, int>, DiscoverRecipesCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetCraftingOrderQuery, CraftingOrder?>, GetCraftingOrderQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllCraftingOrdersQuery, CraftingOrdersResult>, GetAllCraftingOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetCraftingStationStatsQuery, Dictionary<string, object>>, GetCraftingStationStatsQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecipeQuery, Recipe?>, GetRecipeQueryHandler>();
        services.AddScoped<IQueryHandler<GetUnlockedRecipesQuery, IReadOnlyList<Recipe>>, GetUnlockedRecipesQueryHandler>();
        services.AddScoped<IQueryHandler<SearchRecipesQuery, IReadOnlyList<Recipe>>, SearchRecipesQueryHandler>();
        services.AddScoped<IQueryHandler<IsRecipeUnlockedQuery, bool>, IsRecipeUnlockedQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecipeManagerStatsQuery, Dictionary<string, object>>, GetRecipeManagerStatsQueryHandler>();

        return services;
    }

    /// <summary>
    /// Registers only the Crafting data loading services (without CQS handlers).
    /// Use this for scenarios where you only need data access without the full CQS infrastructure.
    /// </summary>
    public static IServiceCollection AddCraftingDataServices(this IServiceCollection services)
    {
        // Register data loaders for Crafting domain
        services.AddSingleton<IDataLoader<RecipeDataSet>, JsonDataLoader<RecipeDataSet>>();
        
        // Register Crafting domain data services
        services.AddSingleton<CraftingDataService>(provider =>
        {
            var craftingDataService = new CraftingDataService(provider.GetRequiredService<IDataLoader<RecipeDataSet>>());
            
            // Set up backward compatibility for static Phase1Recipes class
#pragma warning disable CS0618 // Type or member is obsolete
            Phase1Recipes.SetCraftingDataService(craftingDataService);
#pragma warning restore CS0618 // Type or member is obsolete
            
            return craftingDataService;
        });
        services.AddSingleton<RecipeInitializationService>(provider =>
        {
            var recipeInitializationService = new RecipeInitializationService(provider.GetRequiredService<CraftingDataService>());
            
            // Set up backward compatibility for static StarterRecipes class
#pragma warning disable CS0618 // Type or member is obsolete
            StarterRecipes.SetRecipeInitializationService(recipeInitializationService);
#pragma warning restore CS0618 // Type or member is obsolete
            
            return recipeInitializationService;
        });

        return services;
    }

    /// <summary>
    /// Adds only the core crafting systems without handlers (for scenarios where CQS is not used).
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddCraftingCoreSystems(this IServiceCollection services)
    {
        services.AddSingleton<RecipeManager>();
        services.AddSingleton<IRecipeManager>(provider => provider.GetRequiredService<RecipeManager>());
        services.AddSingleton<CraftingStation>();
        services.AddSingleton<ICraftingStation>(provider => provider.GetRequiredService<CraftingStation>());

        return services;
    }
}
