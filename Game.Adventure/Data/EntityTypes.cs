namespace Game.Adventure.Data;

/// <summary>
/// Predefined entity type configurations using modern C# features
/// </summary>
public static class EntityTypes
{
    // Adventurer Types
    public static EntityTypeConfig NoviceAdventurer { get; } = new(
        Name: "Novice Adventurer",
        BaseHealth: 100,
        BaseDamage: 10,
        RetreatThreshold: 0.25f,
        HealthRegenPerSecond: 1
    );

    public static EntityTypeConfig ExperiencedAdventurer { get; } = new(
        Name: "Experienced Adventurer", 
        BaseHealth: 150,
        BaseDamage: 15,
        RetreatThreshold: 0.20f,
        HealthRegenPerSecond: 2
    );

    // Monster Types  
    public static EntityTypeConfig Goblin { get; } = new(
        Name: "Goblin",
        BaseHealth: 20,
        BaseDamage: 5,
        RetreatThreshold: 0f // Monsters fight to the death
    );

    public static EntityTypeConfig Orc { get; } = new(
        Name: "Orc",
        BaseHealth: 40,
        BaseDamage: 8,
        RetreatThreshold: 0f
    );

    public static EntityTypeConfig Troll { get; } = new(
        Name: "Troll",
        BaseHealth: 80,
        BaseDamage: 12,
        RetreatThreshold: 0f
    );
}
