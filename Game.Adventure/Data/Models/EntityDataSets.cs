#nullable enable

using Game.Adventure.Data;

namespace Game.Adventure.Data.Models;

/// <summary>
/// Data model for entity configurations loaded from JSON
/// </summary>
public record EntityData
{
    public required string EntityId { get; init; }
    public required string Name { get; init; }
    public required int BaseHealth { get; init; }
    public required int BaseDamage { get; init; }
    public float RetreatThreshold { get; init; } = 0f;
    public int HealthRegenPerSecond { get; init; } = 0;
    public string EntityType { get; init; } = "monster"; // "adventurer" or "monster"
    public string Description { get; init; } = "";

    /// <summary>
    /// Converts JSON data to domain configuration
    /// </summary>
    public EntityTypeConfig ToEntityTypeConfig()
    {
        return new EntityTypeConfig(
            Name: Name,
            BaseHealth: BaseHealth,
            BaseDamage: BaseDamage,
            RetreatThreshold: RetreatThreshold,
            HealthRegenPerSecond: HealthRegenPerSecond
        );
    }
}

/// <summary>
/// Root dataset for entity configurations
/// </summary>
public record EntityDataSet
{
    public IReadOnlyList<EntityData> Adventurers { get; init; } = new List<EntityData>();
    public IReadOnlyList<EntityData> Monsters { get; init; } = new List<EntityData>();

    /// <summary>
    /// Gets all entities as a flat collection
    /// </summary>
    public IEnumerable<EntityData> GetAllEntities()
    {
        return Adventurers.Concat(Monsters);
    }

    /// <summary>
    /// Gets entity by ID
    /// </summary>
    public EntityData? GetEntityById(string entityId)
    {
        return GetAllEntities().FirstOrDefault(e => e.EntityId == entityId);
    }

    /// <summary>
    /// Gets entities by type
    /// </summary>
    public IEnumerable<EntityData> GetEntitiesByType(string entityType)
    {
        return GetAllEntities().Where(e => e.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
    }
}
