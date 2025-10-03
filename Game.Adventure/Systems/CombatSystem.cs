using System;
using System.Collections.Generic;
using Game.Adventure.Models;

namespace Game.Adventure.Systems;

/// <summary>
/// Manages combat between adventurers and monsters with health-based auto-combat
/// </summary>
public class CombatSystem
{
    private readonly Queue<CombatEntityStats> _monsters;
    private CombatEntityStats? _currentAdventurer;
    private CombatEntityStats? _currentMonster;
    private AdventurerState _state;

    /// <summary>
    /// Accumulates fractional damage dealt by the adventurer to the monster.
    /// <para>
    /// Fractional accumulation is necessary to prevent rounding errors in scenarios where
    /// the adventurer's damage per tick is less than 1. By accumulating the fractional
    /// damage over multiple ticks, we ensure that all intended damage is eventually applied,
    /// preventing the loss of damage that would occur if values were truncated or rounded
    /// each tick. This is especially important in low DPS scenarios.
    /// </para>
    /// </summary>
    private float _accumulatedAdventurerDamage = 0f;

    /// <summary>
    /// Accumulates fractional damage dealt by the monster to the adventurer.
    /// <para>
    /// Fractional accumulation is necessary to prevent rounding errors in scenarios where
    /// the monster's damage per tick is less than 1. By accumulating the fractional
    /// damage over multiple ticks, we ensure that all intended damage is eventually applied,
    /// preventing the loss of damage that would occur if values were truncated or rounded
    /// each tick. This is especially important in low DPS scenarios.
    /// </para>
    /// </summary>
    private float _accumulatedMonsterDamage = 0f;

    public AdventurerState State
    {
        get => _state;
        private set
        {
            _state = value;
            StateChanged?.Invoke(_state);
        }
    }

    public CombatEntityStats? CurrentAdventurer => _currentAdventurer;
    public CombatEntityStats? CurrentMonster => _currentMonster;
    public bool IsInCombat => State == AdventurerState.Fighting;
    public bool HasMonstersRemaining => _monsters.Count > 0 || _currentMonster?.IsAlive == true;

    public event Action<AdventurerState>? StateChanged;
    public event Action<string>? CombatLogUpdated;
    public event Action<CombatEntityStats>? MonsterDefeated;
    public event Action? ExpeditionCompleted;

    public CombatSystem()
    {
        _monsters = new Queue<CombatEntityStats>();
        State = AdventurerState.Idle;
    }

    /// <summary>
    /// Starts an expedition with the given adventurer against a list of monsters
    /// </summary>
    public void StartExpedition(CombatEntityStats adventurer, IEnumerable<CombatEntityStats> monsters)
    {
        if (State != AdventurerState.Idle)
            throw new InvalidOperationException("Cannot start expedition while adventurer is busy");

        _currentAdventurer = adventurer ?? throw new ArgumentNullException(nameof(adventurer));

        _monsters.Clear();
        foreach (var monster in monsters)
        {
            _monsters.Enqueue(monster);
        }

        // Reset accumulated damage for new expedition
        _accumulatedAdventurerDamage = 0f;
        _accumulatedMonsterDamage = 0f;

        State = AdventurerState.Traveling;
        LogMessage($"Adventurer begins expedition with {_monsters.Count} monsters to face");

        // Start combat with first monster
        StartNextFight();
    }

    /// <summary>
    /// Updates combat state and processes damage over time
    /// </summary>
    public void Update()
    {
        Update(1.0f); // Default to 1 second for backward compatibility
    }

    /// <summary>
    /// Updates combat state and processes damage over time with fixed time step
    /// </summary>
    public void Update(float fixedDeltaTime)
    {
        switch (State)
        {
            case AdventurerState.Fighting:
                ProcessCombat(fixedDeltaTime);
                break;
            case AdventurerState.Retreating:
                ProcessRetreat(fixedDeltaTime);
                break;
            case AdventurerState.Regenerating:
                ProcessRegeneration(fixedDeltaTime);
                break;
        }
    }

    private void StartNextFight()
    {
        if (_currentAdventurer == null) return;

        if (_monsters.Count > 0)
        {
            _currentMonster = _monsters.Dequeue();
            _currentMonster.Died += OnMonsterDied;
            State = AdventurerState.Fighting;

            // Reset accumulated damage for new fight
            _accumulatedAdventurerDamage = 0f;
            _accumulatedMonsterDamage = 0f;

            LogMessage($"Combat begins against {_currentMonster.Name}!");
        }
        else
        {
            // All monsters defeated
            State = AdventurerState.Regenerating;
            LogMessage("All monsters defeated! Adventurer returns victorious!");
            ExpeditionCompleted?.Invoke();
        }
    }

    private void ProcessCombat(float deltaTime)
    {
        if (_currentAdventurer == null || _currentMonster == null)
        {
            // If we're missing entities but still in fighting state, transition appropriately
            if (State == AdventurerState.Fighting)
            {
                if (_monsters.Count > 0)
                {
                    StartNextFight();
                }
                else
                {
                    State = AdventurerState.Regenerating;
                    LogMessage("Combat ended - adventurer returns!");
                    ExpeditionCompleted?.Invoke();
                }
            }
            return;
        }

        // Check if adventurer should retreat
        if (_currentAdventurer.ShouldRetreat)
        {
            State = AdventurerState.Retreating;
            LogMessage($"Adventurer retreats at {_currentAdventurer.HealthPercentage:P0} health!");
            ExpeditionCompleted?.Invoke();
            return;
        }

        // Store local references to prevent race conditions
        var currentAdventurer = _currentAdventurer;
        var currentMonster = _currentMonster;

        // Double-check that they're still valid after storing references
        if (currentAdventurer == null || currentMonster == null)
        {
            return;
        }

        // Apply damage over time with proper fractional damage accumulation
        _accumulatedAdventurerDamage += currentAdventurer.DamagePerSecond * deltaTime;
        _accumulatedMonsterDamage += currentMonster.DamagePerSecond * deltaTime;

        // Apply accumulated damage when it reaches at least 1 point
        var adventurerDamage = (int)_accumulatedAdventurerDamage;
        var monsterDamage = (int)_accumulatedMonsterDamage;

        if (adventurerDamage > 0)
        {
            _accumulatedAdventurerDamage -= adventurerDamage;
            currentMonster.TakeDamage(adventurerDamage);
        }

        // Check if monster was alive before damage and apply counter-damage
        var monsterWasAlive = currentMonster.IsAlive;

        if (monsterWasAlive && monsterDamage > 0 && currentAdventurer.IsAlive)
        {
            _accumulatedMonsterDamage -= monsterDamage;
            currentAdventurer.TakeDamage(monsterDamage);

            // Check if adventurer died
            if (!currentAdventurer.IsAlive)
            {
                State = AdventurerState.Retreating;
                LogMessage("Adventurer has fallen! Emergency retreat!");
                ExpeditionCompleted?.Invoke();
            }
        }
    }

    private void ProcessRetreat(float deltaTime)
    {
        if (_currentAdventurer == null) return;

        // Clear current monster and remaining monsters from the expedition
        if (_currentMonster != null)
        {
            _currentMonster.Died -= OnMonsterDied;
            _currentMonster = null;
        }
        _monsters.Clear();

        // Transition to regenerating to begin healing
        State = AdventurerState.Regenerating;
        LogMessage("Adventurer reaches safety and begins recovering");
    }

    private void ProcessRegeneration(float deltaTime)
    {
        if (_currentAdventurer == null) return;

        _currentAdventurer.RegenerateHealth();

        // Check if fully healed or enough time has passed
        if (_currentAdventurer.CurrentHealth == _currentAdventurer.MaxHealth)
        {
            State = AdventurerState.Idle;
            LogMessage("Adventurer has fully recovered and is ready for another expedition");
        }
    }

    private void OnMonsterDied(CombatEntityStats monster)
    {
        LogMessage($"{monster.Name} defeated!");
        MonsterDefeated?.Invoke(monster);

        if (monster == _currentMonster)
        {
            _currentMonster.Died -= OnMonsterDied;
            _currentMonster = null;

            // Immediately start next fight to avoid state inconsistencies
            StartNextFight();
        }
    }

    private void LogMessage(string message)
    {
        CombatLogUpdated?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    /// <summary>
    /// Forces the adventurer to retreat immediately
    /// </summary>
    public void ForceRetreat()
    {
        if (State == AdventurerState.Fighting || State == AdventurerState.Traveling)
        {
            State = AdventurerState.Retreating;
            LogMessage("Forced retreat initiated!");
            ExpeditionCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Resets the combat system to idle state
    /// </summary>
    public void Reset()
    {
        _monsters.Clear();
        _currentMonster = null;
        _currentAdventurer = null;
        State = AdventurerState.Idle;
    }
}
