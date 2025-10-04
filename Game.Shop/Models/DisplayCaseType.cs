namespace Game.Shop.Models;

/// <summary>
/// Types of display cases available for shop slots.
/// Each type affects customer appeal and item presentation.
/// </summary>
public enum DisplayCaseType
{
    /// <summary>Basic wooden display case - no bonus effects.</summary>
    Basic = 0,

    /// <summary>Glass display case - increases perceived value by 10%.</summary>
    Glass = 1,

    /// <summary>Illuminated case - attracts more customer attention.</summary>
    Illuminated = 2,

    /// <summary>Security case - prevents theft, allows higher-value items.</summary>
    Security = 3,

    /// <summary>Premium showcase - maximum customer appeal and value perception.</summary>
    Premium = 4
}