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
    // TODO: Uncomment when weapon/armor configs are implemented
    // private readonly IDataLoader<WeaponDataSet> _weaponLoader;
    // private readonly IDataLoader<ArmorDataSet> _armorLoader;
    private readonly IDataLoader<LootTableDataSet> _lootTableLoader;
    
    public ItemDataService(
        IDataLoader<MaterialDataSet> materialLoader,
        // IDataLoader<WeaponDataSet> weaponLoader,
        // IDataLoader<ArmorDataSet> armorLoader,
        IDataLoader<LootTableDataSet> lootTableLoader)
    {
        _materialLoader = materialLoader ?? throw new ArgumentNullException(nameof(materialLoader));
        // _weaponLoader = weaponLoader ?? throw new ArgumentNullException(nameof(weaponLoader));
        // _armorLoader = armorLoader ?? throw new ArgumentNullException(nameof(armorLoader));
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
    /// Fallback loot tables if JSON loading fails
    /// </summary>
    private Dictionary<string, LootTableData> GetFallbackLootTables()
    {
        GameLogger.Warning("Using fallback loot table configurations");
        return new Dictionary<string, LootTableData>();
    }
}
