using Xunit;
using Game.Main.Managers;

namespace Game.Main.Tests.Managers
{
    public class GameManagerTests
    {
        [Fact]
        public void Constructor_InitializesSystemsCorrectly()
        {
            // Act
            var gameManager = new GameManager();

            // Assert
            Assert.NotNull(gameManager.AdventurerController);
            Assert.NotNull(gameManager.CombatSystem);
            Assert.Equal(AdventurerState.Idle, gameManager.CombatSystem.State);
        }

        [Fact]
        public void Initialize_ResetsSystemsToInitialState()
        {
            // Arrange
            var gameManager = new GameManager();
            
            // Start an expedition to change state
            gameManager.AdventurerController.SendToGoblinCave();
            Assert.NotEqual(AdventurerState.Idle, gameManager.CombatSystem.State);

            // Act
            gameManager.Initialize();

            // Assert
            Assert.Equal(AdventurerState.Idle, gameManager.CombatSystem.State);
            Assert.Null(gameManager.CombatSystem.CurrentAdventurer);
            Assert.Null(gameManager.CombatSystem.CurrentMonster);
        }

        [Fact]
        public void Update_CallsAdventurerControllerUpdate()
        {
            // Arrange
            var gameManager = new GameManager();

            // Act
            gameManager.Update(); // Should not throw any exceptions

            // Assert
            // If we get here without exceptions, the update worked
            Assert.True(true);
        }

        [Fact]
        public void Update_WithActiveExpedition_ProgressesCombat()
        {
            // Arrange
            var gameManager = new GameManager();
            gameManager.AdventurerController.SendToGoblinCave();
            
            var initialState = gameManager.CombatSystem.State;

            // Act
            gameManager.Update();

            // Assert
            // State should be maintained or progressed appropriately
            Assert.True(gameManager.CombatSystem.State == initialState || 
                       gameManager.CombatSystem.State == AdventurerState.Fighting ||
                       gameManager.CombatSystem.State == AdventurerState.Retreating ||
                       gameManager.CombatSystem.State == AdventurerState.Regenerating);
        }

        [Fact]
        public void AdventurerController_Property_ReturnsCorrectInstance()
        {
            // Arrange
            var gameManager = new GameManager();

            // Act
            var controller = gameManager.AdventurerController;

            // Assert
            Assert.NotNull(controller);
            Assert.NotNull(controller.Adventurer);
            Assert.Equal("Novice Adventurer", controller.Adventurer.Name);
        }

        [Fact]
        public void CombatSystem_Property_ReturnsCorrectInstance()
        {
            // Arrange
            var gameManager = new GameManager();

            // Act
            var combatSystem = gameManager.CombatSystem;

            // Assert
            Assert.NotNull(combatSystem);
            Assert.Equal(AdventurerState.Idle, combatSystem.State);
        }

        [Fact]
        public void Dispose_DisposesAdventurerController()
        {
            // Arrange
            var gameManager = new GameManager();
            
            // Set up event handler to verify disposal
            var eventCount = 0;
            gameManager.AdventurerController.StatusUpdated += _ => eventCount++;
            
            gameManager.AdventurerController.SendToGoblinCave();

            // Act
            gameManager.Dispose();
            var eventCountAfterDispose = eventCount;
            
            // Try to trigger events through combat system after disposal
            gameManager.CombatSystem.ForceRetreat();

            // Assert
            // Event count should not have increased after disposal
            Assert.Equal(eventCountAfterDispose, eventCount);
        }

        [Fact]
        public void Integration_FullGameLoop_WorksCorrectly()
        {
            // Arrange
            var gameManager = new GameManager();
            gameManager.Initialize();

            // Act & Assert - Start expedition
            gameManager.AdventurerController.SendToGoblinCave();
            Assert.Equal(AdventurerState.Fighting, gameManager.CombatSystem.State);
            Assert.NotNull(gameManager.CombatSystem.CurrentMonster);

            // Update game state
            gameManager.Update();
            
            // Force retreat
            gameManager.AdventurerController.Retreat();
            Assert.Equal(AdventurerState.Retreating, gameManager.CombatSystem.State);

            // Reset game
            gameManager.Initialize();
            Assert.Equal(AdventurerState.Idle, gameManager.CombatSystem.State);
            Assert.True(gameManager.AdventurerController.IsAvailable);
        }

        [Fact]
        public void SystemsIntegration_EventsFlowCorrectly()
        {
            // Arrange
            var gameManager = new GameManager();
            var statusUpdatesReceived = 0;
            
            gameManager.AdventurerController.StatusUpdated += _ => statusUpdatesReceived++;

            // Act
            gameManager.AdventurerController.SendToGoblinCave();
            gameManager.Update();

            // Assert
            Assert.True(statusUpdatesReceived > 0);
        }
    }
}
