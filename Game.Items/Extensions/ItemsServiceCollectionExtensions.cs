using Game.Core.CQS;
using Game.Items.Commands;
using Game.Items.Handlers;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Queries;
using Game.Items.Systems;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Items.Extensions;

/// <summary>
/// Extension methods for registering Item system services with dependency injection.
/// </summary>
public static class ItemsServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Item system services including CQS handlers and dependencies.
    /// </summary>
    /// <param name="services">The service collection to register services with</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddItemsServices(this IServiceCollection services)
    {
        // Register core systems
        services.AddSingleton<LootGenerator>(_ =>
        {
            // Create a simple loot generator with basic configurations
            // In a real implementation, this would come from configuration or database
            var lootTables = CreateDefaultLootTables();
            return new LootGenerator(lootTables);
        });

        // Register command handlers
        services.AddScoped<ICommandHandler<CreateWeaponCommand, Weapon>, CreateWeaponCommandHandler>();
        services.AddScoped<ICommandHandler<CreateArmorCommand, Armor>, CreateArmorCommandHandler>();
        services.AddScoped<ICommandHandler<CreateMaterialCommand, Material>, CreateMaterialCommandHandler>();
        services.AddScoped<ICommandHandler<GenerateLootCommand, List<Drop>>, GenerateLootCommandHandler>();
        services.AddScoped<ICommandHandler<CalculateItemValueCommand, int>, CalculateItemValueCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetWeaponConfigsQuery, IEnumerable<Data.WeaponConfig>>, GetWeaponConfigsQueryHandler>();
        services.AddScoped<IQueryHandler<GetArmorConfigsQuery, IEnumerable<Data.ArmorConfig>>, GetArmorConfigsQueryHandler>();
        services.AddScoped<IQueryHandler<GetMaterialConfigsQuery, IEnumerable<Data.MaterialConfig>>, GetMaterialConfigsQueryHandler>();
        services.AddScoped<IQueryHandler<GetLootStatisticsQuery, Dictionary<string, float>>, GetLootStatisticsQueryHandler>();
        services.AddScoped<IQueryHandler<GetQualityTierModifiersQuery, Models.QualityTierModifierResult>, GetQualityTierModifiersQueryHandler>();
        services.AddScoped<IQueryHandler<CheckLootTableExistsQuery, bool>, CheckLootTableExistsQueryHandler>();

        return services;
    }

    private static Dictionary<string, Models.LootTable> CreateDefaultLootTables()
    {
        // Create basic loot tables for testing
        // In a real implementation, this would come from configuration files or database
        var lootTables = new Dictionary<string, Models.LootTable>();

        // Add some basic loot tables for common monsters
        // This is a placeholder - actual loot tables would be more complex
        return lootTables;
    }
}
