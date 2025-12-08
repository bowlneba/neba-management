namespace Neba.UnitTests.Awards;

public sealed class SeasonalAwardTypeTests
{
    [Fact]
    public void SeasonalAwardType_ShouldHaveCorrectNumberOfAwards()
    {
        // Arrange
        var expectedCount = 3;

        // Act
        var actualCount = SeasonalAwardType.List.Count;

        // Assert
        Assert.Equal(expectedCount, actualCount);
    }

    [Theory]
    [InlineData("Bowler of the Year", 1)]
    [InlineData("High Average", 2)]
    [InlineData("High 5-Game Block", 3)]
    public void SeasonalAwardType_ShouldHaveCorrectNameAndValue(string expectedName, int expectedValue)
    {
        // Act
        var awardType = SeasonalAwardType.List.FirstOrDefault(a => a.Value == expectedValue);

        // Assert
        Assert.NotNull(awardType);
        Assert.Equal(expectedName, awardType.Name);
        Assert.Equal(expectedValue, awardType.Value);
    }
}
