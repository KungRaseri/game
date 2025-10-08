#nullable enable

using FluentAssertions;
using Game.Adventure.Data;
using Game.Adventure.Handlers;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.Utils;

namespace Game.Adventure.Tests.CQS.Handlers;

public class GetCurrentAdventurerQueryHandlerTests : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly GetCurrentAdventurerQueryHandler _handler;

    public GetCurrentAdventurerQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _combatSystem = new CombatSystem();
        _handler = new GetCurrentAdventurerQueryHandler(_combatSystem);
    }

    public void Dispose()
    {
        // Clean up if needed
    }

    [Fact]
    public async Task HandleAsync_WithNoAdventurer_ReturnsNull()
    {
        // Arrange
        var query = new GetCurrentAdventurerQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithAdventurer_ReturnsAdventurerStats()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        _combatSystem.StartExpedition(adventurer, new List<Game.Adventure.Models.CombatEntityStats>());
        
        var query = new GetCurrentAdventurerQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("Novice");
        result.MaxHealth.Should().BeGreaterThan(0);
        result.DamagePerSecond.Should().BeGreaterThan(0);
        result.CurrentHealth.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HandleAsync_WithDamagedAdventurer_ReturnsCurrentState()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var initialHealth = adventurer.CurrentHealth;
        adventurer.TakeDamage(30);
        _combatSystem.StartExpedition(adventurer, new List<Game.Adventure.Models.CombatEntityStats>());
        
        var query = new GetCurrentAdventurerQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentHealth.Should().Be(initialHealth - 30);
        result.MaxHealth.Should().Be(initialHealth);
    }

    [Fact]
    public void Constructor_WithNullCombatSystem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetCurrentAdventurerQueryHandler(null!));
    }
}
