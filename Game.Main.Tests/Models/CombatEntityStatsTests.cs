using System;
using Xunit;
using Game.Main.Models;

namespace Game.Main.Tests.Models
{
    public class CombatEntityStatsTests
    {
        [Fact]
        public void Constructor_SetsValues_Correctly()
        {
            // Arrange & Act
            var entity = new CombatEntityStats("Test Entity", 100, 10, 0.25f);

            // Assert
            Assert.Equal("Test Entity", entity.Name);
            Assert.Equal(100, entity.MaxHealth);
            Assert.Equal(100, entity.CurrentHealth);
            Assert.Equal(10, entity.DamagePerSecond);
            Assert.Equal(0.25f, entity.RetreatThreshold);
            Assert.Equal(1.0f, entity.HealthPercentage);
            Assert.True(entity.IsAlive);
            Assert.False(entity.ShouldRetreat);
        }

        [Fact]
        public void TakeDamage_ReducesHealth_AndTriggersEvent()
        {
            // Arrange
            var entity = new CombatEntityStats("Test", 100, 10);
            var eventTriggered = false;
            var reportedCurrent = 0;
            var reportedMax = 0;

            entity.HealthChanged += (current, max) =>
            {
                eventTriggered = true;
                reportedCurrent = current;
                reportedMax = max;
            };

            // Act
            entity.TakeDamage(30);

            // Assert
            Assert.Equal(70, entity.CurrentHealth);
            Assert.True(eventTriggered);
            Assert.Equal(70, reportedCurrent);
            Assert.Equal(100, reportedMax);
        }

        [Fact]
        public void ShouldRetreat_RespectsCustomThreshold()
        {
            // Arrange
            var entity = new CombatEntityStats("Test", 100, 10, 0.4f); // 40% retreat threshold

            // Act
            entity.TakeDamage(50); // 50% health remaining - should not retreat
            var shouldNotRetreat = entity.ShouldRetreat;
            
            entity.TakeDamage(15); // 35% health remaining - should retreat
            var shouldRetreat = entity.ShouldRetreat;

            // Assert
            Assert.False(shouldNotRetreat);
            Assert.True(shouldRetreat);
        }

        [Fact]
        public void RegenerateHealth_IncreasesHealthByAmount()
        {
            // Arrange
            var entity = new CombatEntityStats("Test", 100, 10);
            entity.TakeDamage(50); // 50 health remaining

            // Act
            entity.RegenerateHealth(5); // Custom regen amount

            // Assert
            Assert.Equal(55, entity.CurrentHealth);
        }

        [Fact]
        public void RegenerateHealth_DoesNotExceedMaxHealth()
        {
            // Arrange
            var entity = new CombatEntityStats("Test", 100, 10);
            entity.TakeDamage(1); // 99 health remaining

            // Act
            entity.RegenerateHealth(5); // Should cap at max health

            // Assert
            Assert.Equal(100, entity.CurrentHealth);
        }

        [Fact]
        public void TakeDamage_KillsEntity_TriggersDeathEvent()
        {
            // Arrange
            var entity = new CombatEntityStats("Test", 10, 5);
            var diedTriggered = false;
            CombatEntityStats? reportedEntity = null;

            entity.Died += (e) =>
            {
                diedTriggered = true;
                reportedEntity = e;
            };

            // Act
            entity.TakeDamage(10);

            // Assert
            Assert.Equal(0, entity.CurrentHealth);
            Assert.False(entity.IsAlive);
            Assert.True(diedTriggered);
            Assert.Same(entity, reportedEntity);
        }

        [Theory]
        [InlineData(0f, 50, false)] // No retreat threshold
        [InlineData(0.25f, 30, false)] // 30% health, 25% threshold - should not retreat
        [InlineData(0.25f, 25, false)] // Exactly at threshold - should not retreat
        [InlineData(0.25f, 24, true)]  // Below threshold - should retreat
        [InlineData(0.5f, 49, true)]   // 49% health, 50% threshold - should retreat
        public void ShouldRetreat_CalculatesCorrectly(float threshold, int currentHealth, bool expectedRetreat)
        {
            // Arrange
            var entity = new CombatEntityStats("Test", 100, 10, threshold);
            
            // Act
            entity.TakeDamage(100 - currentHealth);

            // Assert
            Assert.Equal(expectedRetreat, entity.ShouldRetreat);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var entity = new CombatEntityStats("Goblin Warrior", 50, 8);
            entity.TakeDamage(10);

            // Act
            var result = entity.ToString();

            // Assert
            Assert.Equal("Goblin Warrior (HP: 40/50, DPS: 8)", result);
        }

        [Fact]
        public void Constructor_WithNullName_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CombatEntityStats(null!, 100, 10));
        }

        [Theory]
        [InlineData(0, 1)] // Health gets clamped to minimum 1
        [InlineData(-10, 1)]
        public void Constructor_WithInvalidHealth_ClampsToMinimum(int inputHealth, int expectedHealth)
        {
            // Act
            var entity = new CombatEntityStats("Test", inputHealth, 10);

            // Assert
            Assert.Equal(expectedHealth, entity.MaxHealth);
        }

        [Theory]
        [InlineData(0, 1)] // Damage gets clamped to minimum 1
        [InlineData(-5, 1)]
        public void Constructor_WithInvalidDamage_ClampsToMinimum(int inputDamage, int expectedDamage)
        {
            // Act
            var entity = new CombatEntityStats("Test", 100, inputDamage);

            // Assert
            Assert.Equal(expectedDamage, entity.DamagePerSecond);
        }
    }
}
