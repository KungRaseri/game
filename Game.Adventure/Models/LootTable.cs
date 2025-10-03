using Game.Core.Models.Materials;

namespace Game.Adventure.Models;

/// <summary>
/// Defines the possible loot drops for a specific monster type.
/// Contains multiple LootEntry items with their probabilities and constraints.
/// </summary>
public class LootTable
{
    public string MonsterTypeId { get; }
    public List<LootEntry> PossibleDrops { get; }
    public int GuaranteedDropCount { get; }
    public int MaximumDropCount { get; }

    /// <summary>
    /// Creates a new loot table for a monster type.
    /// </summary>
    /// <param name="monsterTypeId">The ID of the monster type this table applies to</param>
    /// <param name="possibleDrops">List of possible material drops</param>
    /// <param name="guaranteedDropCount">Minimum number of materials that will always drop</param>
    /// <param name="maximumDropCount">Maximum number of materials that can drop at once</param>
    public LootTable(
        string monsterTypeId,
        List<LootEntry> possibleDrops,
        int guaranteedDropCount = 1,
        int maximumDropCount = 5)
    {
        MonsterTypeId = monsterTypeId;
        PossibleDrops = possibleDrops;
        GuaranteedDropCount = guaranteedDropCount;
        MaximumDropCount = maximumDropCount;

        Validate();
    }

    /// <summary>
    /// Validates that the loot table configuration is valid.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(MonsterTypeId))
        {
            throw new ArgumentException("Monster type ID cannot be null or empty", nameof(MonsterTypeId));
        }

        if (PossibleDrops.Count == 0)
        {
            throw new ArgumentException("Loot table must have at least one possible drop", nameof(PossibleDrops));
        }

        if (GuaranteedDropCount < 0)
        {
            throw new ArgumentException("Guaranteed drop count cannot be negative", nameof(GuaranteedDropCount));
        }

        if (MaximumDropCount < GuaranteedDropCount)
        {
            throw new ArgumentException("Maximum drop count cannot be less than guaranteed drop count", nameof(MaximumDropCount));
        }

        // Validate all loot entries
        foreach (var entry in PossibleDrops)
        {
            entry.Validate();
        }
    }

    /// <summary>
    /// Gets all loot entries that have a chance to drop, sorted by drop chance (highest first).
    /// </summary>
    public List<LootEntry> GetDropsByProbability()
    {
        return PossibleDrops
            .Where(entry => entry.DropChance > 0.0f)
            .OrderByDescending(entry => entry.DropChance)
            .ToList();
    }

    /// <summary>
    /// Gets all loot entries for a specific material category.
    /// </summary>
    public List<LootEntry> GetDropsByCategory(MaterialCategory category)
    {
        return PossibleDrops
            .Where(entry => entry.Material.Category == category)
            .ToList();
    }

    /// <summary>
    /// Gets all loot entries for a specific rarity level.
    /// </summary>
    public List<LootEntry> GetDropsByRarity(MaterialRarity rarity)
    {
        return PossibleDrops
            .Where(entry => entry.GetEffectiveRarity() == rarity)
            .ToList();
    }

    /// <summary>
    /// Creates a display string for this loot table.
    /// </summary>
    public override string ToString()
    {
        return $"LootTable for {MonsterTypeId}: {PossibleDrops.Count} possible drops, {GuaranteedDropCount}-{MaximumDropCount} guaranteed";
    }
}
