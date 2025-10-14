#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.Extensions;
using Game.Core.Data.Interfaces;
using Game.Core.Data.Services;
using Game.Adventure.Commands;
using Game.Adventure.Queries;
using Game.Adventure.Handlers;
using Game.Adventure.Systems;
using Game.Adventure.Models;
using Game.Adventure.Data;
using Game.Adventure.Data.Services;
using Game.Adventure.Data.Models;

namespace Game.Adventure.Extensions;

/// <summary>
/// Extension methods for registering Adventure module services and CQS handlers.
/// </summary>
public static class AdventureServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Adventure module services, systems, and CQS handlers.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddAdventureModule(this IServiceCollection services)
    {
        // Register data services
        services.AddAdventureDataServices();

        // Register core systems
        services.AddSingleton<CombatSystem>();
        services.AddScoped<AdventureSystem>();

        // Register command handlers
        services.AddCommandHandler<SendAdventurerToGoblinCaveCommand, SendAdventurerToGoblinCaveCommandHandler>();
        services.AddCommandHandler<ForceAdventurerRetreatCommand, ForceAdventurerRetreatCommandHandler>();
        services.AddCommandHandler<UpdateAdventurerStateCommand, UpdateAdventurerStateCommandHandler>();
        services.AddCommandHandler<ResetCombatSystemCommand, ResetCombatSystemCommandHandler>();

        // Register query handlers
        services.AddQueryHandler<GetAdventurerStatusQuery, string, GetAdventurerStatusQueryHandler>();
        services.AddQueryHandler<GetAdventurerStateQuery, AdventurerState, GetAdventurerStateQueryHandler>();
        services.AddQueryHandler<IsAdventurerAvailableQuery, bool, IsAdventurerAvailableQueryHandler>();
        services.AddQueryHandler<GetCurrentAdventurerQuery, CombatEntityStats?, GetCurrentAdventurerQueryHandler>();
        services.AddQueryHandler<GetCurrentMonsterQuery, CombatEntityStats?, GetCurrentMonsterQueryHandler>();
        services.AddQueryHandler<IsAdventurerInCombatQuery, bool, IsAdventurerInCombatQueryHandler>();
        services.AddQueryHandler<HasMonstersRemainingQuery, bool, HasMonstersRemainingQueryHandler>();
        services.AddQueryHandler<GetAdventurerInfoQuery, AdventurerInfo, GetAdventurerInfoQueryHandler>();
        services.AddQueryHandler<GetEntitiesByTypeQuery, IReadOnlyList<EntityTypeConfig>, GetEntitiesByTypeQueryHandler>();

        return services;
    }

    /// <summary>
    /// Registers Adventure data loading services
    /// </summary>
    public static IServiceCollection AddAdventureDataServices(this IServiceCollection services)
    {
        // Register core data services
        services.AddSingleton<HotReloadService>();
        
        // Register data loaders for Adventure domain
        services.AddSingleton<IDataLoader<EntityDataSet>, JsonDataLoader<EntityDataSet>>();
        
        // Register Adventure domain data services
        services.AddSingleton<IAdventureDataService, AdventureDataService>();
        services.AddSingleton<AdventureDataService>(provider => (AdventureDataService)provider.GetRequiredService<IAdventureDataService>());
        services.AddSingleton<EntityCreationService>(provider =>
        {
            var entityCreationService = new EntityCreationService(provider.GetRequiredService<AdventureDataService>());
            
            // Initialize EntityFactory with the service for backward compatibility
            EntityFactory.SetEntityCreationService(entityCreationService);
            
            return entityCreationService;
        });

        return services;
    }
}
