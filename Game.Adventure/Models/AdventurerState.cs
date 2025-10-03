namespace Game.Adventure.Models;

/// <summary>
/// Defines the different states an adventurer can be in
/// </summary>
public enum AdventurerState
{
    Idle,           // In town, ready to be sent out
    Fighting,       // Currently in combat with a monster
    Retreating,     // Health too low, returning to town
    Regenerating,   // Recovering health after expedition
    Traveling       // Moving between town and dungeon
}
