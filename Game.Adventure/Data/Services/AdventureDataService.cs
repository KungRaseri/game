#nullable enable

using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;
using Game.Core.Data.Services;
using Game.Core.Extensions;
using Game.Core.Utils;
using Game.Adventure.Data.Models;
using Game.Adventure.Data;

namespace Game.Adventure.Data.Services;

/// <summary>
/// Service for loading adventure-related data from JSON files within the Game.Adventure domain.
/// Supports hot-reload for development scenarios.
/// </summary>
public class AdventureDataService
{
    private readonly IDataLoader<EntityDataSet> _entityLoader;
    private readonly HotReloadService _hotReloadService;
    
    private IReadOnlyList<EntityTypeConfig>? _cachedAdventurerConfigs;
    private IReadOnlyList<EntityTypeConfig>? _cachedMonsterConfigs;
    private IReadOnlyDictionary<string, EntityTypeConfig>? _cachedEntityLookup;
    
    public AdventureDataService(IDataLoader<EntityDataSet> entityLoader, HotReloadService hotReloadService)
    {
        _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        _hotReloadService = hotReloadService ?? throw new ArgumentNullException(nameof(hotReloadService));

        // Enable hot-reload for development
        EnableHotReload();
    }

    /// <summary>
    /// Enables hot-reload for the Adventure domain JSON files
    /// </summary>
    private void EnableHotReload()
    {
        _hotReloadService.EnableIfDevelopment();
        
        Action clearCache = ClearCache;
        _hotReloadService.EnableForDomain("Adventure", clearCache.ToAsyncCallback());
    }

    /// <summary>
    /// Gets all adventurer configurations from JSON
    /// </summary>
    public async Task<IReadOnlyList<EntityTypeConfig>> GetAdventurerConfigsAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedAdventurerConfigs != null)
        {
            return _cachedAdventurerConfigs;
        }

        try
        {
            var dataPath = DataPath.GetDomainJsonPath("entities.json");
            var result = await _entityLoader.LoadAsync(dataPath, cancellationToken);
            
            if (!result.IsSuccess)
            {
                GameLogger.Error($"Failed to load adventurer configurations: {result.ErrorMessage}");
                return GetFallbackAdventurers();
            }

            _cachedAdventurerConfigs = result.Data?.Adventurers
                .Select(adventurer => adventurer.ToEntityTypeConfig())
                .ToList()
                .AsReadOnly() ?? GetFallbackAdventurers();

            GameLogger.Debug($"Loaded {_cachedAdventurerConfigs.Count} adventurer configurations from JSON");
            return _cachedAdventurerConfigs;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to load adventurer configurations from JSON");
            return GetFallbackAdventurers();
        }
    }

    /// <summary>
    /// Gets all monster configurations from JSON
    /// </summary>
    public async Task<IReadOnlyList<EntityTypeConfig>> GetMonsterConfigsAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedMonsterConfigs != null)
        {
            return _cachedMonsterConfigs;
        }

        try
        {
            var dataPath = DataPath.GetDomainJsonPath("entities.json");
            var result = await _entityLoader.LoadAsync(dataPath, cancellationToken);
            
            if (!result.IsSuccess)
            {
                GameLogger.Error($"Failed to load monster configurations: {result.ErrorMessage}");
                return GetFallbackMonsters();
            }

            _cachedMonsterConfigs = result.Data?.Monsters
                .Select(monster => monster.ToEntityTypeConfig())
                .ToList()
                .AsReadOnly() ?? GetFallbackMonsters();

            GameLogger.Debug($"Loaded {_cachedMonsterConfigs.Count} monster configurations from JSON");
            return _cachedMonsterConfigs;
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to load monster configurations from JSON");
            return GetFallbackMonsters();
        }
    }

    /// <summary>
    /// Gets a specific entity configuration by ID
    /// </summary>
    public async Task<EntityTypeConfig?> GetEntityConfigAsync(string entityId, CancellationToken cancellationToken = default)
    {
        await EnsureEntityLookupCachedAsync(cancellationToken);
        
        _cachedEntityLookup!.TryGetValue(entityId, out var config);
        return config;
    }

    /// <summary>
    /// Gets all entity configurations (adventurers and monsters)
    /// </summary>
    public async Task<IReadOnlyList<EntityTypeConfig>> GetAllEntityConfigsAsync(CancellationToken cancellationToken = default)
    {
        var adventurers = await GetAdventurerConfigsAsync(cancellationToken);
        var monsters = await GetMonsterConfigsAsync(cancellationToken);
        
        return adventurers.Concat(monsters).ToList().AsReadOnly();
    }

    /// <summary>
    /// Clears the cache to force reload from files (useful for hot-reload scenarios)
    /// </summary>
    public void ClearCache()
    {
        _cachedAdventurerConfigs = null;
        _cachedMonsterConfigs = null;
        _cachedEntityLookup = null;
        GameLogger.Debug("Adventure data cache cleared");
    }

    private async Task EnsureEntityLookupCachedAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedEntityLookup != null)
        {
            return;
        }

        try
        {
            var dataPath = DataPath.GetDomainJsonPath("entities.json");
            var result = await _entityLoader.LoadAsync(dataPath, cancellationToken);
            
            if (!result.IsSuccess)
            {
                GameLogger.Error($"Failed to build entity lookup cache: {result.ErrorMessage}");
                _cachedEntityLookup = new Dictionary<string, EntityTypeConfig>().AsReadOnly();
                return;
            }

            var lookup = new Dictionary<string, EntityTypeConfig>();
            
            if (result.Data != null)
            {
                foreach (var entity in result.Data.GetAllEntities())
                {
                    lookup[entity.EntityId] = entity.ToEntityTypeConfig();
                }
            }

            _cachedEntityLookup = lookup.AsReadOnly();
            GameLogger.Debug($"Cached {_cachedEntityLookup.Count} entity configurations for lookup");
        }
        catch (Exception ex)
        {
            GameLogger.Error(ex, "Failed to build entity lookup cache");
            _cachedEntityLookup = new Dictionary<string, EntityTypeConfig>().AsReadOnly();
        }
    }

    /// <summary>
    /// Fallback adventurer configurations for when JSON loading fails
    /// </summary>
    private static IReadOnlyList<EntityTypeConfig> GetFallbackAdventurers()
    {
        return new List<EntityTypeConfig>
        {
            new("Novice Adventurer", 100, 10, 0.25f, 1),
            new("Experienced Adventurer", 150, 15, 0.20f, 2)
        }.AsReadOnly();
    }

    /// <summary>
    /// Fallback monster configurations for when JSON loading fails
    /// </summary>
    private static IReadOnlyList<EntityTypeConfig> GetFallbackMonsters()
    {
        return new List<EntityTypeConfig>
        {
            new("Goblin", 20, 5, 0f, 0),
            new("Orc", 40, 8, 0f, 0),
            new("Troll", 80, 12, 0f, 0)
        }.AsReadOnly();
    }
}
