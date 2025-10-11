using Game.Items.Models.Materials;
using Game.Items.Models;

namespace Game.Crafting.Models;

/// <summary>
/// Represents an individual crafting order with progress tracking.
/// </summary>
public class CraftingOrder
{
    /// <summary>
    /// Unique identifier for this crafting order.
    /// </summary>
    public string OrderId { get; }

    /// <summary>
    /// The recipe being crafted.
    /// </summary>
    public Recipe Recipe { get; }

    /// <summary>
    /// The specific materials allocated for this crafting order.
    /// </summary>
    public IReadOnlyDictionary<string, Material> AllocatedMaterials { get; }

    /// <summary>
    /// Current status of the crafting order.
    /// </summary>
    public CraftingStatus Status { get; private set; }

    /// <summary>
    /// Current progress as a percentage (0.0 to 1.0).
    /// </summary>
    public double Progress { get; private set; }

    /// <summary>
    /// When this order was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// When this order was started (moved from Queued to InProgress).
    /// </summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>
    /// When this order was completed (success, failure, or cancellation).
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// The estimated completion time based on the recipe's crafting time.
    /// </summary>
    public DateTime EstimatedCompletionTime => StartedAt?.AddSeconds(Recipe.CraftingTime) ?? DateTime.MaxValue;

    /// <summary>
    /// Error message if the crafting failed.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// The actual quality of the final product (determined when crafting completes).
    /// </summary>
    public QualityTier? FinalQuality { get; private set; }

    public CraftingOrder(
        string orderId,
        Recipe recipe,
        IReadOnlyDictionary<string, Material> allocatedMaterials)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order ID cannot be null or empty", nameof(orderId));
        }

        OrderId = orderId;
        Recipe = recipe ?? throw new ArgumentNullException(nameof(recipe));
        AllocatedMaterials = allocatedMaterials ?? throw new ArgumentNullException(nameof(allocatedMaterials));
        Status = CraftingStatus.Queued;
        Progress = 0.0;
        CreatedAt = DateTime.UtcNow;

        ValidateAllocatedMaterials();
    }

    /// <summary>
    /// Starts the crafting process.
    /// </summary>
    public void Start()
    {
        if (Status != CraftingStatus.Queued)
        {
            throw new InvalidOperationException($"Cannot start order with status {Status}");
        }

        Status = CraftingStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the progress of the crafting order.
    /// </summary>
    /// <param name="newProgress">Progress value between 0.0 and 1.0</param>
    public void UpdateProgress(double newProgress)
    {
        if (Status != CraftingStatus.InProgress)
        {
            throw new InvalidOperationException($"Cannot update progress of order with status {Status}");
        }

        Progress = Math.Clamp(newProgress, 0.0, 1.0);
    }

    /// <summary>
    /// Marks the order as completed successfully.
    /// </summary>
    /// <param name="finalQuality">The final quality of the crafted item</param>
    public void Complete(QualityTier finalQuality)
    {
        if (Status != CraftingStatus.InProgress)
        {
            throw new InvalidOperationException($"Cannot complete order with status {Status}");
        }

        Status = CraftingStatus.Completed;
        Progress = 1.0;
        CompletedAt = DateTime.UtcNow;
        FinalQuality = finalQuality;
    }

    /// <summary>
    /// Marks the order as failed.
    /// </summary>
    /// <param name="reason">Reason for the failure</param>
    public void Fail(string reason)
    {
        if (Status != CraftingStatus.InProgress)
        {
            throw new InvalidOperationException($"Cannot fail order with status {Status}");
        }

        Status = CraftingStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        FailureReason = reason ?? "Unknown failure";
    }

    /// <summary>
    /// Cancels the order.
    /// </summary>
    public void Cancel()
    {
        if (Status == CraftingStatus.Completed || Status == CraftingStatus.Failed)
        {
            throw new InvalidOperationException($"Cannot cancel order with status {Status}");
        }

        Status = CraftingStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the remaining time for this order to complete.
    /// </summary>
    /// <returns>Remaining time, or null if not in progress</returns>
    public TimeSpan? GetRemainingTime()
    {
        if (Status != CraftingStatus.InProgress || StartedAt == null)
        {
            return null;
        }

        var remainingSeconds = Recipe.CraftingTime * (1.0 - Progress);
        return TimeSpan.FromSeconds(remainingSeconds);
    }

    /// <summary>
    /// Calculates the actual success rate for this order based on material quality.
    /// </summary>
    /// <returns>Success rate as a percentage (0-100)</returns>
    public double CalculateSuccessRate()
    {
        var baseRate = Recipe.CalculateBaseSuccessRate();
        
        // Bonus based on material quality above minimum requirements
        var qualityBonus = CalculateQualityBonus();
        
        return Math.Min(99.0, baseRate + qualityBonus);
    }

    /// <summary>
    /// Calculates quality bonus from using higher quality materials.
    /// </summary>
    /// <returns>Success rate bonus percentage</returns>
    private double CalculateQualityBonus()
    {
        double totalBonus = 0.0;
        int requirementCount = 0;

        foreach (var requirement in Recipe.MaterialRequirements)
        {
            var allocatedMaterial = FindAllocatedMaterial(requirement);
            if (allocatedMaterial != null)
            {
                var qualityDifference = (int)allocatedMaterial.Quality - (int)requirement.MinimumQuality;
                totalBonus += qualityDifference * 2.0; // 2% bonus per quality tier above minimum
                requirementCount++;
            }
        }

        return requirementCount > 0 ? totalBonus / requirementCount : 0.0;
    }

    /// <summary>
    /// Finds the allocated material for a specific requirement.
    /// </summary>
    /// <param name="requirement">The material requirement</param>
    /// <returns>The allocated material, or null if not found</returns>
    private Material? FindAllocatedMaterial(MaterialRequirement requirement)
    {
        return AllocatedMaterials.Values.FirstOrDefault(material => 
            requirement.IsSatisfiedBy(material));
    }

    /// <summary>
    /// Validates that the allocated materials satisfy all recipe requirements.
    /// </summary>
    private void ValidateAllocatedMaterials()
    {
        foreach (var requirement in Recipe.MaterialRequirements)
        {
            var satisfiedQuantity = AllocatedMaterials.Values
                .Where(requirement.IsSatisfiedBy)
                .Sum(_ => 1); // Each material in the dictionary represents one unit

            if (satisfiedQuantity < requirement.Quantity)
            {
                throw new ArgumentException(
                    $"Insufficient materials for requirement: {requirement}. " +
                    $"Required: {requirement.Quantity}, Allocated: {satisfiedQuantity}");
            }
        }
    }

    public override string ToString()
    {
        var statusInfo = Status == CraftingStatus.InProgress ? $" ({Progress:P1})" : "";
        return $"Order {OrderId}: {Recipe.Name} - {Status}{statusInfo}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CraftingOrder other)
            return false;

        return OrderId == other.OrderId;
    }

    public override int GetHashCode()
    {
        return OrderId.GetHashCode();
    }
}
