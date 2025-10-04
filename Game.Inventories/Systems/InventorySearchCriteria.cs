using Game.Inventories.Models;
using Game.Items.Models;
using Game.Items.Models.Materials;

namespace Game.Inventories.Systems;

/// <summary>
/// Criteria for searching and filtering inventory.
/// </summary>
public record InventorySearchCriteria(
    string? SearchTerm = null,
    Category? CategoryFilter = null,
    QualityTier? RarityFilter = null,
    int MinQuantity = 0,
    int MinValue = 0,
    MaterialSortBy SortBy = MaterialSortBy.Name,
    bool SortAscending = true
);