#nullable enable

using System;
using Game.Adventure.Models;
using Game.Adventure.Systems;

namespace Game.Adventure.Systems;

/// <summary>
/// Adventure management system that coordinates combat and adventurer activities.
/// Replaces the controller pattern with a system that can work with CQS.
/// Contains events for UI integration and manages the overall adventure lifecycle.
/// </summary>
public class AdventureSystem : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private bool _disposed;

    public CombatSystem CombatSystem => _combatSystem;

    // Events for UI and external system integration
    public event Action<string>? StatusUpdated;
    public event Action<AdventurerState>? StateChanged;
    public event Action<CombatEntityStats>? MonsterDefeated;
    public event Action? ExpeditionCompleted;

    public AdventureSystem(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));

        // Subscribe to combat system events and forward them
        _combatSystem.StateChanged += OnStateChanged;
        _combatSystem.CombatLogUpdated += OnCombatLogUpdated;
        _combatSystem.MonsterDefeated += OnMonsterDefeated;
        _combatSystem.ExpeditionCompleted += OnExpeditionCompleted;
    }

    /// <summary>
    /// Creates a static factory method for easy instantiation with a new combat system.
    /// </summary>
    public static AdventureSystem Create()
    {
        var combatSystem = new CombatSystem();
        return new AdventureSystem(combatSystem);
    }

    private void OnStateChanged(AdventurerState newState)
    {
        var message = $"Adventurer state changed to: {newState}";
        UpdateStatus(message);
        StateChanged?.Invoke(newState);
    }

    private void OnCombatLogUpdated(string logMessage)
    {
        UpdateStatus(logMessage);
    }

    private void OnMonsterDefeated(CombatEntityStats monster)
    {
        var message = $"Victory! {monster.Name} has been defeated!";
        UpdateStatus(message);
        MonsterDefeated?.Invoke(monster);
    }

    private void OnExpeditionCompleted()
    {
        var state = _combatSystem.State;
        var message = state switch
        {
            AdventurerState.Retreating => "Expedition ended - adventurer retreated safely",
            AdventurerState.Regenerating => "Expedition completed successfully!",
            _ => "Expedition ended"
        };
        UpdateStatus(message);
        ExpeditionCompleted?.Invoke();
    }

    private void UpdateStatus(string message)
    {
        StatusUpdated?.Invoke(message);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _combatSystem.StateChanged -= OnStateChanged;
        _combatSystem.CombatLogUpdated -= OnCombatLogUpdated;
        _combatSystem.MonsterDefeated -= OnMonsterDefeated;
        _combatSystem.ExpeditionCompleted -= OnExpeditionCompleted;

        _disposed = true;
    }
}
