namespace Game.Main.Systems.Inventory;

/// <summary>
/// Criteria for searching and filtering inventory.
/// </summary>
public record InventorySearchCriteria(
    string? SearchTerm = null,
    MaterialCategory? CategoryFilter = null,
    MaterialRarity? RarityFilter = null,
    int MinQuantity = 0,
    int MinValue = 0,
    MaterialSortBy SortBy = MaterialSortBy.Name,
    bool SortAscending = true
);