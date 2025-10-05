using Game.Items.Models.Materials;
using Game.Items.Models;

namespace Game.Crafting.Models;

/// <summary>
/// Represents a material requirement for a crafting recipe.
/// </summary>
public class MaterialRequirement
{
    /// <summary>
    /// The category of material required (Metal, Wood, Leather, etc.).
    /// </summary>
    public Category MaterialCategory { get; }

    /// <summary>
    /// The minimum quality tier required for this material.
    /// </summary>
    public QualityTier MinimumQuality { get; }

    /// <summary>
    /// The quantity of this material required.
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Optional: specific material ID if a specific material is required.
    /// If null, any material of the specified category and quality will work.
    /// </summary>
    public string? SpecificMaterialId { get; }

    public MaterialRequirement(
        Category materialCategory,
        QualityTier minimumQuality,
        int quantity,
        string? specificMaterialId = null)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        MaterialCategory = materialCategory;
        MinimumQuality = minimumQuality;
        Quantity = quantity;
        SpecificMaterialId = specificMaterialId;
    }

    /// <summary>
    /// Checks if the provided material satisfies this requirement.
    /// </summary>
    /// <param name="material">The material to check</param>
    /// <returns>True if the material meets the requirements</returns>
    public bool IsSatisfiedBy(Material material)
    {
        if (material == null)
            return false;

        // Check specific material ID if required
        if (!string.IsNullOrEmpty(SpecificMaterialId) && 
            material.ItemId != SpecificMaterialId)
        {
            return false;
        }

        // Check category match
        if (material.Category != MaterialCategory)
            return false;

        // Check quality meets minimum requirement
        return material.Quality >= MinimumQuality;
    }

    public override string ToString()
    {
        var description = $"{Quantity}x {MaterialCategory} ({MinimumQuality}+)";
        if (!string.IsNullOrEmpty(SpecificMaterialId))
        {
            description += $" (Specific: {SpecificMaterialId})";
        }
        return description;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MaterialRequirement other)
            return false;

        return MaterialCategory == other.MaterialCategory &&
               MinimumQuality == other.MinimumQuality &&
               Quantity == other.Quantity &&
               SpecificMaterialId == other.SpecificMaterialId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MaterialCategory, MinimumQuality, Quantity, SpecificMaterialId);
    }
}
