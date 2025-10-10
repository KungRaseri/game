#nullable enable

using Game.Items.Data.Services;
using Game.Items.Models;
using Game.Items.Models.Materials;
using Game.Items.Utils;

namespace Game.Items.Data.Services;

/// <summary>
/// Service for creating items using JSON-loaded configurations
/// Replaces static ItemFactory with dependency-injected approach
/// </summary>
public class ItemCreationService
{
    private readonly ItemDataService _itemDataService;

    public ItemCreationService(ItemDataService itemDataService)
    {
        _itemDataService = itemDataService ?? throw new ArgumentNullException(nameof(itemDataService));
    }

    /// <summary>
    /// Creates a weapon by ID using JSON configuration
    /// </summary>
    public async Task<Weapon> CreateWeaponAsync(string weaponId, QualityTier quality = QualityTier.Common)
    {
        var config = await _itemDataService.GetWeaponConfigAsync(weaponId);
        if (config == null)
        {
            throw new ArgumentException($"Weapon configuration '{weaponId}' not found.");
        }

        return CreateWeaponFromConfig(config, quality);
    }

    /// <summary>
    /// Creates an armor by ID using JSON configuration
    /// </summary>
    public async Task<Armor> CreateArmorAsync(string armorId, QualityTier quality = QualityTier.Common)
    {
        var config = await _itemDataService.GetArmorConfigAsync(armorId);
        if (config == null)
        {
            throw new ArgumentException($"Armor configuration '{armorId}' not found.");
        }

        return CreateArmorFromConfig(config, quality);
    }

    /// <summary>
    /// Creates a material by ID using JSON configuration
    /// </summary>
    public async Task<Material> CreateMaterialAsync(string materialId, QualityTier quality = QualityTier.Common)
    {
        var config = await _itemDataService.GetMaterialConfigAsync(materialId);
        if (config == null)
        {
            throw new ArgumentException($"Material configuration '{materialId}' not found.");
        }

        return CreateMaterialFromConfig(config, quality);
    }

    /// <summary>
    /// Gets all available weapon configurations
    /// </summary>
    public async Task<IReadOnlyList<WeaponConfig>> GetAvailableWeaponsAsync()
    {
        return await _itemDataService.GetAllWeaponConfigsAsync();
    }

    /// <summary>
    /// Gets all available armor configurations
    /// </summary>
    public async Task<IReadOnlyList<ArmorConfig>> GetAvailableArmorAsync()
    {
        return await _itemDataService.GetAllArmorConfigsAsync();
    }

    /// <summary>
    /// Gets all available material configurations
    /// </summary>
    public async Task<IReadOnlyList<MaterialConfig>> GetAvailableMaterialsAsync()
    {
        return await _itemDataService.GetAllMaterialConfigsAsync();
    }

    /// <summary>
    /// Creates a weapon from configuration with quality tier modifiers
    /// </summary>
    private static Weapon CreateWeaponFromConfig(WeaponConfig config, QualityTier quality)
    {
        int damageBonus = QualityTierModifiers.GetWeaponDamageBonus(quality);
        int finalValue = QualityTierModifiers.CalculateItemValue(config.BaseValue, quality);

        return new Weapon(
            itemId: $"{config.ItemId}_{quality.ToString().ToLower()}",
            name: config.Name,
            description: config.Description,
            quality: quality,
            value: finalValue,
            damageBonus: config.BaseDamageBonus + damageBonus
        );
    }

    /// <summary>
    /// Creates armor from configuration with quality tier modifiers
    /// </summary>
    private static Armor CreateArmorFromConfig(ArmorConfig config, QualityTier quality)
    {
        int damageReduction = QualityTierModifiers.GetArmorDamageReduction(quality);
        int finalValue = QualityTierModifiers.CalculateItemValue(config.BaseValue, quality);

        return new Armor(
            itemId: $"{config.ItemId}_{quality.ToString().ToLower()}",
            name: config.Name,
            description: config.Description,
            quality: quality,
            value: finalValue,
            damageReduction: config.BaseDamageReduction + damageReduction
        );
    }

    /// <summary>
    /// Creates material from configuration with quality tier modifiers
    /// </summary>
    private static Material CreateMaterialFromConfig(MaterialConfig config, QualityTier quality)
    {
        int finalValue = QualityTierModifiers.CalculateItemValue(config.BaseValue, quality);

        return new Material(
            itemId: $"{config.ItemId}_{quality.ToString().ToLower()}",
            name: config.Name,
            description: config.Description,
            quality: quality,
            value: finalValue,
            category: config.Category,
            stackable: config.Stackable,
            maxStackSize: config.MaxStackSize
        );
    }
}
