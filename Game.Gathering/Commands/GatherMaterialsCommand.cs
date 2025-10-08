#nullable enable

using Game.Core.CQS;
using Game.Items.Models.Materials;

namespace Game.Gathering.Commands;

/// <summary>
/// Command for the player to manually gather materials in Phase 1.
/// This simulates the ShopKeeper going out and collecting basic crafting materials.
/// </summary>
public record GatherMaterialsCommand : ICommand<GatherMaterialsResult>
{
    /// <summary>
    /// The gathering location or method (for future expansion).
    /// For Phase 1, this can be "forest", "hills", "meadow", etc.
    /// </summary>
    public string GatheringLocation { get; init; } = "surrounding_area";
    
    /// <summary>
    /// Effort level of the gathering attempt (affects yield).
    /// For Phase 1, this could be based on time spent or player choice.
    /// </summary>
    public GatheringEffort Effort { get; init; } = GatheringEffort.Normal;
}

/// <summary>
/// Result of a gathering operation.
/// </summary>
public record GatherMaterialsResult
{
    /// <summary>
    /// Materials successfully gathered.
    /// </summary>
    public IReadOnlyList<Drop> MaterialsGathered { get; init; } = new List<Drop>();
    
    /// <summary>
    /// Display message for the gathering result.
    /// </summary>
    public string ResultMessage { get; init; } = string.Empty;
    
    /// <summary>
    /// Whether the gathering was successful.
    /// </summary>
    public bool IsSuccess { get; init; } = true;
}

/// <summary>
/// Gathering effort levels that affect yield.
/// </summary>
public enum GatheringEffort
{
    Quick,    // Fast gathering, lower yield
    Normal,   // Standard gathering
    Thorough  // Careful gathering, higher yield but takes longer
}
