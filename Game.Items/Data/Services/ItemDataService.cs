#nullable enable

using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;
using Game.Core.Data.Services;
using Game.Core.Extensions;
using Game.Core.Utils;
using Game.Items.Data.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Data.Services;

/// <summary>
/// Service for loading item-related data from JSON files within the Game.Items domain.
/// Supports hot-reload for development scenarios.
/// </summary>
public class ItemDataService
{
    private readonly IDataLoader<MaterialDataSet> _materialLoader;
    private readonly IDataLoader<WeaponDataSet> _weaponLoader;
    private readonly IDataLoader<ArmorDataSet> _armorLoader;
    private readonly IDataLoader<LootTableDataSet> _lootTableLoader;
    private readonly HotReloadService _hotReloadService;

    // Cache fields for hot-reload support
    private IReadOnlyList<MaterialConfig>? _cachedMaterials;
    private IReadOnlyList<WeaponConfig>? _cachedWeapons;
    private IReadOnlyList<ArmorConfig>? _cachedArmor;
    
    public ItemDataService(
        IDataLoader<MaterialDataSet> materialLoader,
        IDataLoader<WeaponDataSet> weaponLoader,
        IDataLoader<ArmorDataSet> armorLoader,
        IDataLoader<LootTableDataSet> lootTableLoader,
        HotReloadService hotReloadService)
    {
        _materialLoader = materialLoader ?? throw new ArgumentNullException(nameof(materialLoader));
        _weaponLoader = weaponLoader ?? throw new ArgumentNullException(nameof(weaponLoader));
        _armorLoader = armorLoader ?? throw new ArgumentNullException(nameof(armorLoader));
        _lootTableLoader = lootTableLoader ?? throw new ArgumentNullException(nameof(lootTableLoader));
        _hotReloadService = hotReloadService ?? throw new ArgumentNullException(nameof(hotReloadService));

        // Enable hot-reload for development
        EnableHotReload();
    }

    /// <summary>
    /// Enables hot-reload for all JSON files in the Items domain
    /// </summary>
    private void EnableHotReload()
    {
        _hotReloadService.EnableIfDevelopment();
        
        Action clearCache = ClearAllCaches;
        _hotReloadService.EnableForDomain("Items", clearCache.ToAsyncCallback());
    }

    /// <summary>
    /// Clears all cached data to force reload from files (useful for hot-reload scenarios)
    /// </summary>
    public void ClearAllCaches()
    {
        _cachedMaterials = null;
        _cachedWeapons = null;
        _cachedArmor = null;
        GameLogger.Debug("Items data cache cleared");
    }

    /// <summary>
    /// Gets all material configurations from JSON
    /// </summary>
    public async Task<IReadOnlyList<MaterialConfig>> GetAllMaterialConfigsAsync()
    {
        if (_cachedMaterials != null)
        {
            return _cachedMaterials;
        }

        var dataPath = DataPath.GetDomainJsonPath("materials.json");
        var result = await _materialLoader.LoadAsync(dataPath);
        
        if (!result.IsSuccess)
        {
            GameLogger.Error($"Failed to load materials: {result.ErrorMessage}");
            return GetFallbackMaterials();
        }

        _cachedMaterials = result.Data?.Materials?.Select(m => m.ToMaterialConfig()).ToList().AsReadOnly() 
               ?? GetFallbackMaterials();
               
        GameLogger.Debug($"Loaded {_cachedMaterials.Count} material configurations from JSON");
        return _cachedMaterials;
    }

    /// <summary>
    /// Gets a specific material configuration by ID
    /// </summary>
    public async Task<MaterialConfig?> GetMaterialConfigAsync(string materialId)
    {
        var materials = await GetAllMaterialConfigsAsync();
        return materials.FirstOrDefault(m => m.ItemId == materialId);
    }

    /// <summary>
    /// Gets all weapon configurations from JSON
    /// </summary>
    public async Task<IReadOnlyList<WeaponConfig>> GetAllWeaponConfigsAsync()
    {
        if (_cachedWeapons != null)
        {
            return _cachedWeapons;
        }

        var dataPath = DataPath.GetDomainJsonPath("weapons.json");
        var result = await _weaponLoader.LoadAsync(dataPath);
        
        if (!result.IsSuccess)
        {
            GameLogger.Error($"Failed to load weapons: {result.ErrorMessage}");
            return GetFallbackWeapons();
        }

        _cachedWeapons = result.Data?.Weapons?.Select(w => w.ToWeaponConfig()).ToList().AsReadOnly() 
               ?? GetFallbackWeapons();
               
        GameLogger.Debug($"Loaded {_cachedWeapons.Count} weapon configurations from JSON");
        return _cachedWeapons;
    }

    /// <summary>
    /// Gets a specific weapon configuration by ID
    /// </summary>
    public async Task<WeaponConfig?> GetWeaponConfigAsync(string weaponId)
    {
        var weapons = await GetAllWeaponConfigsAsync();
        return weapons.FirstOrDefault(w => w.ItemId == weaponId);
    }

    /// <summary>
    /// Gets all armor configurations from JSON
    /// </summary>
    public async Task<IReadOnlyList<ArmorConfig>> GetAllArmorConfigsAsync()
    {
        if (_cachedArmor != null)
        {
            return _cachedArmor;
        }

        var dataPath = DataPath.GetDomainJsonPath("armor.json");
        var result = await _armorLoader.LoadAsync(dataPath);
        
        if (!result.IsSuccess)
        {
            GameLogger.Error($"Failed to load armor: {result.ErrorMessage}");
            return GetFallbackArmor();
        }

        _cachedArmor = result.Data?.Armor?.Select(a => a.ToArmorConfig()).ToList().AsReadOnly() 
               ?? GetFallbackArmor();
               
        GameLogger.Debug($"Loaded {_cachedArmor.Count} armor configurations from JSON");
        return _cachedArmor;
    }

    /// <summary>
    /// Gets a specific armor configuration by ID
    /// </summary>
    public async Task<ArmorConfig?> GetArmorConfigAsync(string armorId)
    {
        var armor = await GetAllArmorConfigsAsync();
        return armor.FirstOrDefault(a => a.ItemId == armorId);
    }

    /// <summary>
    /// Gets loot tables from JSON
    /// </summary>
    public async Task<Dictionary<string, LootTableData>> GetLootTablesAsync()
    {
        var dataPath = DataPath.GetDomainJsonPath("loot-tables.json");
        var result = await _lootTableLoader.LoadAsync(dataPath);
        
        if (!result.IsSuccess)
        {
            GameLogger.Error($"Failed to load loot tables: {result.ErrorMessage}");
            return GetFallbackLootTables();
        }

        return result.Data?.LootTables?.ToDictionary(lt => lt.Id, lt => lt) 
               ?? GetFallbackLootTables();
    }

    /// <summary>
    /// Fallback materials if JSON loading fails
    /// </summary>
    private IReadOnlyList<MaterialConfig> GetFallbackMaterials()
    {
        GameLogger.Warning("Using fallback material configurations");
        return new List<MaterialConfig>
        {
            ItemTypes.IronOre,
            ItemTypes.OakWood,
            ItemTypes.MonsterHide
        }.AsReadOnly();
    }

    /// <summary>
    /// Fallback weapons if JSON loading fails
    /// </summary>
    private IReadOnlyList<WeaponConfig> GetFallbackWeapons()
    {
        GameLogger.Warning("Using fallback weapon configurations");
        return new List<WeaponConfig>
        {
            ItemTypes.IronSword,
            ItemTypes.SteelAxe,
            ItemTypes.MithrilDagger
        }.AsReadOnly();
    }

    /// <summary>
    /// Fallback armor if JSON loading fails
    /// </summary>
    private IReadOnlyList<ArmorConfig> GetFallbackArmor()
    {
        GameLogger.Warning("Using fallback armor configurations");
        return new List<ArmorConfig>
        {
            ItemTypes.LeatherArmor,
            ItemTypes.ChainMail,
            ItemTypes.PlateArmor
        }.AsReadOnly();
    }

    /// <summary>
    /// Fallback loot tables if JSON loading fails
    /// </summary>
    private Dictionary<string, LootTableData> GetFallbackLootTables()
    {
        GameLogger.Warning("Using fallback loot table configurations");
        return new Dictionary<string, LootTableData>();
    }
}
