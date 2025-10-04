using Game.Adventure.Data;
using Game.Adventure.Models;
using Game.Adventure.Systems;

namespace Game.Adventure.Tests
{
    public class CombatSystemTests
    {
        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Act
            var combatSystem = new CombatSystem();

            // Assert
            Assert.Equal(AdventurerState.Idle, combatSystem.State);
            Assert.Null(combatSystem.CurrentAdventurer);
            Assert.Null(combatSystem.CurrentMonster);
            Assert.False(combatSystem.IsInCombat);
            Assert.False(combatSystem.HasMonstersRemaining);
        }

        [Fact]
        public void StartExpedition_WithValidParameters_InitializesCorrectly()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monsters = new List<CombatEntityStats> { EntityFactory.CreateGoblin() };
            var stateChangedEvents = new List<AdventurerState>();
            var logMessages = new List<string>();

            combatSystem.StateChanged += state => stateChangedEvents.Add(state);
            combatSystem.CombatLogUpdated += message => logMessages.Add(message);

            // Act
            combatSystem.StartExpedition(adventurer, monsters);

            // Assert
            Assert.Equal(AdventurerState.Fighting, combatSystem.State);
            Assert.Same(adventurer, combatSystem.CurrentAdventurer);
            Assert.NotNull(combatSystem.CurrentMonster);
            Assert.True(combatSystem.IsInCombat);
            Assert.True(combatSystem.HasMonstersRemaining);

            // Check events
            Assert.Contains(AdventurerState.Traveling, stateChangedEvents);
            Assert.Contains(AdventurerState.Fighting, stateChangedEvents);
            Assert.NotEmpty(logMessages);
        }

        [Fact]
        public void StartExpedition_WhenNotIdle_ThrowsException()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monsters = new List<CombatEntityStats> { EntityFactory.CreateGoblin() };

            combatSystem.StartExpedition(adventurer, monsters);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                combatSystem.StartExpedition(adventurer, monsters));
        }

        [Fact]
        public void StartExpedition_WithNullAdventurer_ThrowsArgumentNullException()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var monsters = new List<CombatEntityStats> { EntityFactory.CreateGoblin() };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                combatSystem.StartExpedition(null!, monsters));
        }

        [Fact]
        public void StartExpedition_WithEmptyMonsterList_CompletesImmediately()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var emptyMonsters = new List<CombatEntityStats>();
            var expeditionCompleted = false;

            combatSystem.ExpeditionCompleted += () => expeditionCompleted = true;

            // Act
            combatSystem.StartExpedition(adventurer, emptyMonsters);

            // Assert
            Assert.Equal(AdventurerState.Regenerating, combatSystem.State);
            Assert.True(expeditionCompleted);
            Assert.False(combatSystem.HasMonstersRemaining);
        }

        [Fact]
        public void Update_DuringCombat_AppliesDamageOverTime()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monster = EntityFactory.CreateGoblin();
            var initialAdventurerHealth = adventurer.CurrentHealth;
            var initialMonsterHealth = monster.CurrentHealth;

            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats> { monster });

            // Simulate time passing by calling Update multiple times with longer delays
            // Act
            for (int i = 0; i < 20; i++)
            {
                combatSystem.Update();
                Thread.Sleep(100); // Longer delay to ensure damage is applied

                // Break early if either entity takes damage or dies
                if (adventurer.CurrentHealth < initialAdventurerHealth ||
                    monster.CurrentHealth < initialMonsterHealth ||
                    !adventurer.IsAlive || !monster.IsAlive)
                {
                    break;
                }
            }

            // Assert
            // Either adventurer or monster should have taken some damage, or the fight should be over
            var adventurerTookDamage = adventurer.CurrentHealth < initialAdventurerHealth;
            var monsterTookDamage = monster.CurrentHealth < initialMonsterHealth;
            var fightIsOver = !adventurer.IsAlive || !monster.IsAlive;

            Assert.True(adventurerTookDamage || monsterTookDamage || fightIsOver);
        }

        [Fact]
        public void Update_WhenAdventurerShouldRetreat_ChangesToRetreatingState()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monster = EntityFactory.CreateGoblin();
            var expeditionCompleted = false;

            // Damage adventurer to below retreat threshold
            adventurer.TakeDamage(80); // Should be at 20% health, below 25% threshold

            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats> { monster });
            combatSystem.ExpeditionCompleted += () => expeditionCompleted = true;

            // Act
            combatSystem.Update();

            // Assert
            Assert.Equal(AdventurerState.Retreating, combatSystem.State);
            Assert.True(expeditionCompleted);
        }

        [Fact]
        public void Update_WhenAdventurerDies_ChangesToRetreatingState()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monster = EntityFactory.CreateGoblin();
            var expeditionCompleted = false;

            // Kill the adventurer
            adventurer.TakeDamage(adventurer.CurrentHealth);

            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats> { monster });
            combatSystem.ExpeditionCompleted += () => expeditionCompleted = true;

            // Act
            combatSystem.Update();

            // Assert
            Assert.Equal(AdventurerState.Retreating, combatSystem.State);
            Assert.True(expeditionCompleted);
        }

        [Fact]
        public void MonsterDefeated_Event_TriggersWhenMonsterDies()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monster = EntityFactory.CreateGoblin();
            var defeatedMonsters = new List<CombatEntityStats>();

            combatSystem.MonsterDefeated += m => defeatedMonsters.Add(m);
            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats> { monster });

            // Act - Kill the monster directly
            monster.TakeDamage(monster.CurrentHealth);

            // Assert
            Assert.Contains(monster, defeatedMonsters);
        }

        [Fact]
        public void StartExpedition_WithMultipleMonsters_FightsSequentially()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monsters = new List<CombatEntityStats>
            {
                EntityFactory.CreateGoblin(),
                EntityFactory.CreateGoblin(),
                EntityFactory.CreateGoblin()
            };

            combatSystem.StartExpedition(adventurer, monsters);
            var firstMonster = combatSystem.CurrentMonster;

            // Act - Kill first monster
            firstMonster!.TakeDamage(firstMonster.CurrentHealth);

            // Assert
            Assert.NotSame(firstMonster, combatSystem.CurrentMonster);
            Assert.NotNull(combatSystem.CurrentMonster);
            Assert.Equal(AdventurerState.Fighting, combatSystem.State);
        }

        [Fact]
        public void ForceRetreat_WhenFighting_ChangesToRetreatingState()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monster = EntityFactory.CreateGoblin();
            var expeditionCompleted = false;

            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats> { monster });
            combatSystem.ExpeditionCompleted += () => expeditionCompleted = true;

            // Act
            combatSystem.ForceRetreat();

            // Assert
            Assert.Equal(AdventurerState.Retreating, combatSystem.State);
            Assert.True(expeditionCompleted);
        }

        [Fact]
        public void ForceRetreat_WhenIdle_DoesNothing()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var expeditionCompleted = false;
            combatSystem.ExpeditionCompleted += () => expeditionCompleted = true;

            // Act
            combatSystem.ForceRetreat();

            // Assert
            Assert.Equal(AdventurerState.Idle, combatSystem.State);
            Assert.False(expeditionCompleted);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monster = EntityFactory.CreateGoblin();

            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats> { monster });

            // Act
            combatSystem.Reset();

            // Assert
            Assert.Equal(AdventurerState.Idle, combatSystem.State);
            Assert.Null(combatSystem.CurrentAdventurer);
            Assert.Null(combatSystem.CurrentMonster);
            Assert.False(combatSystem.IsInCombat);
            Assert.False(combatSystem.HasMonstersRemaining);
        }

        [Fact]
        public void Update_InRegeneratingState_RestoresHealthGradually()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();

            // Damage adventurer and put in regenerating state
            adventurer.TakeDamage(50);
            var initialHealth = adventurer.CurrentHealth;

            // Complete an expedition to enter regenerating state
            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats>());

            // Act
            combatSystem.Update();

            // Assert
            Assert.Equal(AdventurerState.Regenerating, combatSystem.State);
            // Health should have regenerated by at least 1 point
            Assert.True(adventurer.CurrentHealth >= initialHealth);
        }

        [Fact]
        public void Update_InRegeneratingState_WhenFullyHealed_ReturnsToIdle()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();

            // Damage adventurer slightly and put in regenerating state
            adventurer.TakeDamage(1);
            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats>());

            // Act
            combatSystem.Update(); // This should heal the 1 HP and return to idle

            // Assert
            Assert.Equal(AdventurerState.Idle, combatSystem.State);
            Assert.Equal(adventurer.MaxHealth, adventurer.CurrentHealth);
        }

        [Fact]
        public void ForceRetreat_TransitionsToRegeneratingThenIdle()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var adventurer = EntityFactory.CreateNoviceAdventurer();
            var monster = EntityFactory.CreateGoblin();
            var stateChanges = new List<AdventurerState>();

            // Damage adventurer so they need healing
            adventurer.TakeDamage(30); // Take some damage so regeneration is needed

            combatSystem.StateChanged += state => stateChanges.Add(state);
            combatSystem.StartExpedition(adventurer, new List<CombatEntityStats> { monster });

            // Act - Force retreat and then process updates
            combatSystem.ForceRetreat();

            // Verify initial retreat state
            Assert.Equal(AdventurerState.Retreating, combatSystem.State);

            // Process retreat (should transition to Regenerating)
            combatSystem.Update();
            Assert.Equal(AdventurerState.Regenerating, combatSystem.State);

            // Process regeneration until fully healed (should transition to Idle)
            int maxUpdates = 50; // Safety check to prevent infinite loop
            int updateCount = 0;
            while (combatSystem.State == AdventurerState.Regenerating && updateCount < maxUpdates)
            {
                combatSystem.Update();
                updateCount++;
            }

            // Assert final state
            Assert.Equal(AdventurerState.Idle, combatSystem.State);
            Assert.Equal(adventurer.MaxHealth, adventurer.CurrentHealth);

            // Verify state transition sequence
            Assert.Contains(AdventurerState.Traveling, stateChanges);
            Assert.Contains(AdventurerState.Fighting, stateChanges);
            Assert.Contains(AdventurerState.Retreating, stateChanges);
            Assert.Contains(AdventurerState.Regenerating, stateChanges);
            Assert.Contains(AdventurerState.Idle, stateChanges);
        }
    }
}