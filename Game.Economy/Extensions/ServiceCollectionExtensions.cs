#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.CQS;
using Game.Economy.Commands;
using Game.Economy.Queries;
using Game.Economy.Handlers;
using Game.Economy.Models;
using Game.Economy.Systems;

namespace Game.Economy.Extensions;

/// <summary>
/// Extension methods for registering economy-related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all economy services including core systems, command handlers, and query handlers.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEconomyServices(this IServiceCollection services)
    {
        // Register core systems
        services.AddEconomyCoreSystems();
        
        // Register command handlers
        services.AddScoped<ICommandHandler<AddRevenueCommand>, AddRevenueCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessExpenseCommand, bool>, ProcessExpenseCommandHandler>();
        services.AddScoped<ICommandHandler<MakeInvestmentCommand, bool>, MakeInvestmentCommandHandler>();
        services.AddScoped<ICommandHandler<SetMonthlyBudgetCommand>, SetMonthlyBudgetCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessRecurringExpensesCommand>, ProcessRecurringExpensesCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetFinancialSummaryQuery, FinancialSummary>, GetFinancialSummaryQueryHandler>();
        services.AddScoped<IQueryHandler<GetCurrentGoldQuery, decimal>, GetCurrentGoldQueryHandler>();
        services.AddScoped<IQueryHandler<GetExpenseHistoryQuery, IReadOnlyList<ShopExpense>>, GetExpenseHistoryQueryHandler>();
        services.AddScoped<IQueryHandler<GetAvailableInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>, GetAvailableInvestmentsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCompletedInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>, GetCompletedInvestmentsQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecommendedInvestmentsQuery, IReadOnlyList<InvestmentOpportunity>>, GetRecommendedInvestmentsQueryHandler>();
        services.AddScoped<IQueryHandler<GetMonthlyBudgetQuery, decimal?>, GetMonthlyBudgetQueryHandler>();

        return services;
    }

    /// <summary>
    /// Registers only the core economy systems without CQS handlers.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEconomyCoreSystems(this IServiceCollection services)
    {
        // Register treasury manager as singleton to maintain state
        services.AddSingleton<ITreasuryManager>(_ => new TreasuryManager());
        
        // Register the high-level economy service
        services.AddScoped<EconomyService>();
        
        return services;
    }
}
