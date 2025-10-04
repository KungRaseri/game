#nullable enable

namespace Game.Economy.Models;

/// <summary>
/// Types of business expenses for shop operations.
/// </summary>
public enum ExpenseType
{
    /// <summary>
    /// Monthly rent for the shop space.
    /// </summary>
    Rent,

    /// <summary>
    /// Utilities like electricity, water, heating.
    /// </summary>
    Utilities,

    /// <summary>
    /// Security costs for shop protection.
    /// </summary>
    Security,

    /// <summary>
    /// Staff wages and benefits.
    /// </summary>
    Staff,

    /// <summary>
    /// Equipment and inventory purchases.
    /// </summary>
    Equipment,

    /// <summary>
    /// Shop improvements and upgrades.
    /// </summary>
    Improvements,

    /// <summary>
    /// Marketing and advertising costs.
    /// </summary>
    Marketing,

    /// <summary>
    /// Maintenance and repairs.
    /// </summary>
    Maintenance,

    /// <summary>
    /// Insurance and legal fees.
    /// </summary>
    Insurance,

    /// <summary>
    /// Miscellaneous operating expenses.
    /// </summary>
    Miscellaneous
}