using Game.Items.Models;

namespace Game.Crafting.Models;

/// <summary>
/// Represents a crafting recipe with requirements, results, and metadata.
/// </summary>
public class Recipe
{
    /// <summary>
    /// Unique identifier for this recipe.
    /// </summary>
    public string RecipeId { get; }

    /// <summary>
    /// Display name of the recipe.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Description of what this recipe creates.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Category or type of recipe (Weapons, Armor, Consumables, etc.).
    /// </summary>
    public RecipeCategory Category { get; }

    /// <summary>
    /// List of materials required to craft this recipe.
    /// </summary>
    public IReadOnlyList<MaterialRequirement> MaterialRequirements { get; }

    /// <summary>
    /// The result produced when this recipe is successfully crafted.
    /// </summary>
    public CraftingResult Result { get; }

    /// <summary>
    /// Time in seconds required to complete this recipe.
    /// </summary>
    public double CraftingTime { get; }

    /// <summary>
    /// The difficulty level of this recipe (affects success rate).
    /// </summary>
    public int Difficulty { get; }

    /// <summary>
    /// Prerequisites that must be met before this recipe can be used.
    /// </summary>
    public IReadOnlyList<string> Prerequisites { get; }

    /// <summary>
    /// Whether this recipe has been unlocked and can be used.
    /// </summary>
    public bool IsUnlocked { get; private set; }

    /// <summary>
    /// Experience points gained when this recipe is successfully crafted.
    /// </summary>
    public int ExperienceReward { get; }

    public Recipe(
        string recipeId,
        string name,
        string description,
        RecipeCategory category,
        IReadOnlyList<MaterialRequirement> materialRequirements,
        CraftingResult result,
        double craftingTime,
        int difficulty = 1,
        IReadOnlyList<string>? prerequisites = null,
        bool isUnlocked = false,
        int experienceReward = 10)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            throw new ArgumentException("Recipe ID cannot be null or empty", nameof(recipeId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Recipe name cannot be null or empty", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Recipe description cannot be null or empty", nameof(description));
        }

        if (materialRequirements == null || materialRequirements.Count == 0)
        {
            throw new ArgumentException("Recipe must have at least one material requirement", nameof(materialRequirements));
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (craftingTime <= 0)
        {
            throw new ArgumentException("Crafting time must be greater than zero", nameof(craftingTime));
        }

        if (difficulty < 1)
        {
            throw new ArgumentException("Difficulty must be at least 1", nameof(difficulty));
        }

        if (experienceReward < 0)
        {
            throw new ArgumentException("Experience reward cannot be negative", nameof(experienceReward));
        }

        RecipeId = recipeId;
        Name = name;
        Description = description;
        Category = category;
        MaterialRequirements = materialRequirements;
        Result = result;
        CraftingTime = craftingTime;
        Difficulty = difficulty;
        Prerequisites = prerequisites ?? new List<string>();
        IsUnlocked = isUnlocked;
        ExperienceReward = experienceReward;
    }

    /// <summary>
    /// Unlocks this recipe for use.
    /// </summary>
    public void Unlock()
    {
        IsUnlocked = true;
    }

    /// <summary>
    /// Locks this recipe, preventing its use.
    /// </summary>
    public void Lock()
    {
        IsUnlocked = false;
    }

    /// <summary>
    /// Gets the total quantity of materials required for this recipe.
    /// </summary>
    /// <returns>Sum of all material quantities</returns>
    public int GetTotalMaterialCount()
    {
        return MaterialRequirements.Sum(req => req.Quantity);
    }

    /// <summary>
    /// Estimates the total value of materials required for this recipe.
    /// This is used for cost analysis and UI display.
    /// </summary>
    /// <param name="materialValueProvider">Function to get material values by category and quality</param>
    /// <returns>Estimated total material cost</returns>
    public int EstimateMaterialCost(Func<MaterialRequirement, int> materialValueProvider)
    {
        if (materialValueProvider == null)
        {
            throw new ArgumentNullException(nameof(materialValueProvider));
        }

        return MaterialRequirements.Sum(req => 
            materialValueProvider(req) * req.Quantity);
    }

    /// <summary>
    /// Calculates the base success rate for this recipe based on difficulty.
    /// </summary>
    /// <returns>Success rate as a percentage (0-100)</returns>
    public double CalculateBaseSuccessRate()
    {
        // Base success rate decreases with difficulty
        // Difficulty 1 = 95%, Difficulty 5 = 75%, Difficulty 10 = 50%
        return Math.Max(50.0, 100.0 - (Difficulty * 5.0));
    }

    public override string ToString()
    {
        var materialSummary = string.Join(", ", MaterialRequirements.Take(3).Select(req => req.ToString()));
        if (MaterialRequirements.Count > 3)
        {
            materialSummary += "...";
        }

        return $"{Name} [{Category}] - {materialSummary} → {Result.Quantity}x {Result.ItemName} ({CraftingTime}s)";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Recipe other)
            return false;

        return RecipeId == other.RecipeId;
    }

    public override int GetHashCode()
    {
        return RecipeId.GetHashCode();
    }
}
