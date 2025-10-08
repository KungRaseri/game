using FluentAssertions;
using Game.Adventure.Commands;
using Game.Adventure.Data;
using Game.Adventure.Handlers;
using Game.Adventure.Models;
using Game.Adventure.Systems;
using Game.Core.Utils;

namespace Game.Adventure.Tests.CQS.Handlers;

public class LoggerDiagnosticTest : IDisposable
{
    private readonly CombatSystem _combatSystem;
    private readonly ForceAdventurerRetreatCommandHandler _handler;
    private readonly TestableLoggerBackend _loggerBackend;

    public LoggerDiagnosticTest()
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
    public void TestDirectLogging()
    {
        // Clear any existing logs
        _loggerBackend.Clear();
        
        // Direct logging test
        GameLogger.Info("Test message 1");
        GameLogger.Warning("Test message 2");
        
        var logs = _loggerBackend.GetLogs();
        
        // Print all logs for debugging
        foreach (var log in logs)
        {
            Console.WriteLine($"Level: {log.Level}, Message: {log.Message}");
        }
        
        logs.Count.Should().Be(2);
        logs[0].Message.Should().Be("Test message 1");
        logs[1].Message.Should().Be("Test message 2");
    }

    [Fact]
    public async Task TestHandlerLogging()
    {
        // Setup for handler test
        var adventurer = EntityFactory.CreateNoviceAdventurer();
        var goblin = EntityFactory.CreateGoblin();
        
        _combatSystem.StartExpedition(adventurer, new[] { goblin });
        
        // Clear logs after setup
        _loggerBackend.Clear();
        
        var command = new ForceAdventurerRetreatCommand();
        await _handler.HandleAsync(command);
        
        var logs = _loggerBackend.GetLogs();
        
        // Print all logs for debugging
        Console.WriteLine($"Found {logs.Count} log entries:");
        foreach (var log in logs)
        {
            Console.WriteLine($"Level: {log.Level}, Message: {log.Message}");
        }
        
        logs.Should().HaveCountGreaterThan(0);
    }
}
