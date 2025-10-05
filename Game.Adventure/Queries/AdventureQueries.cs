#nullable enable

using Game.Core.CQS;
using Game.Adventure.Models;

namespace Game.Adventure.Queries;

/// <summary>
/// Query to get the current adventurer's detailed status information.
/// Returns health, state, and combat information as a formatted string.
/// </summary>
public record GetAdventurerStatusQuery() : IQuery<string>;

/// <summary>
/// Query to get the current adventurer's state.
/// </summary>
public record GetAdventurerStateQuery() : IQuery<AdventurerState>;

/// <summary>
/// Query to check if the adventurer is available for a new expedition.
/// Returns true if the adventurer is in Idle state.
/// </summary>
public record IsAdventurerAvailableQuery() : IQuery<bool>;

/// <summary>
/// Query to get the current adventurer's stats.
/// Returns null if no adventurer is currently active.
/// </summary>
public record GetCurrentAdventurerQuery() : IQuery<CombatEntityStats?>;

/// <summary>
/// Query to get the current monster being fought.
/// Returns null if not in combat or no monster is active.
/// </summary>
public record GetCurrentMonsterQuery() : IQuery<CombatEntityStats?>;

/// <summary>
/// Query to check if the adventurer is currently in combat.
/// </summary>
public record IsAdventurerInCombatQuery() : IQuery<bool>;

/// <summary>
/// Query to check if there are monsters remaining in the current expedition.
/// </summary>
public record HasMonstersRemainingQuery() : IQuery<bool>;

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

/// <summary>
/// Query to get comprehensive adventurer information in a single call.
/// More efficient than making multiple separate queries.
/// </summary>
public record GetAdventurerInfoQuery() : IQuery<AdventurerInfo>;
