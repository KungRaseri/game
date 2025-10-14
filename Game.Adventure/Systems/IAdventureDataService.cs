#nullable enable

using Game.Adventure.Data;
using Game.Adventure.Data.Models;
using Game.Adventure.Models;

namespace Game.Adventure.Systems;

/// <summary>
/// Interface for Adventure data service operations to support entity type queries and caching.
/// </summary>
public interface IAdventureDataService
{
    /// <summary>
    /// Gets all adventurer configurations.
    /// </summary>
    Task<IReadOnlyList<EntityTypeConfig>> GetAdventurerConfigsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all monster configurations.
    /// </summary>
    Task<IReadOnlyList<EntityTypeConfig>> GetMonsterConfigsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets entity configurations by the specified entity type.
    /// </summary>
    Task<IReadOnlyList<EntityTypeConfig>> GetEntitiesByTypeAsync(EntityType entityType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entity configurations from all types.
    /// </summary>
    Task<IReadOnlyList<EntityTypeConfig>> GetAllEntityConfigsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an entity configuration by ID.
    /// </summary>
    Task<EntityTypeConfig?> GetEntityConfigAsync(string entityId, CancellationToken cancellationToken = default);
}
