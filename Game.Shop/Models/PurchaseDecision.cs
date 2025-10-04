namespace Game.Shop.Models;

/// <summary>
/// Represents a customer's purchase decision after evaluating an item.
/// </summary>
public enum PurchaseDecision
{
    /// <summary>Will not purchase the item.</summary>
    NotBuying = 0,

    /// <summary>Will purchase the item at asking price.</summary>
    Buying = 1,

    /// <summary>Wants to negotiate for a lower price first.</summary>
    WantsToNegotiate = 2,

    /// <summary>Will consider purchasing if specific conditions are met.</summary>
    Considering = 3
}