#nullable enable

using FluentAssertions;
using Game.Adventure.Data;
using Game.Adventure.Handlers;
using Game.Adventure.Models;
using Game.Adventure.Queries;
using Game.Adventure.Systems;
using Game.Core.Utils;

namespace Game.Adventure.Tests.CQS.Handlers;

public class GetAdventurerStatusQueryHandlerTests : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly GetAdventurerStatusQueryHandler _handler;

    public GetAdventurerStatusQueryHandlerTests()
    {
        GameLogger.SetBackend(new TestableLoggerBackend());
        _combatSystem = new CombatSystem();
        _handler = new GetAdventurerStatusQueryHandler(_combatSystem);
    }

    public void Dispose()
    {
        // Clean up if needed
    }

    [Fact]
    public async Task HandleAsync_WithNoAdventurer_ReturnsNoAdventurerMessage()
    {
        // Arrange
        var query = new GetAdventurerStatusQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().Be("No adventurer currently active");
    }

    [Fact]
    public async Task HandleAsync_WithAdventurerIdle_ReturnsHealthAndStateInfo()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        _combatSystem.StartExpedition(adventurer, new List<Game.Adventure.Models.CombatEntityStats>());
        
        // Wait for expedition to complete (no monsters)
        _combatSystem.Update(0.1f);
        
        var query = new GetAdventurerStatusQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().Contain("State: Idle");
        result.Should().NotContain("Fighting:");
    }

    [Fact]
    public async Task HandleAsync_WithAdventurerInCombat_IncludesMonsterInfo()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        adventurer.TakeDamage(20); // Reduce health to test percentage
        var goblin = EntityFactory.CreateGoblin();
        goblin.TakeDamage(10); // Reduce goblin health
        
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        var query = new GetAdventurerStatusQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().Contain("State: Fighting");
        result.Should().Contain($"Fighting: {goblin.Name}");
        result.Should().Contain($"({goblin.CurrentHealth}/{goblin.MaxHealth} HP)");
    }

    [Fact]
    public async Task HandleAsync_WithDamagedAdventurer_ShowsCorrectHealthPercentage()
    {
        // Arrange
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var initialHealth = adventurer.CurrentHealth;
        var damageAmount = (int)(initialHealth * 0.25f); // 25% damage
        adventurer.TakeDamage(damageAmount);
        
        _combatSystem.StartExpedition(adventurer, new List<Game.Adventure.Models.CombatEntityStats>());
        _combatSystem.Update(0.1f); // Complete expedition
        
        var query = new GetAdventurerStatusQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        var expectedHealth = initialHealth - damageAmount;
        result.Should().Contain($"HP: {expectedHealth}/{initialHealth}");
    }

    [Fact]
    public void Constructor_WithNullCombatSystem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetAdventurerStatusQueryHandler(null!));
    }
}
