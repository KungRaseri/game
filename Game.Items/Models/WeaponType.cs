#nullable enable

namespace Game.Items.Models;

/// <summary>
/// Defines the different types of weapons available in the game.
/// </summary>
public enum WeaponType
{
    /// <summary>
    /// Single-handed blade weapon with balanced attack speed and reach.
    /// </summary>
    Sword,

    /// <summary>
    /// Heavy cutting weapon with slower attack speed but higher damage potential.
    /// </summary>
    Axe,

    /// <summary>
    /// Light, fast weapon with quick attack speed but shorter reach.
    /// </summary>
    Dagger
}
