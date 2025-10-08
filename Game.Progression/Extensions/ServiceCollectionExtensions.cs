#nullable enable

using Game.Core.CQS;
using Game.Progression.Commands;
using Game.Progression.Handlers;
using Game.Progression.Models;
using Game.Progression.Queries;
using Game.Progression.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Progression.Extensions;

/// <summary>
/// Extension methods for registering progression services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all progression services and CQS handlers with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services with</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddProgressionServices(this IServiceCollection services)
    {
        // Register the progression manager as a singleton to maintain state
        services.AddSingleton<ProgressionManager>();

        // Register command handlers
        services.AddScoped<ICommandHandler<RecordMaterialsGatheredCommand>, RecordMaterialsGatheredCommandHandler>();
        services.AddScoped<ICommandHandler<RecordItemCraftedCommand>, RecordItemCraftedCommandHandler>();
        services.AddScoped<ICommandHandler<RecordSaleCommand>, RecordSaleCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetPlayerProgressQuery, PlayerProgress>, GetPlayerProgressQueryHandler>();
        services.AddScoped<IQueryHandler<GetCurrentGamePhaseQuery, GamePhase>, GetCurrentGamePhaseQueryHandler>();
        services.AddScoped<IQueryHandler<GetNextMilestoneQuery, ProgressionMilestone?>, GetNextMilestoneQueryHandler>();

        return services;
    }
}
