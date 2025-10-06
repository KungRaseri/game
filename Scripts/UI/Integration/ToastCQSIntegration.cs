#nullable enable

using Game.Core.CQS;
using Game.UI.Commands;
using Game.UI.Models;
using Game.UI.Queries;

namespace Game.Scripts.UI.Integration;

/// <summary>
/// Example usage of the CQS toast system.
/// Shows how to use individual command and query handlers like Game.Adventure.
/// </summary>
public static class ToastCQSUsageExample
{
    /// <summary>
    /// Example: Show different types of toasts using CQS commands.
    /// 
    /// Note: In a real application, you would register these handlers with your DI container
    /// and the dispatcher would resolve them automatically. This example shows direct usage.
    /// </summary>
    public static async Task ShowExampleToasts(IDispatcher dispatcher)
    {
        // Simple toast
        await dispatcher.DispatchCommandAsync(new ShowSimpleToastCommand("Hello World!"));

        // Success toast
        await dispatcher.DispatchCommandAsync(new ShowSuccessToastCommand("Achievement unlocked!"));

        // Warning toast
        await dispatcher.DispatchCommandAsync(new ShowWarningToastCommand("Low health warning"));

        // Error toast
        await dispatcher.DispatchCommandAsync(new ShowErrorToastCommand("Connection failed"));

        // Material collection toast
        var materials = new List<string> { "Iron Ore", "Coal", "Copper Ore" };
        await dispatcher.DispatchCommandAsync(new ShowMaterialToastCommand(materials));

        // Custom configured toast
        var customConfig = new ToastConfig
        {
            Title = "Custom Toast",
            Message = "This is a custom configured toast",
            Style = ToastStyle.Material,
            Anchor = ToastAnchor.Center,
            Animation = ToastAnimation.Bounce,
            DisplayDuration = 5.0f
        };
        await dispatcher.DispatchCommandAsync(new ShowToastCommand(customConfig));
    }

    /// <summary>
    /// Example: Query toast information using CQS queries.
    /// 
    /// Note: In a real application, you would register these handlers with your DI container
    /// and the dispatcher would resolve them automatically. This example shows direct usage.
    /// </summary>
    public static async Task QueryToastInformation(IDispatcher dispatcher)
    {
        // Get count of active toasts
        var count = await dispatcher.DispatchQueryAsync<GetActiveToastCountQuery, int>(new GetActiveToastCountQuery());
        
        // Get all active toasts
        var activeToasts = await dispatcher.DispatchQueryAsync<GetActiveToastsQuery, List<ToastInfo>>(new GetActiveToastsQuery());
        
        // Get toasts at specific anchor
        var topRightToasts = await dispatcher.DispatchQueryAsync<GetToastsByAnchorQuery, List<ToastInfo>>(new GetToastsByAnchorQuery(ToastAnchor.TopRight));
        
        // Check if toast limit is reached
        var limitReached = await dispatcher.DispatchQueryAsync<IsToastLimitReachedQuery, bool>(new IsToastLimitReachedQuery());
        
        // Get specific toast by ID (if you have the ID)
        if (activeToasts.Count > 0)
        {
            var firstToastId = activeToasts[0].Id;
            var specificToast = await dispatcher.DispatchQueryAsync<GetToastByIdQuery, ToastInfo?>(new GetToastByIdQuery(firstToastId));
        }
    }

    /// <summary>
    /// Example: Clear toasts using CQS commands.
    /// </summary>
    public static async Task ClearToasts(IDispatcher dispatcher)
    {
        // Clear all toasts
        await dispatcher.DispatchCommandAsync(new ClearAllToastsCommand());

        // Or dismiss a specific toast by ID
        // await dispatcher.DispatchCommandAsync(new DismissToastCommand("specific-toast-id"));
    }
}
