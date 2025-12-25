
using Neba.Domain.Awards;

namespace Neba.UnitTests.Domain.Awards;

public sealed class SeasonAwardTypeTests
{
    [Fact]
    public void SeasonAwardType_ShouldHaveCorrectNumberOfAwards()
    {
        // Arrange
        const int expectedCount = 3;

        // Act
        int actualCount = SeasonAwardType.List.Count;

        // Assert
        actualCount.ShouldBe(expectedCount);
    }

    [Theory]
    [InlineData("Bowler of the Year", 1)]
    [InlineData("High Average", 2)]
    [InlineData("High 5-Game Block", 3)]
    public void SeasonAwardType_ShouldHaveCorrectNameAndValue(string expectedName, int expectedValue)
    {
        // Act
        SeasonAwardType? awardType = SeasonAwardType.List.FirstOrDefault(a => a.Value == expectedValue);

        // Assert
        awardType.ShouldNotBeNull();
        awardType.Name.ShouldBe(expectedName);
        awardType.Value.ShouldBe(expectedValue);
    }
}
