namespace Game.Adventure.Data;

/// <summary>
/// Configuration data for different entity types
/// Uses modern C# record type for immutable data
/// </summary>
public record EntityTypeConfig(
    string Name,
    int BaseHealth,
    int BaseDamage,
    float RetreatThreshold = 0f,
    int HealthRegenPerSecond = 0
)
{
}