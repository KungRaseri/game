#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.Extensions;
using Game.UI.Systems;
using Game.UI.Commands;
using Game.UI.Queries;
using Game.UI.Handlers;
using Game.UI.Models;

namespace Game.UI.Extensions;

/// <summary>
/// Extension methods for registering UI module services and CQS handlers.
/// </summary>
public static class UIServiceCollectionExtensions
{
    /// <summary>
    /// Registers all UI module services, systems, and CQS handlers.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddUIModule(this IServiceCollection services)
    {
        // Register core systems
        services.AddSingleton<ToastSystem>();
        services.AddSingleton<IToastOperations>(provider => provider.GetRequiredService<ToastSystem>());
        services.AddScoped<UISystem>();
        
        // Register scene management systems
        services.AddSingleton<SceneManagerSystem>();
        services.AddSingleton<LoadingSystem>();
        
        // Register fade transition service interface (implementation will be provided by Godot scripts)
        services.AddSingleton<IFadeTransitionService>(provider => 
        {
            // This will be overridden by the Godot scene when it's available
            // For now, provide a no-op implementation for dependency resolution
            return new NoOpFadeTransitionService();
        });

        // Register toast command handlers
        services.AddCommandHandler<ShowToastCommand, ShowToastCommandHandler>();
        services.AddCommandHandler<ShowSimpleToastCommand, ShowSimpleToastCommandHandler>();
        services.AddCommandHandler<ShowTitledToastCommand, ShowTitledToastCommandHandler>();
        services.AddCommandHandler<ShowMaterialToastCommand, ShowMaterialToastCommandHandler>();
        services.AddCommandHandler<ShowSuccessToastCommand, ShowSuccessToastCommandHandler>();
        services.AddCommandHandler<ShowWarningToastCommand, ShowWarningToastCommandHandler>();
        services.AddCommandHandler<ShowErrorToastCommand, ShowErrorToastCommandHandler>();
        services.AddCommandHandler<ShowInfoToastCommand, ShowInfoToastCommandHandler>();
        services.AddCommandHandler<ClearAllToastsCommand, ClearAllToastsCommandHandler>();
        services.AddCommandHandler<DismissToastCommand, DismissToastCommandHandler>();
        
        // Register scene management command handlers
        services.AddCommandHandler<TransitionToSceneCommand, TransitionToSceneCommandHandler>();
        services.AddCommandHandler<StartLoadingCommand, StartLoadingCommandHandler>();
        services.AddCommandHandler<QueueResourceCommand, QueueResourceCommandHandler>();
        services.AddCommandHandler<UpdateLoadingProgressCommand, UpdateLoadingProgressCommandHandler>();
        services.AddCommandHandler<CompleteLoadingCommand, CompleteLoadingCommandHandler>();
        services.AddCommandHandler<FadeTransitionCommand, FadeTransitionCommandHandler>();

        // Register toast query handlers
        services.AddQueryHandler<GetActiveToastsQuery, List<ToastInfo>, GetActiveToastsQueryHandler>();
        services.AddQueryHandler<GetToastByIdQuery, ToastInfo?, GetToastByIdQueryHandler>();
        services.AddQueryHandler<GetToastsByAnchorQuery, List<ToastInfo>, GetToastsByAnchorQueryHandler>();
        services.AddQueryHandler<GetActiveToastCountQuery, int, GetActiveToastCountQueryHandler>();
        services.AddQueryHandler<IsToastLimitReachedQuery, bool, IsToastLimitReachedQueryHandler>();
        
        // Register scene management query handlers
        services.AddQueryHandler<GetLoadingProgressQuery, LoadingProgress, GetLoadingProgressQueryHandler>();
        services.AddQueryHandler<GetSceneStateQuery, SceneState, GetSceneStateQueryHandler>();
        services.AddQueryHandler<GetQueuedResourcesQuery, List<ResourceLoadInfo>, GetQueuedResourcesQueryHandler>();
        services.AddQueryHandler<GetLoadingPhaseQuery, LoadingPhaseInfo, GetLoadingPhaseQueryHandler>();
        services.AddQueryHandler<GetLoadingConfigurationQuery, LoadingConfiguration, GetLoadingConfigurationQueryHandler>();

        return services;
    }
}
