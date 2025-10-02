namespace Game.Main.Models;

/// <summary>
/// Customer satisfaction levels for transactions and shop experience.
/// </summary>
public enum LegacyCustomerSatisfaction
{
    /// <summary>Customer was very unhappy with the transaction.</summary>
    VeryUnsatisfied = 1,
    
    /// <summary>Customer was somewhat disappointed.</summary>
    Unsatisfied = 2,
    
    /// <summary>Customer had a neutral experience.</summary>
    Neutral = 3,
    
    /// <summary>Customer was pleased with the transaction.</summary>
    Satisfied = 4,
    
    /// <summary>Customer was very happy and likely to return.</summary>
    VerySatisfied = 5
}