namespace Game.Crafting.Models;

/// <summary>
/// Defines the categories of crafting recipes available in the game.
/// </summary>
public enum RecipeCategory
{
    /// <summary>
    /// Weapons such as swords, bows, and staff.
    /// </summary>
    Weapons,

    /// <summary>
    /// Protective equipment like helmets, armor, and shields.
    /// </summary>
    Armor,

    /// <summary>
    /// Consumable items like potions, food, and scrolls.
    /// </summary>
    Consumables,

    /// <summary>
    /// Tools and equipment like pickaxes, hammers, and crafting implements.
    /// </summary>
    Tools,

    /// <summary>
    /// Raw materials and refined components used in other recipes.
    /// </summary>
    Materials
}
