using Game.Adventure.Models;

namespace Game.Adventure.Queries;

/// <summary>
/// Response object containing comprehensive adventurer information.
/// </summary>
public record AdventurerInfo(
    CombatEntityStats? Adventurer,
    AdventurerState State,
    bool IsAvailable,
    bool IsInCombat,
    CombatEntityStats? CurrentMonster,
    bool HasMonstersRemaining,
    string StatusInfo
);