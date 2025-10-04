namespace Game.Inventories.Systems;

/// <summary>
/// Result of inventory validation.
/// </summary>
/// <param name="IsValid">Whether the inventory is valid</param>
/// <param name="Issues">List of issues found</param>
/// <param name="FixedIssues">List of issues that were automatically fixed</param>
public record InventoryValidationResult(
    bool IsValid,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> FixedIssues
);