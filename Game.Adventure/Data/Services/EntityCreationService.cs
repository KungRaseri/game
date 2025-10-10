#nullable enable

using Game.Adventure.Data;
using Game.Adventure.Data.Services;
using Game.Adventure.Models;
using Game.Core.Utils;

namespace Game.Adventure.Data.Services;

/// <summary>
/// Service for creating adventure entities from JSON configurations
/// </summary>
public class EntityCreationService
{
    private readonly AdventureDataService _adventureDataService;

    public EntityCreationService(AdventureDataService adventureDataService)
    {
        _adventureDataService = adventureDataService ?? throw new ArgumentNullException(nameof(adventureDataService));
    }

    /// <summary>
    /// Creates an adventurer entity from JSON configuration
    /// </summary>
    public async Task<CombatEntityStats> CreateAdventurerAsync(string adventurerConfigId, string? customName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _adventureDataService.GetEntityConfigAsync(adventurerConfigId, cancellationToken);
            
            if (config == null)
            {
                throw new ArgumentException($"Adventurer configuration '{adventurerConfigId}' not found.");
            }

            var name = customName ?? config.Name;
            
            return new CombatEntityStats(
                name: name,
                maxHealth: config.BaseHealth,
                damagePerSecond: config.BaseDamage,
                retreatThreshold: config.RetreatThreshold
            );
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            GameLogger.Error(ex, $"Failed to create adventurer '{adventurerConfigId}'");
            throw new InvalidOperationException($"Failed to create adventurer '{adventurerConfigId}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a monster entity from JSON configuration
    /// </summary>
    public async Task<CombatEntityStats> CreateMonsterAsync(string monsterConfigId, string? customName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _adventureDataService.GetEntityConfigAsync(monsterConfigId, cancellationToken);
            
            if (config == null)
            {
                throw new ArgumentException($"Monster configuration '{monsterConfigId}' not found.");
            }

            var name = customName ?? config.Name;
            
            return new CombatEntityStats(
                name: name,
                maxHealth: config.BaseHealth,
                damagePerSecond: config.BaseDamage,
                retreatThreshold: config.RetreatThreshold
            );
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            GameLogger.Error(ex, $"Failed to create monster '{monsterConfigId}'");
            throw new InvalidOperationException($"Failed to create monster '{monsterConfigId}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates any entity from JSON configuration
    /// </summary>
    public async Task<CombatEntityStats> CreateEntityAsync(string entityConfigId, string? customName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _adventureDataService.GetEntityConfigAsync(entityConfigId, cancellationToken);
            
            if (config == null)
            {
                throw new ArgumentException($"Entity configuration '{entityConfigId}' not found.");
            }

            var name = customName ?? config.Name;
            
            return new CombatEntityStats(
                name: name,
                maxHealth: config.BaseHealth,
                damagePerSecond: config.BaseDamage,
                retreatThreshold: config.RetreatThreshold
            );
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            GameLogger.Error(ex, $"Failed to create entity '{entityConfigId}'");
            throw new InvalidOperationException($"Failed to create entity '{entityConfigId}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a combat entity from a type configuration
    /// </summary>
    public CombatEntityStats CreateEntityFromConfig(EntityTypeConfig config, string? customName = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        var name = customName ?? config.Name;
        
        return new CombatEntityStats(
            name: name,
            maxHealth: config.BaseHealth,
            damagePerSecond: config.BaseDamage,
            retreatThreshold: config.RetreatThreshold
        );
    }
}
