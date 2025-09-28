namespace Game.Main.Models
{
    /// <summary>
    /// Configuration data for different entity types
    /// </summary>
    public class EntityTypeConfig
    {
        public string Name { get; set; } = string.Empty;
        public int BaseHealth { get; set; }
        public int BaseDamage { get; set; }
        public float RetreatThreshold { get; set; }
        public int HealthRegenPerSecond { get; set; } = 0;

        public EntityTypeConfig() { }

        public EntityTypeConfig(string name, int baseHealth, int baseDamage, float retreatThreshold = 0f, int healthRegenPerSecond = 0)
        {
            Name = name;
            BaseHealth = baseHealth;
            BaseDamage = baseDamage;
            RetreatThreshold = retreatThreshold;
            HealthRegenPerSecond = healthRegenPerSecond;
        }
    }

    /// <summary>
    /// Predefined entity type configurations
    /// </summary>
    public static class EntityTypes
    {
        // Adventurer Types
        public static readonly EntityTypeConfig NoviceAdventurer = new(
            name: "Novice Adventurer",
            baseHealth: 100,
            baseDamage: 10,
            retreatThreshold: 0.25f,
            healthRegenPerSecond: 1
        );

        public static readonly EntityTypeConfig ExperiencedAdventurer = new(
            name: "Experienced Adventurer", 
            baseHealth: 150,
            baseDamage: 15,
            retreatThreshold: 0.20f,
            healthRegenPerSecond: 2
        );

        // Monster Types
        public static readonly EntityTypeConfig Goblin = new(
            name: "Goblin",
            baseHealth: 20,
            baseDamage: 5,
            retreatThreshold: 0f // Monsters fight to the death
        );

        public static readonly EntityTypeConfig Orc = new(
            name: "Orc",
            baseHealth: 40,
            baseDamage: 8,
            retreatThreshold: 0f
        );

        public static readonly EntityTypeConfig Troll = new(
            name: "Troll",
            baseHealth: 80,
            baseDamage: 12,
            retreatThreshold: 0f
        );
    }
}
