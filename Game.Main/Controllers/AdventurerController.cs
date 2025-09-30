using System;
using System.Collections.Generic;
using Game.Main.Models;
using Game.Main.Systems;
using Game.Main.Data;

namespace Game.Main.Controllers;

/// <summary>
/// Controls adventurer actions and manages their state
/// </summary>
public class AdventurerController : IDisposable
{
    private readonly CombatEntityStats _adventurer;
    private readonly CombatSystem _combatSystem;

        public CombatEntityStats Adventurer => _adventurer;
        public AdventurerState State => _combatSystem.State;
        public bool IsAvailable => State == AdventurerState.Idle;
        public CombatEntityStats? CurrentMonster => _combatSystem.CurrentMonster;

        public event Action<string>? StatusUpdated;
        public event Action<AdventurerState>? StateChanged;
        public event Action<CombatEntityStats>? MonsterDefeated;
        public event Action? ExpeditionCompleted;

    public AdventurerController(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem ?? throw new ArgumentNullException(nameof(combatSystem));
        _adventurer = EntityFactory.CreateNoviceAdventurer();
        
        // Subscribe to combat system events
        _combatSystem.StateChanged += OnStateChanged;
        _combatSystem.CombatLogUpdated += OnCombatLogUpdated;
        _combatSystem.MonsterDefeated += OnMonsterDefeated;
        _combatSystem.ExpeditionCompleted += OnExpeditionCompleted;
    }

    /// <summary>
    /// Sends the adventurer on an expedition to the Goblin Cave
    /// </summary>
    public void SendToGoblinCave()
    {
        if (!IsAvailable)
        {
            UpdateStatus("Adventurer is not available for expedition");
            return;
        }

        // Create 3 goblins for the dungeon
        var monsters = new List<CombatEntityStats>
        {
            EntityFactory.CreateGoblin(),
            EntityFactory.CreateGoblin(),
            EntityFactory.CreateGoblin()
        };

        _combatSystem.StartExpedition(_adventurer, monsters);
        UpdateStatus("Adventurer departs for the Goblin Cave");
    }

    /// <summary>
    /// Forces the adventurer to retreat from current expedition
    /// </summary>
    public void Retreat()
    {
        _combatSystem.ForceRetreat();
        UpdateStatus("Retreat order given to adventurer");
    }

        /// <summary>
        /// Updates the adventurer's combat state (should be called regularly)
        /// </summary>
        public void Update()
        {
            _combatSystem.Update();
        }

        /// <summary>
        /// Updates the adventurer's combat state with fixed time step
        /// </summary>
        public void Update(float fixedDeltaTime)
        {
            _combatSystem.Update(fixedDeltaTime);
        }

    /// <summary>
    /// Gets current adventurer status information
    /// </summary>
    public string GetStatusInfo()
    {
        var healthInfo = $"HP: {_adventurer.CurrentHealth}/{_adventurer.MaxHealth} ({_adventurer.HealthPercentage:P0})";
        var stateInfo = $"State: {State}";
        var combatInfo = "";

        if (_combatSystem.CurrentMonster != null)
        {
            var monster = _combatSystem.CurrentMonster;
            combatInfo = $" | Fighting: {monster.Name} ({monster.CurrentHealth}/{monster.MaxHealth} HP)";
        }

        return $"{healthInfo} | {stateInfo}{combatInfo}";
    }

        private void OnStateChanged(AdventurerState newState)
        {
            UpdateStatus($"Adventurer state changed to: {newState}");
            StateChanged?.Invoke(newState);
        }

    private void OnCombatLogUpdated(string logMessage)
    {
        UpdateStatus(logMessage);
    }

        private void OnMonsterDefeated(CombatEntityStats monster)
        {
            UpdateStatus($"Victory! {monster.Name} has been defeated!");
            MonsterDefeated?.Invoke(monster);
        }

        private void OnExpeditionCompleted()
        {
            var message = State switch
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
        _combatSystem.StateChanged -= OnStateChanged;
        _combatSystem.CombatLogUpdated -= OnCombatLogUpdated;
        _combatSystem.MonsterDefeated -= OnMonsterDefeated;
        _combatSystem.ExpeditionCompleted -= OnExpeditionCompleted;
    }
}
