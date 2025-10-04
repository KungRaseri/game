namespace Game.Inventories.Systems;

/// <summary>
/// Result of inventory search operation.
/// </summary>
/// <param name="Results">Matching material stacks</param>
/// <param name="TotalStacks">Number of matching stacks</param>
/// <param name="TotalMaterials">Total number of individual materials</param>
/// <param name="TotalValue">Combined value of matching materials</param>
public record InventorySearchResult(
    IReadOnlyList<MaterialStack> Results,
    int TotalStacks,
    int TotalMaterials,
    int TotalValue
);