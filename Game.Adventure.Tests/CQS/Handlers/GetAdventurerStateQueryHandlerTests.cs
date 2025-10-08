#nullable enable

using FluentAssertions;
using Game.Adventure.Data;
using Game.Adventure.Handlers;
using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.Utils;

namespace Game.Adventure.Tests.CQS.Handlers;

public class GetAdventurerStateQueryHandlerTests : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly GetAdventurerStateQueryHandler _handler;

    public GetAdventurerStateQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _combatSystem = new CombatSystem();
        _handler = new GetAdventurerStateQueryHandler(_combatSystem);
    }

    public void Dispose()
    {
        // Clean up if needed
    }

    [Fact]
    public async Task HandleAsync_WithIdleState_ReturnsIdle()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        _combatSystem.StartExpedition(adventurer, new List<Game.Adventure.Models.CombatEntityStats>());
        _combatSystem.Update(0.1f); // Complete expedition to return to idle
        
        var query = new GetAdventurerStateQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().Be(AdventurerState.Idle);
    }

    [Fact]
    public async Task HandleAsync_WithFightingState_ReturnsFighting()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var goblin = EntityFactory.CreateGoblin();
        
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        var query = new GetAdventurerStateQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().Be(AdventurerState.Fighting);
    }

    [Fact]
    public async Task HandleAsync_AfterCombatEnds_ReturnsRegenerating()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var weakGoblin = EntityFactory.CreateGoblin();
        
        _combatSystem.StartExpedition(adventurer, new[] { weakGoblin });
        
        // Kill the goblin during combat to trigger the death event and state change
        weakGoblin.TakeDamage(weakGoblin.MaxHealth);
        
        var query = new GetAdventurerStateQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().Be(AdventurerState.Regenerating);
    }

    [Fact]
    public void Constructor_WithNullCombatSystem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetAdventurerStateQueryHandler(null!));
    }
}
