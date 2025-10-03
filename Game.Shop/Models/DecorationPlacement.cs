using Godot;

namespace Game.Shop.Models;

/// <summary>
/// Represents a decorative element placed in the shop.
/// </summary>
public record DecorationPlacement(
    /// <summary>Type of decoration (plant, artwork, furniture, etc.).</summary>
    string DecorationType,
    
    /// <summary>Position in the shop space.</summary>
    Vector2 Position,
    
    /// <summary>Size/scale of the decoration.</summary>
    float Scale,
    
    /// <summary>Customer appeal bonus from this decoration.</summary>
    float AppealBonus
);