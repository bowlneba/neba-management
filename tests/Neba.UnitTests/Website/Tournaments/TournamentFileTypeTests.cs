
using Neba.Website.Domain.Tournaments;

namespace Neba.UnitTests.Website.Tournaments;

public sealed class TournamentFileTypeTests
{
    [Fact(DisplayName = "TournamentFileType should have one file type")]
    public void TournamentFileType_ShouldHaveOneFileType()
    {
        // Arrange & Act
        IReadOnlyCollection<TournamentFileType> fileTypes = TournamentFileType.List;

        // Assert
        fileTypes.Count.ShouldBe(1);
    }

    [Theory(DisplayName = "Has correct name and value for all file types")]
    [InlineData("Logo", 1, TestDisplayName = "Logo file type is correct")]
    public void TournamentFileType_ShouldHaveCorrectNameAndValue(string expectedName, int expectedValue)
    {
        // Arrange & Act
        TournamentFileType? fileType = TournamentFileType.List
            .SingleOrDefault(dt => dt.Name == expectedName);

        // Assert
        fileType.ShouldNotBeNull();
        fileType.Name.ShouldBe(expectedName);
        fileType.Value.ShouldBe(expectedValue);
    }
}
