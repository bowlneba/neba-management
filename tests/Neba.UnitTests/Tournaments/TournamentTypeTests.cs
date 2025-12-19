using Neba.Website.Domain.Tournaments;

namespace Neba.UnitTests.Tournaments;

public sealed class TournamentTypeTests
{
    [Fact]
    public void TournamentType_ShouldHave14DifferentTournamentTypes()
    {
        // Arrange & Act
        IReadOnlyCollection<TournamentType> tournamentTypes = TournamentType.List;

        // Assert
        tournamentTypes.Count.ShouldBe(14);
    }

    [Theory]
    [InlineData("Singles", 10, 1, true)]
    [InlineData("Doubles", 20, 2, true)]
    [InlineData("Trios", 30, 3, true)]
    [InlineData("Non-Champions", 11, 1, true)]
    [InlineData("Tournament of Champions", 12, 1, true)]
    [InlineData("Invitational", 13, 1, true)]
    [InlineData("Masters", 14, 1, true)]
    [InlineData("High Roller", 15, 1, false)]
    [InlineData("Senior", 16, 1, true)]
    [InlineData("Women", 17, 1, true)]
    [InlineData("Over 40", 18, 1, false)]
    [InlineData("40 - 49", 19, 1, false)]
    [InlineData("Over/Under 50 Doubles", 21, 2, true)]
    [InlineData("Over/Under 40 Doubles", 22, 2, false)]
    public void TournamentType_ShouldHaveCorrectProperties(string expectedName, int expectedValue, int expectedPlayersPerTeam, bool expectedIsActive)
    {
        // Arrange
        TournamentType tournamentType = TournamentType.List
            .Single(tt => tt.Name == expectedName);

        // Act & Assert
        tournamentType.ShouldNotBeNull();
        tournamentType.Value.ShouldBe(expectedValue);
        tournamentType.TeamSize.ShouldBe(expectedPlayersPerTeam);
        tournamentType.ActiveFormat.ShouldBe(expectedIsActive);
    }
}
