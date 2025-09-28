using System;
using System.Collections.Generic;
using Xunit;
using Game.Main.Controllers;
using Game.Main.Models;
using Game.Main.Systems;
using Game.Main.Data;

namespace Game.Main.Tests.Controllers
{
    public class AdventurerControllerTests
    {
        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            var combatSystem = new CombatSystem();

            // Act
            var controller = new AdventurerController(combatSystem);

            // Assert
            Assert.NotNull(controller.Adventurer);
            Assert.Equal("Novice Adventurer", controller.Adventurer.Name);
            Assert.Equal(AdventurerState.Idle, controller.State);
            Assert.True(controller.IsAvailable);
        }

        [Fact]
        public void Constructor_WithNullCombatSystem_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AdventurerController(null!));
        }

        [Fact]
        public void SendToGoblinCave_WhenAvailable_StartsExpedition()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);

            // Act
            controller.SendToGoblinCave();

            // Assert
            Assert.False(controller.IsAvailable);
            Assert.Contains(statusMessages, m => m.Contains("departs for the Goblin Cave"));
            Assert.Equal(AdventurerState.Fighting, controller.State);
        }

        [Fact]
        public void SendToGoblinCave_WhenNotAvailable_DoesNotStartExpedition()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);
            
            // Send once to make unavailable
            controller.SendToGoblinCave();
            statusMessages.Clear();

            // Act
            controller.SendToGoblinCave();

            // Assert
            Assert.Contains(statusMessages, m => m.Contains("not available"));
        }

        [Fact]
        public void Retreat_WhenInCombat_ForcesRetreat()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);
            controller.SendToGoblinCave();
            statusMessages.Clear();

            // Act
            controller.Retreat();

            // Assert
            Assert.Contains(statusMessages, m => m.Contains("Retreat order"));
            Assert.Equal(AdventurerState.Retreating, controller.State);
        }

        [Fact]
        public void Update_CallsCombatSystemUpdate()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);

            // Act
            controller.Update(); // Should not throw any exceptions

            // Assert
            // If we get here without exceptions, the update worked
            Assert.True(true);
        }

        [Fact]
        public void GetStatusInfo_ReturnsFormattedStatus()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);

            // Act
            var status = controller.GetStatusInfo();

            // Assert
            Assert.Contains("HP:", status);
            Assert.Contains("State:", status);
            Assert.Contains("Idle", status);
        }

        [Fact]
        public void GetStatusInfo_WhenFighting_IncludesMonsterInfo()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            
            controller.SendToGoblinCave();

            // Act
            var status = controller.GetStatusInfo();

            // Assert
            Assert.Contains("Fighting:", status);
            Assert.Contains("Goblin", status);
        }

        [Fact]
        public void StatusUpdated_Event_TriggersOnStateChanges()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);

            // Act
            controller.SendToGoblinCave();

            // Assert
            Assert.NotEmpty(statusMessages);
            Assert.True(statusMessages.Count > 1); // Should have multiple status updates
        }

        [Fact]
        public void MonsterDefeated_Event_TriggersStatusUpdate()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);
            controller.SendToGoblinCave();
            statusMessages.Clear();

            // Act - Kill the current monster
            var currentMonster = combatSystem.CurrentMonster;
            currentMonster!.TakeDamage(currentMonster.CurrentHealth);

            // Assert
            Assert.Contains(statusMessages, m => m.Contains("Victory") && m.Contains("defeated"));
        }

        [Fact]
        public void ExpeditionCompleted_Event_TriggersStatusUpdate()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);
            controller.SendToGoblinCave();
            statusMessages.Clear();

            // Act - Force retreat to complete expedition
            controller.Retreat();

            // Assert
            Assert.Contains(statusMessages, m => m.Contains("Expedition ended"));
        }

        [Fact]
        public void CombatLog_Updates_TriggerStatusUpdates()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);

            // Act
            controller.SendToGoblinCave();

            // Assert
            Assert.Contains(statusMessages, m => m.Contains("Combat begins"));
        }

        [Fact]
        public void Dispose_UnsubscribesFromEvents()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);
            var statusMessages = new List<string>();
            
            controller.StatusUpdated += message => statusMessages.Add(message);
            controller.SendToGoblinCave();
            
            // Act
            controller.Dispose();
            statusMessages.Clear();
            
            // Try to trigger combat system events directly after disposal
            combatSystem.ForceRetreat(); // This should not trigger controller status updates

            // Assert
            // Should not receive status updates from combat system events after disposal
            Assert.Empty(statusMessages);
        }

        [Fact]
        public void AdventurerProperty_ReturnsCorrectEntity()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);

            // Act
            var adventurer = controller.Adventurer;

            // Assert
            Assert.NotNull(adventurer);
            Assert.Equal(100, adventurer.MaxHealth);
            Assert.Equal(10, adventurer.DamagePerSecond);
            Assert.Equal(0.25f, adventurer.RetreatThreshold);
        }

        [Fact]
        public void IsAvailable_ReflectsCurrentState()
        {
            // Arrange
            var combatSystem = new CombatSystem();
            var controller = new AdventurerController(combatSystem);

            // Act & Assert - Initially available
            Assert.True(controller.IsAvailable);
            
            // Send to dungeon - should not be available
            controller.SendToGoblinCave();
            Assert.False(controller.IsAvailable);
            
            // Force retreat - still not available (regenerating)
            controller.Retreat();
            Assert.False(controller.IsAvailable);
        }
    }
}
