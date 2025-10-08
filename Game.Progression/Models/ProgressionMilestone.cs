#nullable enable

namespace Game.Progression.Models;

/// <summary>
/// Defines progression milestones that must be achieved to unlock new game phases.
/// </summary>
public class ProgressionMilestone
{
    /// <summary>
    /// Unique identifier for this milestone.
    /// </summary>
    public string MilestoneId { get; }
    
    /// <summary>
    /// Display name of the milestone.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Description of what needs to be accomplished.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// The game phase this milestone unlocks.
    /// </summary>
    public GamePhase UnlocksPhase { get; }
    
    /// <summary>
    /// Minimum gold required to complete this milestone.
    /// </summary>
    public decimal RequiredGold { get; }
    
    /// <summary>
    /// Number of items that must be crafted.
    /// </summary>
    public int RequiredItemsCrafted { get; }
    
    /// <summary>
    /// Number of successful shop sales required.
    /// </summary>
    public int RequiredSales { get; }
    
    /// <summary>
    /// Number of different material types that must be gathered.
    /// </summary>
    public int RequiredMaterialTypesGathered { get; }
    
    /// <summary>
    /// Whether this milestone has been completed.
    /// </summary>
    public bool IsCompleted { get; private set; }
    
    /// <summary>
    /// When this milestone was completed (null if not completed).
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    public ProgressionMilestone(
        string milestoneId,
        string name,
        string description,
        GamePhase unlocksPhase,
        decimal requiredGold = 0,
        int requiredItemsCrafted = 0,
        int requiredSales = 0,
        int requiredMaterialTypesGathered = 0)
    {
        if (string.IsNullOrWhiteSpace(milestoneId))
            throw new ArgumentException("Milestone ID cannot be null or empty", nameof(milestoneId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        MilestoneId = milestoneId;
        Name = name;
        Description = description;
        UnlocksPhase = unlocksPhase;
        RequiredGold = requiredGold;
        RequiredItemsCrafted = requiredItemsCrafted;
        RequiredSales = requiredSales;
        RequiredMaterialTypesGathered = requiredMaterialTypesGathered;
        IsCompleted = false;
        CompletedAt = null;
    }

    /// <summary>
    /// Marks this milestone as completed.
    /// </summary>
    public void MarkCompleted()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Resets this milestone to not completed (for testing purposes).
    /// </summary>
    public void Reset()
    {
        IsCompleted = false;
        CompletedAt = null;
    }

    /// <summary>
    /// Checks if the milestone requirements are met with current progress.
    /// </summary>
    /// <param name="progress">Current player progress</param>
    /// <returns>True if all requirements are met</returns>
    public bool AreRequirementsMet(PlayerProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        return progress.CurrentGold >= RequiredGold &&
               progress.ItemsCrafted >= RequiredItemsCrafted &&
               progress.SuccessfulSales >= RequiredSales &&
               progress.MaterialTypesGathered >= RequiredMaterialTypesGathered;
    }

    public override string ToString()
    {
        var status = IsCompleted ? "✓ Completed" : "In Progress";
        return $"{Name} [{status}] - Unlocks: {UnlocksPhase}";
    }
}
