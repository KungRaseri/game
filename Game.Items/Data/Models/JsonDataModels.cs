#nullable enable

using Game.Items.Data;
using Game.Items.Models;
using Game.Items.Models.Materials;
using System.Text.Json.Serialization;
using Game.Items.Data.Json;

namespace Game.Items.Data.Models;

/// <summary>
/// Root container for material data loaded from JSON
/// </summary>
public class MaterialDataSet
{
    public string Version { get; set; } = "1.0";
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public List<MaterialJsonData> Materials { get; set; } = new();
}

/// <summary>
/// JSON representation of material data
/// </summary>
public class MaterialJsonData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BaseValue { get; set; }
    public Category Category { get; set; }
    public QualityTier QualityTier { get; set; } = QualityTier.Common;
    public bool Stackable { get; set; } = true;
    public int MaxStackSize { get; set; } = 99;
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Converts this JSON data to a MaterialConfig
    /// </summary>
    public MaterialConfig ToMaterialConfig()
    {
        return new MaterialConfig(
            ItemId: Id,
            Name: Name,
            Description: Description,
            BaseValue: BaseValue,
            Category: Category,
            Stackable: Stackable,
            MaxStackSize: MaxStackSize
        );
    }
}

/// <summary>
/// Root container for weapon data loaded from JSON
/// </summary>
public class WeaponDataSet
{
    public string Version { get; set; } = "1.0";
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public List<WeaponJsonData> Weapons { get; set; } = new();
}

/// <summary>
/// JSON representation of weapon data
/// </summary>
public class WeaponJsonData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BaseValue { get; set; }
    public int BaseDamage { get; set; }
    public int BaseDurability { get; set; } = 100;
    
    [JsonConverter(typeof(WeaponTypeJsonConverter))]
    public WeaponType WeaponType { get; set; } = WeaponType.Sword;
    
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Converts this JSON data to a WeaponConfig
    /// </summary>
    public WeaponConfig ToWeaponConfig()
    {
        return new WeaponConfig(
            ItemId: Id,
            Name: Name,
            Description: Description,
            BaseValue: BaseValue,
            BaseDamageBonus: BaseDamage,
            WeaponType: WeaponType
        );
    }
}

/// <summary>
/// Root container for armor data loaded from JSON (placeholder for future implementation)
/// </summary>
public class ArmorDataSet
{
    public string Version { get; set; } = "1.0";
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public List<ArmorJsonData> Armor { get; set; } = new();
}

/// <summary>
/// JSON representation of armor data
/// </summary>
public class ArmorJsonData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BaseValue { get; set; }
    public int BaseDefense { get; set; }
    public int BaseDurability { get; set; } = 100;
    
    [JsonConverter(typeof(ArmorTypeJsonConverter))]
    public ArmorType ArmorType { get; set; } = ArmorType.Light;
    
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Converts this JSON data to an ArmorConfig
    /// </summary>
    public ArmorConfig ToArmorConfig()
    {
        return new ArmorConfig(
            ItemId: Id,
            Name: Name,
            Description: Description,
            BaseValue: BaseValue,
            BaseDamageReduction: BaseDefense,
            ArmorType: ArmorType
        );
    }
}

/// <summary>
/// Root container for loot table data loaded from JSON
/// </summary>
public class LootTableDataSet
{
    public string Version { get; set; } = "1.0";
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public List<LootTableData> LootTables { get; set; } = new();
}

/// <summary>
/// JSON representation of loot table data
/// </summary>
public class LootTableData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxDrops { get; set; } = 3;
    public List<LootEntryData> Entries { get; set; } = new();
}

/// <summary>
/// JSON representation of loot entry data
/// </summary>
public class LootEntryData
{
    public string ItemId { get; set; } = string.Empty;
    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 1;
    public double DropChance { get; set; } = 0.1;
    public string QualityTier { get; set; } = "Common";
}
