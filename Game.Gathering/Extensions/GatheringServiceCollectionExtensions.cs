#nullable enable

using Game.Gathering.Commands;
using Game.Gathering.Handlers;
using Game.Gathering.Systems;
using Microsoft.Extensions.DependencyInjection;
using Game.Core.Extensions;

namespace Game.Gathering.Extensions;

/// <summary>
/// Extension methods for registering gathering services with dependency injection.
/// </summary>
public static class GatheringServiceCollectionExtensions
{
    /// <summary>
    /// Registers all gathering module services with the service collection.
    /// </summary>
    public static IServiceCollection AddGatheringModule(this IServiceCollection services)
    {
        // Register core gathering system
        services.AddScoped<IGatheringSystem, GatheringSystem>();
        services.AddScoped<GatheringSystem>();
        
        // Register command handlers
        services.AddCommandHandler<GatherMaterialsCommand, GatherMaterialsResult, GatherMaterialsCommandHandler>();
        services.AddScoped<GatherMaterialsCommandHandler>(); // Also register concrete type
        
        return services;
    }
}
