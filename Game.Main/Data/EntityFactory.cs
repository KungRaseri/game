using Game.Core.Models;

namespace Game.Main.Data;

/// <summary>
/// Factory for creating combat entities from configuration
/// </summary>
public static class EntityFactory
    {
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

        // Convenience methods for common entity types
        public static CombatEntityStats CreateNoviceAdventurer(string? name = null)
        {
            return name != null 
                ? CreateNamedEntity(EntityTypes.NoviceAdventurer, name)
                : CreateEntity(EntityTypes.NoviceAdventurer);
        }

        public static CombatEntityStats CreateGoblin(string? name = null)
        {
            return name != null 
                ? CreateNamedEntity(EntityTypes.Goblin, name)
                : CreateEntity(EntityTypes.Goblin);
        }

        public static CombatEntityStats CreateOrc(string? name = null)
        {
            return name != null 
                ? CreateNamedEntity(EntityTypes.Orc, name)
                : CreateEntity(EntityTypes.Orc);
        }

    public static CombatEntityStats CreateTroll(string? name = null)
    {
        return name != null 
            ? CreateNamedEntity(EntityTypes.Troll, name)
            : CreateEntity(EntityTypes.Troll);
    }
}
