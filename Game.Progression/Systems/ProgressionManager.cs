#nullable enable

using Game.Core.Utils;
using Game.Progression.Models;

namespace Game.Progression.Systems;

/// <summary>
/// Manages player progression milestones and phase transitions.
/// </summary>
public class ProgressionManager
{
    private readonly Dictionary<string, ProgressionMilestone> _milestones;
    private PlayerProgress _currentProgress;

    /// <summary>
    /// Event raised when a milestone is completed.
    /// </summary>
    public event Action<ProgressionMilestone>? MilestoneCompleted;

    /// <summary>
    /// Event raised when the player advances to a new game phase.
    /// </summary>
    public event Action<GamePhase, GamePhase>? PhaseAdvanced;

    /// <summary>
    /// Event raised when player progress is updated.
    /// </summary>
    public event Action<PlayerProgress>? ProgressUpdated;

    /// <summary>
    /// Current player progress.
    /// </summary>
    public PlayerProgress CurrentProgress => _currentProgress.Clone();

    /// <summary>
    /// All configured milestones.
    /// </summary>
    public IReadOnlyCollection<ProgressionMilestone> Milestones => _milestones.Values;

    /// <summary>
    /// Current game phase.
    /// </summary>
    public GamePhase CurrentPhase => _currentProgress.CurrentPhase;

    public ProgressionManager(PlayerProgress? initialProgress = null)
    {
        _currentProgress = initialProgress ?? new PlayerProgress();
        _milestones = new Dictionary<string, ProgressionMilestone>();
        
        InitializeDefaultMilestones();
        GameLogger.Info("ProgressionManager initialized");
    }

    /// <summary>
    /// Initializes the default Phase 1 to Phase 2 progression milestones.
    /// </summary>
    private void InitializeDefaultMilestones()
    {
        // Phase 1 -> Phase 2: Unlock Adventurer Management
        var adventurerUnlock = new ProgressionMilestone(
            milestoneId: "unlock_adventurer_management",
            name: "Ready for Adventure",
            description: "Earn 100 gold, craft 5 items, make 3 sales, and gather 3 material types to unlock adventurer hiring",
            unlocksPhase: GamePhase.AdventurerManagementPhase,
            requiredGold: 100m,
            requiredItemsCrafted: 5,
            requiredSales: 3,
            requiredMaterialTypesGathered: 3
        );

        _milestones[adventurerUnlock.MilestoneId] = adventurerUnlock;
        
        GameLogger.Info($"Initialized {_milestones.Count} progression milestones");
    }

    /// <summary>
    /// Updates player progress with new information and checks for milestone completion.
    /// </summary>
    /// <param name="updatedProgress">The updated progress information</param>
    public void UpdateProgress(PlayerProgress updatedProgress)
    {
        ArgumentNullException.ThrowIfNull(updatedProgress);

        var previousPhase = _currentProgress.CurrentPhase;
        _currentProgress = updatedProgress.Clone();

        // Check for completed milestones
        CheckAndCompleteMilestones();

        // Check for phase advancement
        if (_currentProgress.CurrentPhase != previousPhase)
        {
            PhaseAdvanced?.Invoke(previousPhase, _currentProgress.CurrentPhase);
            GameLogger.Info($"Player advanced from {previousPhase} to {_currentProgress.CurrentPhase}");
        }

        ProgressUpdated?.Invoke(_currentProgress);
    }

    /// <summary>
    /// Records that materials were gathered and updates progress accordingly.
    /// </summary>
    /// <param name="materialItemIds">The item IDs of materials gathered</param>
    /// <param name="totalQuantity">Total quantity gathered</param>
    public void RecordMaterialsGathered(IEnumerable<string> materialItemIds, int totalQuantity)
    {
        _currentProgress.RecordMaterialsGathered(materialItemIds, totalQuantity);
        CheckAndCompleteMilestones();
        ProgressUpdated?.Invoke(_currentProgress);
        
        GameLogger.Debug($"Recorded materials gathered: {string.Join(", ", materialItemIds)} (quantity: {totalQuantity})");
    }

    /// <summary>
    /// Records that an item was crafted.
    /// </summary>
    public void RecordItemCrafted()
    {
        _currentProgress.RecordItemCrafted();
        CheckAndCompleteMilestones();
        ProgressUpdated?.Invoke(_currentProgress);
        
        GameLogger.Debug($"Recorded item crafted - Total: {_currentProgress.ItemsCrafted}");
    }

    /// <summary>
    /// Records that a sale was completed.
    /// </summary>
    /// <param name="goldEarned">Amount of gold earned from the sale</param>
    public void RecordSale(decimal goldEarned)
    {
        _currentProgress.RecordSale(goldEarned);
        CheckAndCompleteMilestones();
        ProgressUpdated?.Invoke(_currentProgress);
        
        GameLogger.Debug($"Recorded sale - Gold earned: {goldEarned:C}, Total gold: {_currentProgress.CurrentGold:C}");
    }

    /// <summary>
    /// Gets the next uncompleted milestone for the current phase.
    /// </summary>
    /// <returns>The next milestone to complete, or null if all milestones are completed</returns>
    public ProgressionMilestone? GetNextMilestone()
    {
        return _milestones.Values
            .Where(m => !m.IsCompleted)
            .OrderBy(m => (int)m.UnlocksPhase)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets milestones for a specific phase.
    /// </summary>
    /// <param name="phase">The game phase to get milestones for</param>
    /// <returns>Milestones that unlock the specified phase</returns>
    public IEnumerable<ProgressionMilestone> GetMilestonesForPhase(GamePhase phase)
    {
        return _milestones.Values.Where(m => m.UnlocksPhase == phase);
    }

    /// <summary>
    /// Checks if the player is ready to advance to the next phase.
    /// </summary>
    /// <returns>True if ready to advance, false otherwise</returns>
    public bool IsReadyToAdvancePhase()
    {
        var nextPhaseMilestones = GetMilestonesForPhase((GamePhase)((int)_currentProgress.CurrentPhase + 1));
        return nextPhaseMilestones.All(m => m.IsCompleted);
    }

    /// <summary>
    /// Forces advancement to the next phase (for testing or special cases).
    /// </summary>
    public void ForceAdvancePhase()
    {
        var currentPhaseInt = (int)_currentProgress.CurrentPhase;
        var nextPhase = (GamePhase)(currentPhaseInt + 1);
        
        if (Enum.IsDefined(typeof(GamePhase), nextPhase))
        {
            var previousPhase = _currentProgress.CurrentPhase;
            _currentProgress.AdvanceToPhase(nextPhase);
            PhaseAdvanced?.Invoke(previousPhase, nextPhase);
            ProgressUpdated?.Invoke(_currentProgress);
            
            GameLogger.Info($"Force advanced from {previousPhase} to {nextPhase}");
        }
    }

    /// <summary>
    /// Gets a detailed progress report.
    /// </summary>
    /// <returns>Human-readable progress report</returns>
    public string GetProgressReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine($"=== Player Progress Report ===");
        report.AppendLine($"Current Phase: {_currentProgress.CurrentPhase}");
        report.AppendLine($"Gold: {_currentProgress.CurrentGold:C}");
        report.AppendLine($"Items Crafted: {_currentProgress.ItemsCrafted}");
        report.AppendLine($"Successful Sales: {_currentProgress.SuccessfulSales}");
        report.AppendLine($"Material Types Gathered: {_currentProgress.MaterialTypesGathered}");
        report.AppendLine($"Total Materials Gathered: {_currentProgress.TotalMaterialsGathered}");
        report.AppendLine();
        
        report.AppendLine("=== Milestones ===");
        foreach (var milestone in _milestones.Values.OrderBy(m => (int)m.UnlocksPhase))
        {
            var status = milestone.IsCompleted ? "✓ COMPLETED" : "⏳ IN PROGRESS";
            report.AppendLine($"{status}: {milestone.Name}");
            report.AppendLine($"  {milestone.Description}");
            
            if (!milestone.IsCompleted)
            {
                report.AppendLine("  Requirements:");
                if (milestone.RequiredGold > 0)
                    report.AppendLine($"    • Gold: {_currentProgress.CurrentGold:C} / {milestone.RequiredGold:C}");
                if (milestone.RequiredItemsCrafted > 0)
                    report.AppendLine($"    • Items Crafted: {_currentProgress.ItemsCrafted} / {milestone.RequiredItemsCrafted}");
                if (milestone.RequiredSales > 0)
                    report.AppendLine($"    • Sales: {_currentProgress.SuccessfulSales} / {milestone.RequiredSales}");
                if (milestone.RequiredMaterialTypesGathered > 0)
                    report.AppendLine($"    • Material Types: {_currentProgress.MaterialTypesGathered} / {milestone.RequiredMaterialTypesGathered}");
            }
            
            report.AppendLine();
        }
        
        return report.ToString();
    }

    /// <summary>
    /// Checks all milestones for completion and handles completion events.
    /// </summary>
    private void CheckAndCompleteMilestones()
    {
        foreach (var milestone in _milestones.Values.Where(m => !m.IsCompleted))
        {
            if (milestone.AreRequirementsMet(_currentProgress))
            {
                milestone.MarkCompleted();
                MilestoneCompleted?.Invoke(milestone);
                GameLogger.Info($"Milestone completed: {milestone.Name}");

                // Check if this milestone unlocks a new phase
                if (milestone.UnlocksPhase > _currentProgress.CurrentPhase)
                {
                    var previousPhase = _currentProgress.CurrentPhase;
                    _currentProgress.AdvanceToPhase(milestone.UnlocksPhase);
                    PhaseAdvanced?.Invoke(previousPhase, milestone.UnlocksPhase);
                    GameLogger.Info($"Phase unlocked: {milestone.UnlocksPhase}");
                }
            }
        }
    }

    /// <summary>
    /// Resets all progression data (for testing purposes).
    /// </summary>
    public void Reset()
    {
        _currentProgress = new PlayerProgress();
        foreach (var milestone in _milestones.Values)
        {
            milestone.Reset();
        }
        GameLogger.Info("Progression reset to initial state");
    }
}
