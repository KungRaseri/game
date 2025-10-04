using Game.Adventure.Data;

namespace Game.Adventure.Tests
{
    public class EntityFactoryTests
    {
        [Fact]
        public void CreateEntity_FromConfig_CreatesCorrectEntity()
        {
            // Arrange
            var config = new EntityTypeConfig("Test Warrior", 80, 12, 0.3f, 2);

            // Act
            var entity = EntityFactory.CreateEntity(config);

            // Assert
            Assert.Equal("Test Warrior", entity.Name);
            Assert.Equal(80, entity.MaxHealth);
            Assert.Equal(80, entity.CurrentHealth);
            Assert.Equal(12, entity.DamagePerSecond);
            Assert.Equal(0.3f, entity.RetreatThreshold);
        }

        [Fact]
        public void CreateNamedEntity_WithCustomName_OverridesConfigName()
        {
            // Arrange
            var config = EntityTypes.Goblin;

            // Act
            var entity = EntityFactory.CreateNamedEntity(config, "Goblin Chief");

            // Assert
            Assert.Equal("Goblin Chief", entity.Name);
            Assert.Equal(EntityTypes.Goblin.BaseHealth, entity.MaxHealth);
            Assert.Equal(EntityTypes.Goblin.BaseDamage, entity.DamagePerSecond);
            Assert.Equal(EntityTypes.Goblin.RetreatThreshold, entity.RetreatThreshold);
        }

        [Fact]
        public void CreateNoviceAdventurer_CreatesCorrectStats()
        {
            // Act
            var adventurer = EntityFactory.CreateNoviceAdventurer();

            // Assert
            Assert.Equal("Novice Adventurer", adventurer.Name);
            Assert.Equal(100, adventurer.MaxHealth);
            Assert.Equal(10, adventurer.DamagePerSecond);
            Assert.Equal(0.25f, adventurer.RetreatThreshold);
        }

        [Fact]
        public void CreateNoviceAdventurer_WithCustomName_UsesCustomName()
        {
            // Act
            var adventurer = EntityFactory.CreateNoviceAdventurer("Bob the Brave");

            // Assert
            Assert.Equal("Bob the Brave", adventurer.Name);
            Assert.Equal(100, adventurer.MaxHealth);
            Assert.Equal(10, adventurer.DamagePerSecond);
            Assert.Equal(0.25f, adventurer.RetreatThreshold);
        }

        [Fact]
        public void CreateGoblin_CreatesCorrectStats()
        {
            // Act
            var goblin = EntityFactory.CreateGoblin();

            // Assert
            Assert.Equal("Goblin", goblin.Name);
            Assert.Equal(20, goblin.MaxHealth);
            Assert.Equal(5, goblin.DamagePerSecond);
            Assert.Equal(0f, goblin.RetreatThreshold); // Monsters don't retreat
        }

        [Fact]
        public void CreateOrc_CreatesCorrectStats()
        {
            // Act
            var orc = EntityFactory.CreateOrc();

            // Assert
            Assert.Equal("Orc", orc.Name);
            Assert.Equal(40, orc.MaxHealth);
            Assert.Equal(8, orc.DamagePerSecond);
            Assert.Equal(0f, orc.RetreatThreshold);
        }

        [Fact]
        public void CreateTroll_CreatesCorrectStats()
        {
            // Act
            var troll = EntityFactory.CreateTroll();

            // Assert
            Assert.Equal("Troll", troll.Name);
            Assert.Equal(80, troll.MaxHealth);
            Assert.Equal(12, troll.DamagePerSecond);
            Assert.Equal(0f, troll.RetreatThreshold);
        }

        [Fact]
        public void CreateTroll_WithCustomName_UsesCustomName()
        {
            // Act
            var troll = EntityFactory.CreateTroll("Ancient Troll");

            // Assert
            Assert.Equal("Ancient Troll", troll.Name);
            Assert.Equal(80, troll.MaxHealth);
            Assert.Equal(12, troll.DamagePerSecond);
            Assert.Equal(0f, troll.RetreatThreshold);
        }
    }
}