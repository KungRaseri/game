#nullable enable

using FluentAssertions;
using Game.Adventure.Commands;
using Game.Adventure.Data;
using Game.Adventure.Handlers;
using Game.Adventure.Models;
using Game.Adventure.Systems;
using Game.Core.Utils;

namespace Game.Adventure.Tests.CQS.Handlers;

public class ForceAdventurerRetreatCommandHandlerTests : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly ForceAdventurerRetreatCommandHandler _handler;
    private readonly TestableLoggerBackend _loggerBackend;

    public ForceAdventurerRetreatCommandHandlerTests()
    {
        _loggerBackend = new TestableLoggerBackend();
        GameLogger.SetBackend(_loggerBackend);
        _combatSystem = new CombatSystem();
        _handler = new ForceAdventurerRetreatCommandHandler(_combatSystem);
    }

    public void Dispose()
    {
        // Clean up if needed
    }

    [Fact]
    public async Task HandleAsync_WhenAdventurerInCombat_ForcesRetreat()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var goblin = EntityFactory.CreateGoblin();
        
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        
        // Verify we're in combat
        _combatSystem.State.Should().Be(AdventurerState.Fighting);
        
        var command = new ForceAdventurerRetreatCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _combatSystem.State.Should().Be(AdventurerState.Retreating);
    }

    [Fact]
    public async Task HandleAsync_WhenAdventurerIdle_RemainsIdle()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        _combatSystem.StartExpedition(adventurer, new List<Game.Adventure.Models.CombatEntityStats>());
        _combatSystem.Update(0.1f); // Complete expedition to be idle
        
        // Verify we're idle
        _combatSystem.State.Should().Be(AdventurerState.Idle);
        
        var command = new ForceAdventurerRetreatCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _combatSystem.State.Should().Be(AdventurerState.Idle);
    }

    [Fact]
    public async Task HandleAsync_LogsStateChange()
    {
        // Ensure this test has the right logger backend (in case other tests changed it)
        GameLogger.SetBackend(_loggerBackend);
        
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var goblin = EntityFactory.CreateGoblin();
        
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        
        // Clear any existing logs from setup
        _loggerBackend.Clear();
        
        var command = new ForceAdventurerRetreatCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var logEntries = _loggerBackend.GetLogs();
        logEntries.Should().Contain(entry => 
            entry.Level == GameLogger.LogLevel.Info && 
            entry.Message.Contains("[Adventure] Forced retreat") &&
            entry.Message.Contains("State changed from Fighting to Retreating"));
    }

    [Fact]
    public void Constructor_WithNullCombatSystem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ForceAdventurerRetreatCommandHandler(null!));
    }
}
