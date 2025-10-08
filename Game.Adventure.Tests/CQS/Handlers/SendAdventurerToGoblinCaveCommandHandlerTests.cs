#nullable enable

using FluentAssertions;
using Game.Adventure.Commands;
using Game.Adventure.Data;
using Game.Adventure.Handlers;
using Game.Adventure.Models;
using Game.Adventure.Systems;
using Game.Core.Utils;

namespace Game.Adventure.Tests.CQS.Handlers;

public class SendAdventurerToGoblinCaveCommandHandlerTests : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly SendAdventurerToGoblinCaveCommandHandler _handler;
    private readonly TestableLoggerBackend _loggerBackend;

    public SendAdventurerToGoblinCaveCommandHandlerTests()
    {
        _loggerBackend = new TestableLoggerBackend();
        GameLogger.SetBackend(_loggerBackend);
        _combatSystem = new CombatSystem();
        _handler = new SendAdventurerToGoblinCaveCommandHandler(_combatSystem);
    }

    public void Dispose()
    {
        // Clean up if needed
    }

    [Fact]
    public async Task HandleAsync_WhenIdle_StartsGoblinCaveExpedition()
    {
        // Arrange
        _combatSystem.State.Should().Be(AdventurerState.Idle);
        var command = new SendAdventurerToGoblinCaveCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _combatSystem.State.Should().Be(AdventurerState.Fighting);
        _combatSystem.CurrentAdventurer.Should().NotBeNull();
        _combatSystem.CurrentAdventurer!.Name.Should().Contain("Novice");
        _combatSystem.CurrentMonster.Should().NotBeNull();
        _combatSystem.HasMonstersRemaining.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_CreatesNoviceAdventurer()
    {
        // Arrange
        var command = new SendAdventurerToGoblinCaveCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var adventurer = _combatSystem.CurrentAdventurer;
        adventurer.Should().NotBeNull();
        adventurer!.Name.Should().Contain("Novice");
        
        // Verify it's a novice adventurer (lower stats)
        adventurer.MaxHealth.Should().BeLessOrEqualTo(200);
        adventurer.DamagePerSecond.Should().BeLessOrEqualTo(50);
    }

    [Fact]
    public async Task HandleAsync_CreatesThreeGoblins()
    {
        // Arrange
        var command = new SendAdventurerToGoblinCaveCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _combatSystem.HasMonstersRemaining.Should().BeTrue();
        _combatSystem.CurrentMonster.Should().NotBeNull();
        _combatSystem.CurrentMonster!.Name.Should().Contain("Goblin");
        
        // Verify there are multiple monsters by checking if more remain after killing one
        var currentMonster = _combatSystem.CurrentMonster;
        currentMonster.TakeDamage(currentMonster.MaxHealth); // Kill current monster
        _combatSystem.Update(0.1f); // Process combat to move to next monster
        
        // Should still have monsters remaining (started with 3, killed 1)
        _combatSystem.HasMonstersRemaining.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyFighting_ThrowsInvalidOperationException()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var goblin = EntityFactory.CreateGoblin();
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        
        var command = new SendAdventurerToGoblinCaveCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        exception.Message.Should().Be("Adventurer is not available for expedition");
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyFighting_LogsWarning()
    {
        // Ensure this test has the right logger backend (in case other tests changed it)
        GameLogger.SetBackend(_loggerBackend);
        
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var goblin = EntityFactory.CreateGoblin();
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        
        // Clear any existing logs from setup
        _loggerBackend.Clear();
        
        var command = new SendAdventurerToGoblinCaveCommand();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        
        var logEntries = _loggerBackend.GetLogs();
        logEntries.Should().Contain(entry => 
            entry.Level == GameLogger.LogLevel.Warning && 
            entry.Message.Contains("[Adventure] Adventurer is not available for expedition") &&
            entry.Message.Contains("Current state: Fighting"));
    }

    [Fact]
    public async Task HandleAsync_LogsExpeditionStart()
    {
        // Ensure this test has the right logger backend (in case other tests changed it)
        GameLogger.SetBackend(_loggerBackend);
        
        // Arrange
        // Clear any existing logs from setup
        _loggerBackend.Clear();
        
        var command = new SendAdventurerToGoblinCaveCommand();

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var logEntries = _loggerBackend.GetLogs();
        logEntries.Should().Contain(entry => 
            entry.Level == GameLogger.LogLevel.Info && 
            entry.Message.Contains("[Adventure] Goblin Cave expedition started") &&
            entry.Message.Contains("Adventurer:"));
    }

    [Fact]
    public void Constructor_WithNullCombatSystem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SendAdventurerToGoblinCaveCommandHandler(null!));
    }
}
