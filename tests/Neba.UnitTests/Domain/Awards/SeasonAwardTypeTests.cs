
using Neba.Domain.Awards;

namespace Neba.UnitTests.Domain.Awards;

[Trait("Category", "Unit")]
[Trait("Component", "Domain.Awards")]

public sealed class SeasonAwardTypeTests
{
    [Fact(DisplayName = "SeasonAwardType should have correct number of awards")]
    public void SeasonAwardType_ShouldHaveCorrectNumberOfAwards()
    {
        // Arrange
        const int expectedCount = 3;

        // Act
        int actualCount = SeasonAwardType.List.Count;

        // Assert
        actualCount.ShouldBe(expectedCount);
    }

    [Theory(DisplayName = "Has correct name and value for all types")]
    [InlineData("Bowler of the Year", 1, TestDisplayName = "Bowler of the Year type is correct")]
    [InlineData("High Average", 2, TestDisplayName = "High Average type is correct")]
    [InlineData("High 5-Game Block", 3, TestDisplayName = "High 5-Game Block type is correct")]
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
