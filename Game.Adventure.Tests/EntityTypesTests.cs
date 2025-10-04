using Game.Adventure.Data;

namespace Game.Adventure.Tests
{
    public class EntityTypesTests
    {
        [Fact]
        public void NoviceAdventurer_HasCorrectConfiguration()
        {
            // Act
            var config = EntityTypes.NoviceAdventurer;

            // Assert
            Assert.Equal("Novice Adventurer", config.Name);
            Assert.Equal(100, config.BaseHealth);
            Assert.Equal(10, config.BaseDamage);
            Assert.Equal(0.25f, config.RetreatThreshold);
            Assert.Equal(1, config.HealthRegenPerSecond);
        }

        [Fact]
        public void ExperiencedAdventurer_HasCorrectConfiguration()
        {
            // Act
            var config = EntityTypes.ExperiencedAdventurer;

            // Assert
            Assert.Equal("Experienced Adventurer", config.Name);
            Assert.Equal(150, config.BaseHealth);
            Assert.Equal(15, config.BaseDamage);
            Assert.Equal(0.20f, config.RetreatThreshold);
            Assert.Equal(2, config.HealthRegenPerSecond);
        }

        [Fact]
        public void Goblin_HasCorrectConfiguration()
        {
            // Act
            var config = EntityTypes.Goblin;

            // Assert
            Assert.Equal("Goblin", config.Name);
            Assert.Equal(20, config.BaseHealth);
            Assert.Equal(5, config.BaseDamage);
            Assert.Equal(0f, config.RetreatThreshold);
            Assert.Equal(0, config.HealthRegenPerSecond);
        }

        [Fact]
        public void Orc_HasCorrectConfiguration()
        {
            // Act
            var config = EntityTypes.Orc;

            // Assert
            Assert.Equal("Orc", config.Name);
            Assert.Equal(40, config.BaseHealth);
            Assert.Equal(8, config.BaseDamage);
            Assert.Equal(0f, config.RetreatThreshold);
            Assert.Equal(0, config.HealthRegenPerSecond);
        }

        [Fact]
        public void Troll_HasCorrectConfiguration()
        {
            // Act
            var config = EntityTypes.Troll;

            // Assert
            Assert.Equal("Troll", config.Name);
            Assert.Equal(80, config.BaseHealth);
            Assert.Equal(12, config.BaseDamage);
            Assert.Equal(0f, config.RetreatThreshold);
            Assert.Equal(0, config.HealthRegenPerSecond);
        }

        [Fact]
        public void AllMonsterTypes_HaveZeroRetreatThreshold()
        {
            // Arrange
            var monsterConfigs = new[]
            {
                EntityTypes.Goblin,
                EntityTypes.Orc,
                EntityTypes.Troll
            };

            // Act & Assert
            foreach (var config in monsterConfigs)
            {
                Assert.Equal(0f, config.RetreatThreshold);
            }
        }

        [Fact]
        public void AllAdventurerTypes_HaveNonZeroRetreatThreshold()
        {
            // Arrange
            var adventurerConfigs = new[]
            {
                EntityTypes.NoviceAdventurer,
                EntityTypes.ExperiencedAdventurer
            };

            // Act & Assert
            foreach (var config in adventurerConfigs)
            {
                Assert.True(config.RetreatThreshold > 0f);
            }
        }

        [Fact]
        public void AllAdventurerTypes_HaveHealthRegeneration()
        {
            // Arrange
            var adventurerConfigs = new[]
            {
                EntityTypes.NoviceAdventurer,
                EntityTypes.ExperiencedAdventurer
            };

            // Act & Assert
            foreach (var config in adventurerConfigs)
            {
                Assert.True(config.HealthRegenPerSecond > 0);
            }
        }

        [Fact]
        public void AllEntityTypes_HaveValidBaseStats()
        {
            // Arrange
            var allConfigs = new[]
            {
                EntityTypes.NoviceAdventurer,
                EntityTypes.ExperiencedAdventurer,
                EntityTypes.Goblin,
                EntityTypes.Orc,
                EntityTypes.Troll
            };

            // Act & Assert
            foreach (var config in allConfigs)
            {
                Assert.False(string.IsNullOrEmpty(config.Name));
                Assert.True(config.BaseHealth > 0);
                Assert.True(config.BaseDamage > 0);
                Assert.True(config.RetreatThreshold >= 0f);
                Assert.True(config.RetreatThreshold <= 1f);
                Assert.True(config.HealthRegenPerSecond >= 0);
            }
        }

        [Fact]
        public void EntityTypeConfig_ConstructorWithRequiredParameters_InitializesCorrectly()
        {
            // Act
            var config = new EntityTypeConfig(
                Name: "Test Entity",
                BaseHealth: 50,
                BaseDamage: 10
            );

            // Assert
            Assert.Equal("Test Entity", config.Name);
            Assert.Equal(50, config.BaseHealth);
            Assert.Equal(10, config.BaseDamage);
            Assert.Equal(0f, config.RetreatThreshold); // Default value
            Assert.Equal(0, config.HealthRegenPerSecond); // Default value
        }

        [Fact]
        public void EntityTypeConfig_ParameterizedConstructor_SetsValuesCorrectly()
        {
            // Act
            var config = new EntityTypeConfig("Test Entity", 50, 7, 0.3f, 2);

            // Assert
            Assert.Equal("Test Entity", config.Name);
            Assert.Equal(50, config.BaseHealth);
            Assert.Equal(7, config.BaseDamage);
            Assert.Equal(0.3f, config.RetreatThreshold);
            Assert.Equal(2, config.HealthRegenPerSecond);
        }

        [Fact]
        public void EntityTypeConfig_ParameterizedConstructor_WithDefaults_UsesDefaultValues()
        {
            // Act
            var config = new EntityTypeConfig("Test Entity", 50, 7);

            // Assert
            Assert.Equal("Test Entity", config.Name);
            Assert.Equal(50, config.BaseHealth);
            Assert.Equal(7, config.BaseDamage);
            Assert.Equal(0f, config.RetreatThreshold);
            Assert.Equal(0, config.HealthRegenPerSecond);
        }
    }
}