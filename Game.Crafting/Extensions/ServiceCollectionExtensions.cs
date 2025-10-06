#nullable enable

using Game.Core.CQS;
using Game.Crafting.Commands;
using Game.Crafting.Handlers;
using Game.Crafting.Models;
using Game.Crafting.Queries;
using Game.Crafting.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Crafting.Extensions;

/// <summary>
/// Extension methods for configuring crafting services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all crafting services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddCraftingServices(this IServiceCollection services)
    {
        // Register core systems
        services.AddSingleton<RecipeManager>();
        services.AddSingleton<CraftingStation>();

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
    /// Adds only the core crafting systems without handlers (for scenarios where CQS is not used).
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddCraftingCoreSystems(this IServiceCollection services)
    {
        services.AddSingleton<RecipeManager>();
        services.AddSingleton<CraftingStation>();

        return services;
    }
}
