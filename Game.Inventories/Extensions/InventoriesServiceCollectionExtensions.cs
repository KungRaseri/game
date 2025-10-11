#nullable enable

using Game.Core.Extensions;
using Game.Inventories.Handlers;
using Game.Inventories.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Inventories.Extensions;

/// <summary>
/// Extension methods for registering inventory services with dependency injection.
/// </summary>
public static class InventoriesServiceCollectionExtensions
{
    /// <summary>
    /// Registers all inventory services and CQS handlers with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services with</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        // Ensure CQS is registered
        services.AddCQS();

        // Register the inventory manager as a singleton
        services.AddSingleton<InventoryManager>();

        // Register command handlers
        services.AddTransient<AddMaterialsCommandHandler>();
        services.AddTransient<RemoveMaterialsCommandHandler>();
        services.AddTransient<ConsumeMaterialsCommandHandler>();
        services.AddTransient<ExpandInventoryCommandHandler>();
        services.AddTransient<ClearInventoryCommandHandler>();
        services.AddTransient<SaveInventoryCommandHandler>();
        services.AddTransient<LoadInventoryCommandHandler>();

        // Register query handlers
        services.AddTransient<GetInventoryStatsQueryHandler>();
        services.AddTransient<SearchInventoryQueryHandler>();
        services.AddTransient<CanConsumeMaterialsQueryHandler>();
        services.AddTransient<GetInventoryContentsQueryHandler>();
        services.AddTransient<ValidateInventoryQueryHandler>();
        services.AddTransient<IsInventoryLoadedQueryHandler>();

        return services;
    }
}
