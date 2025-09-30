using System;
using System.Collections.Generic;
using Game.Main.Models;

namespace Game.Main.Systems;

/// <summary>
/// Manages combat between adventurers and monsters with health-based auto-combat
/// </summary>
public class CombatSystem
    {
        private readonly Queue<CombatEntityStats> _monsters;
        private CombatEntityStats? _currentAdventurer;
        private CombatEntityStats? _currentMonster;
        private AdventurerState _state;
        private DateTime _lastUpdateTime;

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
            _lastUpdateTime = DateTime.Now;
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
            var currentTime = DateTime.Now;
            var deltaTime = (float)(currentTime - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = currentTime;

            switch (State)
            {
                case AdventurerState.Fighting:
                    ProcessCombat(deltaTime);
                    break;
                case AdventurerState.Regenerating:
                    ProcessRegeneration(deltaTime);
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
            if (_currentAdventurer == null || _currentMonster == null) return;

            // Check if adventurer should retreat
            if (_currentAdventurer.ShouldRetreat)
            {
                State = AdventurerState.Retreating;
                LogMessage($"Adventurer retreats at {_currentAdventurer.HealthPercentage:P0} health!");
                ExpeditionCompleted?.Invoke();
                return;
            }

            // Apply damage over time
            var adventurerDamage = (int)(_currentAdventurer.DamagePerSecond * deltaTime);
            var monsterDamage = (int)(_currentMonster.DamagePerSecond * deltaTime);

            _currentMonster.TakeDamage(adventurerDamage);
            
            if (_currentMonster.IsAlive)
            {
                _currentAdventurer.TakeDamage(monsterDamage);
                
                // Check if adventurer died
                if (!_currentAdventurer.IsAlive)
                {
                    State = AdventurerState.Retreating;
                    LogMessage("Adventurer has fallen! Emergency retreat!");
                    ExpeditionCompleted?.Invoke();
                }
            }
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
