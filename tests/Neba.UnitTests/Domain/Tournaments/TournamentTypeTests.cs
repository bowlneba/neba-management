using Neba.Domain.Tournaments;

namespace Neba.UnitTests.Domain.Tournaments;

[Trait("Category", "Unit")]
[Trait("Component", "Tournaments")]

public sealed class TournamentTypeTests
{
    [Fact(DisplayName = "TournamentType should have 14 different tournament types")]
    public void TournamentType_ShouldHave14DifferentTournamentTypes()
    {
        // Arrange & Act
        IReadOnlyCollection<TournamentType> tournamentTypes = TournamentType.List;

        // Assert
        tournamentTypes.Count.ShouldBe(16);
    }

    [Theory(DisplayName = "Has correct properties for all types")]
    [InlineData("Singles", 100, 1, true, TestDisplayName = "Singles type is correct")]
    [InlineData("Doubles", 200, 2, true, TestDisplayName = "Doubles type is correct")]
    [InlineData("Trios", 300, 3, true, TestDisplayName = "Trios type is correct")]
    [InlineData("Baker", 500, 5, true, TestDisplayName = "Baker type is correct")]
    [InlineData("Non-Champions", 101, 1, true, TestDisplayName = "Non-Champions type is correct")]
    [InlineData("Tournament of Champions", 102, 1, true, TestDisplayName = "Tournament of Champions type is correct")]
    [InlineData("Invitational", 103, 1, true, TestDisplayName = "Invitational type is correct")]
    [InlineData("Masters", 104, 1, true, TestDisplayName = "Masters type is correct")]
    [InlineData("High Roller", 105, 1, false, TestDisplayName = "High Roller type is correct and inactive")]
    [InlineData("Senior", 106, 1, true, TestDisplayName = "Senior type is correct")]
    [InlineData("Women", 107, 1, true, TestDisplayName = "Women type is correct")]
    [InlineData("Over 40", 108, 1, false, TestDisplayName = "Over 40 type is correct and inactive")]
    [InlineData("40 - 49", 109, 1, false, TestDisplayName = "40 - 49 type is correct and inactive")]
    [InlineData("Youth", 110, 1, true, TestDisplayName = "Youth type is correct")]
    [InlineData("Over/Under 50 Doubles", 201, 2, true, TestDisplayName = "Over/Under 50 Doubles type is correct")]
    [InlineData("Over/Under 40 Doubles", 202, 2, false, TestDisplayName = "Over/Under 40 Doubles type is correct and inactive")]
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
