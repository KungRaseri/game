using Game.Adventure.Models;
using Game.Core.Models;
using Game.Core.Utils;

namespace Game.Adventure.Systems;

/// <summary>
/// Generates material drops from loot tables using weighted probability calculations.
/// This is the core engine that determines what materials drop when monsters are defeated.
/// </summary>
public class LootGenerator
{
    private readonly Dictionary<string, LootTable> _lootTables;
    private readonly Random _random;

    /// <summary>
    /// Creates a new loot generator with the specified loot tables.
    /// </summary>
    /// <param name="lootTables">Dictionary of monster type ID to loot table mappings</param>
    /// <param name="random">Random number generator (optional, creates new if null)</param>
    public LootGenerator(Dictionary<string, LootTable> lootTables, Random? random = null)
    {
        _lootTables = lootTables;
        _random = random ?? new Random();

        // Validate all loot tables
        foreach (var table in _lootTables.Values)
        {
            table.Validate();
        }
    }

    /// <summary>
    /// Generates material drops for a defeated monster.
    /// </summary>
    /// <param name="monsterTypeId">The type of monster that was defeated</param>
    /// <returns>List of material drops generated</returns>
    public List<MaterialDrop> GenerateDrops(string monsterTypeId)
    {
        if (!_lootTables.TryGetValue(monsterTypeId, out var lootTable))
        {
            GameLogger.Warning($"No loot table found for monster type: {monsterTypeId}");
            return new List<MaterialDrop>();
        }

        var drops = new List<MaterialDrop>();
        var currentTime = DateTime.UtcNow;

        // Process each possible drop based on probability
        foreach (var entry in lootTable.PossibleDrops)
        {
            if (ShouldDrop(entry.DropChance))
            {
                var quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
                var rarity = DetermineActualRarity(entry);

                var drop = new MaterialDrop(
                    entry.Material,
                    rarity,
                    quantity,
                    currentTime
                );

                drops.Add(drop);
                GameLogger.Debug($"Generated drop: {drop}");
            }
        }

        // Ensure minimum guaranteed drops
        if (drops.Count < lootTable.GuaranteedDropCount)
        {
            GenerateGuaranteedDrops(lootTable, drops, currentTime);
        }

        // Limit to maximum drops
        if (drops.Count > lootTable.MaximumDropCount)
        {
            drops = drops.Take(lootTable.MaximumDropCount).ToList();
        }

        GameLogger.Info($"Generated {drops.Count} material drops for {monsterTypeId}");
        return drops;
    }

    /// <summary>
    /// Determines if a drop should occur based on the specified probability.
    /// </summary>
    private bool ShouldDrop(float dropChance)
    {
        return _random.NextSingle() < dropChance;
    }

    /// <summary>
    /// Determines the actual rarity of a drop, potentially upgrading from base rarity.
    /// </summary>
    private MaterialRarity DetermineActualRarity(LootEntry entry)
    {
        var baseRarity = entry.GetEffectiveRarity();

        // Small chance to upgrade rarity (5% chance per tier)
        var upgradeRoll = _random.NextSingle();
        
        return baseRarity switch
        {
            MaterialRarity.Common when upgradeRoll < 0.05f => MaterialRarity.Uncommon,
            MaterialRarity.Uncommon when upgradeRoll < 0.05f => MaterialRarity.Rare,
            MaterialRarity.Rare when upgradeRoll < 0.05f => MaterialRarity.Epic,
            MaterialRarity.Epic when upgradeRoll < 0.05f => MaterialRarity.Legendary,
            _ => baseRarity
        };
    }

    /// <summary>
    /// Generates additional drops to meet the guaranteed minimum count.
    /// </summary>
    private void GenerateGuaranteedDrops(LootTable lootTable, List<MaterialDrop> existingDrops, DateTime currentTime)
    {
        var dropsNeeded = lootTable.GuaranteedDropCount - existingDrops.Count;
        var availableEntries = lootTable.PossibleDrops.ToList();

        for (int i = 0; i < dropsNeeded && availableEntries.Count > 0; i++)
        {
            // Pick a random entry from remaining options
            var randomIndex = _random.Next(availableEntries.Count);
            var entry = availableEntries[randomIndex];

            var quantity = _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);
            var rarity = DetermineActualRarity(entry);

            var drop = new MaterialDrop(
                entry.Material,
                rarity,
                quantity,
                currentTime
            );

            existingDrops.Add(drop);
            GameLogger.Debug($"Generated guaranteed drop: {drop}");

            // Remove this entry to avoid duplicates in guaranteed drops
            availableEntries.RemoveAt(randomIndex);
        }
    }

    /// <summary>
    /// Gets statistics about drop rates for a specific monster type.
    /// </summary>
    public Dictionary<string, float> GetDropStatistics(string monsterTypeId)
    {
        if (!_lootTables.TryGetValue(monsterTypeId, out var lootTable))
        {
            return new Dictionary<string, float>();
        }

        var stats = new Dictionary<string, float>();
        
        foreach (var entry in lootTable.PossibleDrops)
        {
            stats[entry.Material.Name] = entry.DropChance;
        }

        return stats;
    }

    /// <summary>
    /// Checks if a loot table exists for the specified monster type.
    /// </summary>
    public bool HasLootTable(string monsterTypeId)
    {
        return _lootTables.ContainsKey(monsterTypeId);
    }
}
