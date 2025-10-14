#nullable enable

namespace Game.Items.Models;

/// <summary>
/// Defines the different types of armor based on weight and protection level.
/// </summary>
public enum ArmorType
{
    /// <summary>
    /// Light armor offering minimal protection but maximum mobility.
    /// </summary>
    Light,

    /// <summary>
    /// Medium armor providing balanced protection and mobility.
    /// </summary>
    Medium,

    /// <summary>
    /// Heavy armor offering maximum protection at the cost of mobility.
    /// </summary>
    Heavy
}
