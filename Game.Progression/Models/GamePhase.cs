#nullable enable

namespace Game.Progression.Models;

/// <summary>
/// Represents the current game phase.
/// </summary>
public enum GamePhase
{
    /// <summary>Phase 1: Manual gathering, basic crafting, simple shop management</summary>
    ShopKeeperPhase = 1,
    
    /// <summary>Phase 2: Adventurer hiring unlocked, expedition management</summary>
    AdventurerManagementPhase = 2,
    
    /// <summary>Phase 3: Advanced features (future expansion)</summary>
    AdvancedPhase = 3
}
