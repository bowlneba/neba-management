using Neba.Website.Domain.Tournaments;

namespace Neba.UnitTests.Tournaments;
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
        tournamentTypes.Count.ShouldBe(14);
    }

    [Theory(DisplayName = "Has correct properties for all types")]
    [InlineData("Singles", 10, 1, true, TestDisplayName = "Singles type is correct")]
    [InlineData("Doubles", 20, 2, true, TestDisplayName = "Doubles type is correct")]
    [InlineData("Trios", 30, 3, true, TestDisplayName = "Trios type is correct")]
    [InlineData("Non-Champions", 11, 1, true, TestDisplayName = "Non-Champions type is correct")]
    [InlineData("Tournament of Champions", 12, 1, true, TestDisplayName = "Tournament of Champions type is correct")]
    [InlineData("Invitational", 13, 1, true, TestDisplayName = "Invitational type is correct")]
    [InlineData("Masters", 14, 1, true, TestDisplayName = "Masters type is correct")]
    [InlineData("High Roller", 15, 1, false, TestDisplayName = "High Roller type is correct and inactive")]
    [InlineData("Senior", 16, 1, true, TestDisplayName = "Senior type is correct")]
    [InlineData("Women", 17, 1, true, TestDisplayName = "Women type is correct")]
    [InlineData("Over 40", 18, 1, false, TestDisplayName = "Over 40 type is correct and inactive")]
    [InlineData("40 - 49", 19, 1, false, TestDisplayName = "40 - 49 type is correct and inactive")]
    [InlineData("Over/Under 50 Doubles", 21, 2, true, TestDisplayName = "Over/Under 50 Doubles type is correct")]
    [InlineData("Over/Under 40 Doubles", 22, 2, false, TestDisplayName = "Over/Under 40 Doubles type is correct and inactive")]
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
