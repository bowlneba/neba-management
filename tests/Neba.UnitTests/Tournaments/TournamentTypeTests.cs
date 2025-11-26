using Neba.Domain.Tournaments;

namespace Neba.UnitTests.Tournaments;

public sealed class TournamentTypeTests
{
    [Fact]
    public void TournamentType_ShouldHave14DifferentTournamentTypes()
    {
        // Arrange & Act
        IReadOnlyCollection<TournamentType> tournamentTypes = Neba.Domain.Tournaments.TournamentType.List;

        // Assert
        Assert.Equal(14, tournamentTypes.Count);
    }

    [Theory]
    [InlineData("Singles", 10, 1)]
    [InlineData("Doubles", 20, 2)]
    [InlineData("Trios", 30, 3)]
    [InlineData("Non-Champions", 11, 1)]
    [InlineData("Tournament of Champions", 12, 1)]
    [InlineData("Invitational", 13, 1)]
    [InlineData("Masters", 14, 1)]
    [InlineData("High Roller", 15, 1)]
    [InlineData("Senior", 16, 1)]
    [InlineData("Women", 17, 1)]
    [InlineData("Over 40", 18, 1)]
    [InlineData("40 - 49", 19, 1)]
    [InlineData("Over/Under 50 Doubles", 21, 2)]
    [InlineData("Over/Under 40 Doubles", 22, 2)]
    public void TournamentType_ShouldHaveCorrectProperties(string expectedName, int expectedValue, int expectedPlayersPerTeam)
    {
        // Arrange
        TournamentType tournamentType = TournamentType.List
            .Single(tt => tt.Name == expectedName);

        // Act & Assert
        Assert.NotNull(tournamentType);
        Assert.Equal(expectedValue, tournamentType.Value);
        Assert.Equal(expectedPlayersPerTeam, tournamentType.TeamSize);
    }
}
