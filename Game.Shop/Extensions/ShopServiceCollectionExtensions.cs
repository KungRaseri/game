using Game.Core.CQS;
using Game.Economy.Commands;
using Game.Economy.Models;
using Game.Economy.Queries;
using Game.Shop.Commands;
using Game.Shop.Handlers;
using Game.Shop.Models;
using Game.Shop.Queries;
using Game.Shop.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Shop.Extensions;

/// <summary>
/// Extension methods for configuring shop services in the DI container.
/// </summary>
public static class ShopServiceCollectionExtensions
{
    /// <summary>
    /// Adds shop services including CQS handlers and shop systems to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddShopServices(this IServiceCollection services)
    {
        // Register shop systems
        services.AddSingleton<ShopManager>();
        services.AddSingleton<PricingEngine>();
        services.AddSingleton<CompetitionSimulator>();
        services.AddScoped<ShopInventoryManager>();
        services.AddScoped<EnhancedCustomerAI>();
        services.AddScoped<ShopTrafficManager>();

        // Register ShopKeeper state management system
        services.AddSingleton<ShopKeeperStateManager>();

        // Register command handlers
        services.AddScoped<ICommandHandler<StockItemCommand, bool>, StockItemCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveItemCommand, string?>, RemoveItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateItemPriceCommand, bool>, UpdateItemPriceCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessSaleCommand, SaleTransaction?>, ProcessSaleCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateShopLayoutCommand>, UpdateShopLayoutCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessExpenseCommand, bool>, ProcessExpenseCommandHandler>();
        services.AddScoped<ICommandHandler<MakeInvestmentCommand, bool>, MakeInvestmentCommandHandler>();
        services.AddScoped<ICommandHandler<SetPricingStrategyCommand>, SetPricingStrategyCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessDailyOperationsCommand>, ProcessDailyOperationsCommandHandler>();

        // Register ShopKeeper command handlers
        services.AddScoped<ICommandHandler<StartGatheringHerbsCommand>, StartGatheringHerbsCommandHandler>();
        services.AddScoped<ICommandHandler<StartCraftingPotionsCommand>, StartCraftingPotionsCommandHandler>();
        services.AddScoped<ICommandHandler<StartRunningShopCommand>, StartRunningShopCommandHandler>();
        services.AddScoped<ICommandHandler<StopCurrentActivityCommand>, StopCurrentActivityCommandHandler>();
        services.AddScoped<ICommandHandler<ForceStateTransitionCommand>, ForceStateTransitionCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetDisplaySlotQuery, ShopDisplaySlot?>, GetDisplaySlotQueryHandler>();
        services.AddScoped<IQueryHandler<GetDisplaySlotsQuery, IEnumerable<ShopDisplaySlot>>, GetDisplaySlotsQueryHandler>();
        services.AddScoped<IQueryHandler<CalculateSuggestedPriceQuery, decimal>, CalculateSuggestedPriceQueryHandler>();
        services.AddScoped<IQueryHandler<GetMarketAnalysisQuery, MarketAnalysis>, GetMarketAnalysisQueryHandler>();
        services.AddScoped<IQueryHandler<GetShopPerformanceQuery, ShopPerformanceMetrics>, GetShopPerformanceQueryHandler>();
        services.AddScoped<IQueryHandler<GetFinancialSummaryQuery, FinancialSummary>, GetFinancialSummaryQueryHandler>();
        services.AddScoped<IQueryHandler<GetInvestmentOpportunitiesQuery, List<InvestmentOpportunity>>, GetInvestmentOpportunitiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetTransactionHistoryQuery, IReadOnlyList<SaleTransaction>>, GetTransactionHistoryQueryHandler>();
        services.AddScoped<IQueryHandler<GetCompetitiveAnalysisQuery, CompetitionAnalysis>, GetCompetitiveAnalysisQueryHandler>();
        services.AddScoped<IQueryHandler<GetShopLayoutQuery, ShopLayout>, GetShopLayoutQueryHandler>();

        // Register ShopKeeper query handlers
        services.AddScoped<IQueryHandler<GetShopKeeperStateQuery, ShopKeeperStateInfo>, GetShopKeeperStateQueryHandler>();
        services.AddScoped<IQueryHandler<CanTransitionToStateQuery, bool>, CanTransitionToStateQueryHandler>();
        services.AddScoped<IQueryHandler<GetAvailableActivitiesQuery, AvailableActivitiesResult>, GetAvailableActivitiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetActivityStatisticsQuery, ActivityStatisticsResult>, GetActivityStatisticsQueryHandler>();

        return services;
    }
}
