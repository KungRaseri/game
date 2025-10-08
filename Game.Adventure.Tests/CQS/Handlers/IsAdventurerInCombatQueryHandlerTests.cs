#nullable enable

using FluentAssertions;
using Game.Adventure.Data;
using Game.Adventure.Handlers;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.Utils;

namespace Game.Adventure.Tests.CQS.Handlers;

public class IsAdventurerInCombatQueryHandlerTests : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly IsAdventurerInCombatQueryHandler _handler;

    public IsAdventurerInCombatQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _combatSystem = new CombatSystem();
        _handler = new IsAdventurerInCombatQueryHandler(_combatSystem);
    }

    public void Dispose()
    {
        // Clean up if needed
    }

    [Fact]
    public async Task HandleAsync_WhenAdventurerIdle_ReturnsFalse()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var query = new IsAdventurerInCombatQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenAdventurerInCombat_ReturnsTrue()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var goblin = EntityFactory.CreateGoblin();
        
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        
        var query = new IsAdventurerInCombatQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenNoAdventurer_ReturnsFalse()
    {
        // Arrange
        var query = new IsAdventurerInCombatQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_AfterCombatEnds_ReturnsFalse()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var weakGoblin = EntityFactory.CreateGoblin();
        
        _combatSystem.StartExpedition(adventurer, new[] { weakGoblin });
        
        // Kill the goblin during combat, which should trigger monster death and end combat
        weakGoblin.TakeDamage(weakGoblin.MaxHealth);
        
        var query = new IsAdventurerInCombatQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithNullCombatSystem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new IsAdventurerInCombatQueryHandler(null!));
    }
}
