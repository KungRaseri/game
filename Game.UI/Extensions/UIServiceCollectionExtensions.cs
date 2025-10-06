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
/// Extension methods for registering Adventure module services and CQS handlers.
/// </summary>
public static class UIServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Adventure module services, systems, and CQS handlers.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddUIModule(this IServiceCollection services)
    {
        // Register core systems
        services.AddSingleton<ToastSystem>();
        services.AddScoped<UISystem>();

        // Register command handlers
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

        // Register query handlers
        services.AddQueryHandler<GetActiveToastsQuery, List<ToastInfo>, GetActiveToastsQueryHandler>();
        services.AddQueryHandler<GetToastByIdQuery, ToastInfo?, GetToastByIdQueryHandler>();
        services.AddQueryHandler<GetToastsByAnchorQuery, List<ToastInfo>, GetToastsByAnchorQueryHandler>();
        services.AddQueryHandler<GetActiveToastCountQuery, int, GetActiveToastCountQueryHandler>();
        services.AddQueryHandler<IsToastLimitReachedQuery, bool, IsToastLimitReachedQueryHandler>();

        return services;
    }
}
