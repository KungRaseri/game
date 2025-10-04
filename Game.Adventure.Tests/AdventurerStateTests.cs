using Game.Adventure.Models;

namespace Game.Adventure.Tests
{
    public class AdventurerStateTests
    {
        [Fact]
        public void AdventurerState_ContainsAllExpectedValues()
        {
            // Arrange
            var expectedStates = new[]
            {
                AdventurerState.Idle,
                AdventurerState.Fighting,
                AdventurerState.Retreating,
                AdventurerState.Regenerating,
                AdventurerState.Traveling
            };

            // Act
            var allStates = Enum.GetValues<AdventurerState>();

            // Assert
            Assert.Equal(expectedStates.Length, allStates.Length);
            foreach (var expectedState in expectedStates)
            {
                Assert.Contains(expectedState, allStates);
            }
        }

        [Fact]
        public void AdventurerState_CanBeConvertedToString()
        {
            // Act & Assert
            Assert.Equal("Idle", AdventurerState.Idle.ToString());
            Assert.Equal("Fighting", AdventurerState.Fighting.ToString());
            Assert.Equal("Retreating", AdventurerState.Retreating.ToString());
            Assert.Equal("Regenerating", AdventurerState.Regenerating.ToString());
            Assert.Equal("Traveling", AdventurerState.Traveling.ToString());
        }

        [Fact]
        public void AdventurerState_CanBeCompared()
        {
            // Act & Assert
            Assert.Equal(AdventurerState.Idle, AdventurerState.Idle);
            Assert.NotEqual(AdventurerState.Idle, AdventurerState.Fighting);
            Assert.True(AdventurerState.Idle != AdventurerState.Fighting);
        }

        [Fact]
        public void AdventurerState_CanBeUsedInSwitchStatements()
        {
            // Arrange
            var state = AdventurerState.Fighting;
            var result = "";

            // Act
            switch (state)
            {
                case AdventurerState.Idle:
                    result = "Ready";
                    break;
                case AdventurerState.Fighting:
                    result = "In Combat";
                    break;
                case AdventurerState.Retreating:
                    result = "Escaping";
                    break;
                case AdventurerState.Regenerating:
                    result = "Healing";
                    break;
                case AdventurerState.Traveling:
                    result = "Moving";
                    break;
            }

            // Assert
            Assert.Equal("In Combat", result);
        }

        [Theory]
        [InlineData(AdventurerState.Idle)]
        [InlineData(AdventurerState.Fighting)]
        [InlineData(AdventurerState.Retreating)]
        [InlineData(AdventurerState.Regenerating)]
        [InlineData(AdventurerState.Traveling)]
        public void AdventurerState_AllValuesAreValid(AdventurerState state)
        {
            // Act
            var isDefined = Enum.IsDefined(typeof(AdventurerState), state);

            // Assert
            Assert.True(isDefined);
        }

        [Fact]
        public void AdventurerState_HasCorrectUnderlyingValues()
        {
            // Act & Assert
            Assert.Equal(0, (int)AdventurerState.Idle);
            Assert.Equal(1, (int)AdventurerState.Fighting);
            Assert.Equal(2, (int)AdventurerState.Retreating);
            Assert.Equal(3, (int)AdventurerState.Regenerating);
            Assert.Equal(4, (int)AdventurerState.Traveling);
        }
    }
}
