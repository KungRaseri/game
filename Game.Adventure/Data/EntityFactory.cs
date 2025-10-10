using Game.Adventure.Models;
using Game.Adventure.Data.Services;

namespace Game.Adventure.Data;

/// <summary>
/// Factory for creating combat entities from configuration.
/// This class now uses JSON-based data services internally while maintaining the same API.
/// </summary>
public static class EntityFactory
{
    private static EntityCreationService? _entityCreationService;
    
    /// <summary>
    /// Sets the entity creation service for the factory (called during DI setup)
    /// </summary>
    public static void SetEntityCreationService(EntityCreationService entityCreationService)
    {
        _entityCreationService = entityCreationService ?? throw new ArgumentNullException(nameof(entityCreationService));
    }

    /// <summary>
    /// Creates a combat entity from a type configuration
    /// </summary>
    public static CombatEntityStats CreateEntity(EntityTypeConfig config)
    {
        return new CombatEntityStats(
            name: config.Name,
            maxHealth: config.BaseHealth,
            damagePerSecond: config.BaseDamage,
            retreatThreshold: config.RetreatThreshold
        );
    }

    /// <summary>
    /// Creates a named entity with custom stats based on a type configuration
    /// </summary>
    public static CombatEntityStats CreateNamedEntity(EntityTypeConfig config, string customName)
    {
        return new CombatEntityStats(
            name: customName,
            maxHealth: config.BaseHealth,
            damagePerSecond: config.BaseDamage,
            retreatThreshold: config.RetreatThreshold
        );
    }

    // Convenience methods for common entity types (now uses JSON-based service)
    public static CombatEntityStats CreateNoviceAdventurer(string? name = null)
    {
        if (_entityCreationService != null)
        {
            // Use JSON-based service if available
            var task = name != null 
                ? _entityCreationService.CreateAdventurerAsync("novice_adventurer", name)
                : _entityCreationService.CreateAdventurerAsync("novice_adventurer");
            return task.GetAwaiter().GetResult();
        }
        
        // Fallback to hardcoded configuration
        return name != null
            ? CreateNamedEntity(EntityTypes.NoviceAdventurer, name)
            : CreateEntity(EntityTypes.NoviceAdventurer);
    }

    public static CombatEntityStats CreateGoblin(string? name = null)
    {
        if (_entityCreationService != null)
        {
            // Use JSON-based service if available
            var task = name != null 
                ? _entityCreationService.CreateMonsterAsync("goblin", name)
                : _entityCreationService.CreateMonsterAsync("goblin");
            return task.GetAwaiter().GetResult();
        }
        
        // Fallback to hardcoded configuration
        return name != null
            ? CreateNamedEntity(EntityTypes.Goblin, name)
            : CreateEntity(EntityTypes.Goblin);
    }

    public static CombatEntityStats CreateOrc(string? name = null)
    {
        if (_entityCreationService != null)
        {
            // Use JSON-based service if available
            var task = name != null 
                ? _entityCreationService.CreateMonsterAsync("orc", name)
                : _entityCreationService.CreateMonsterAsync("orc");
            return task.GetAwaiter().GetResult();
        }
        
        // Fallback to hardcoded configuration
        return name != null
            ? CreateNamedEntity(EntityTypes.Orc, name)
            : CreateEntity(EntityTypes.Orc);
    }

    public static CombatEntityStats CreateTroll(string? name = null)
    {
        if (_entityCreationService != null)
        {
            // Use JSON-based service if available
            var task = name != null 
                ? _entityCreationService.CreateMonsterAsync("troll", name)
                : _entityCreationService.CreateMonsterAsync("troll");
            return task.GetAwaiter().GetResult();
        }
        
        // Fallback to hardcoded configuration
        return name != null
            ? CreateNamedEntity(EntityTypes.Troll, name)
            : CreateEntity(EntityTypes.Troll);
    }
}