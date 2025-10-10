#nullable enable

using Game.Core.Data.Interfaces;
using Game.Core.Data.Models;
using Game.Core.Utils;
using Game.Items.Data.Models;
using Game.Items.Models.Materials;

namespace Game.Items.Data.Services;

/// <summary>
/// Service for loading item-related data from JSON files within the Game.Items domain
/// </summary>
public class ItemDataService
{
    private readonly IDataLoader<MaterialDataSet> _materialLoader;
    private readonly IDataLoader<WeaponDataSet> _weaponLoader;
    private readonly IDataLoader<ArmorDataSet> _armorLoader;
    private readonly IDataLoader<LootTableDataSet> _lootTableLoader;
    
    public ItemDataService(
        IDataLoader<MaterialDataSet> materialLoader,
        IDataLoader<WeaponDataSet> weaponLoader,
        IDataLoader<ArmorDataSet> armorLoader,
        IDataLoader<LootTableDataSet> lootTableLoader)
    {
        _materialLoader = materialLoader ?? throw new ArgumentNullException(nameof(materialLoader));
        _weaponLoader = weaponLoader ?? throw new ArgumentNullException(nameof(weaponLoader));
        _armorLoader = armorLoader ?? throw new ArgumentNullException(nameof(armorLoader));
        _lootTableLoader = lootTableLoader ?? throw new ArgumentNullException(nameof(lootTableLoader));
    }

    /// <summary>
    /// Gets all material configurations from JSON
    /// </summary>
    public async Task<IReadOnlyList<MaterialConfig>> GetAllMaterialConfigsAsync()
    {
        var dataPath = DataPath.GetDomainJsonPath("materials.json");
        var result = await _materialLoader.LoadAsync(dataPath);
        
        if (!result.IsSuccess)
        {
            GameLogger.Error($"Failed to load materials: {result.ErrorMessage}");
            return GetFallbackMaterials();
        }

        return result.Data?.Materials?.Select(m => m.ToMaterialConfig()).ToList().AsReadOnly() 
               ?? GetFallbackMaterials();
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
        var dataPath = DataPath.GetDomainJsonPath("weapons.json");
        var result = await _weaponLoader.LoadAsync(dataPath);
        
        if (!result.IsSuccess)
        {
            GameLogger.Error($"Failed to load weapons: {result.ErrorMessage}");
            return GetFallbackWeapons();
        }

        return result.Data?.Weapons?.Select(w => w.ToWeaponConfig()).ToList().AsReadOnly() 
               ?? GetFallbackWeapons();
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
        var dataPath = DataPath.GetDomainJsonPath("armor.json");
        var result = await _armorLoader.LoadAsync(dataPath);
        
        if (!result.IsSuccess)
        {
            GameLogger.Error($"Failed to load armor: {result.ErrorMessage}");
            return GetFallbackArmor();
        }

        return result.Data?.Armor?.Select(a => a.ToArmorConfig()).ToList().AsReadOnly() 
               ?? GetFallbackArmor();
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
