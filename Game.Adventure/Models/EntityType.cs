#nullable enable

namespace Game.Adventure.Models;

/// <summary>
/// Defines the different types of entities in the adventure system.
/// Used for classification and behavior differentiation.
/// </summary>
public enum EntityType
{
    /// <summary>
    /// Player-controlled or player-affiliated characters that go on adventures.
    /// </summary>
    Adventurer,

    /// <summary>
    /// Hostile creatures encountered during adventures.
    /// </summary>
    Monster,

    /// <summary>
    /// Non-player characters for trading, quests, or dialogue.
    /// </summary>
    NPC,

    /// <summary>
    /// Powerful monsters with special mechanics and higher rewards.
    /// </summary>
    Boss
}
