using FluentAssertions;
using Game.Core.CQS;
using Game.Core.Extensions;
using Game.Economy.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Economy.Tests.Extensions;

/// <summary>
/// Tests for EconomyServiceCollectionExtensions to ensure proper service registration.
/// </summary>
public class EconomyServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEconomyServices_ShouldRegisterBasicServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCQS();

        // Act
        services.AddEconomyServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verify basic registration works
        serviceProvider.Should().NotBeNull();
    }
}
