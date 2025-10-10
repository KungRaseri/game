#nullable enable

using Game.Scripts.Commands;
using Game.Scripts.Queries;
using Game.Scripts.Handlers;
using Game.Scripts.Systems;
using Game.Core.CQS;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Scripts.Extensions;

/// <summary>
/// Extension methods for registering ShopKeeper CQS handlers and systems.
/// </summary>
public static class ShopKeeperServiceExtensions
{
    /// <summary>
    /// Registers all ShopKeeper state management services including CQS handlers.
    /// </summary>
    public static IServiceCollection AddShopKeeperStateServices(this IServiceCollection services)
    {
        // Register the core state system
        services.AddSingleton<ShopKeeperStateSystem>();

        // Register command handlers
        services.AddScoped<ICommandHandler<StartGatheringHerbsCommand>, StartGatheringHerbsCommandHandler>();
        services.AddScoped<ICommandHandler<StartCraftingPotionsCommand>, StartCraftingPotionsCommandHandler>();
        services.AddScoped<ICommandHandler<StartRunningShopCommand>, StartRunningShopCommandHandler>();
        services.AddScoped<ICommandHandler<StopCurrentActivityCommand>, StopCurrentActivityCommandHandler>();
        services.AddScoped<ICommandHandler<ForceStateTransitionCommand>, ForceStateTransitionCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetShopKeeperStateQuery, Models.ShopKeeperStateInfo>, GetShopKeeperStateQueryHandler>();
        services.AddScoped<IQueryHandler<CanTransitionToStateQuery, bool>, CanTransitionToStateQueryHandler>();
        services.AddScoped<IQueryHandler<GetAvailableActivitiesQuery, AvailableActivitiesResult>, GetAvailableActivitiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetActivityStatisticsQuery, ActivityStatisticsResult>, GetActivityStatisticsQueryHandler>();

        return services;
    }
}
