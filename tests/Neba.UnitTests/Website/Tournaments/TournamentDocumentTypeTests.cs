
using Neba.Website.Domain.Tournaments;

namespace Neba.UnitTests.Website.Tournaments;

public sealed class TournamentDocumentTypeTests
{
    [Fact(DisplayName = "TournamentDocumentType should have one document type")]
    public void TournamentDocumentType_ShouldHaveOneDocumentType()
    {
        // Arrange & Act
        IReadOnlyCollection<TournamentDocumentType> documentTypes = TournamentDocumentType.List;

        // Assert
        documentTypes.Count.ShouldBe(1);
    }

    [Theory(DisplayName = "Has correct name and value for all document types")]
    [InlineData("Logo", 1, TestDisplayName = "Logo document type is correct")]
    public void TournamentDocumentType_ShouldHaveCorrectNameAndValue(string expectedName, int expectedValue)
    {
        // Arrange & Act
        TournamentDocumentType? documentType = TournamentDocumentType.List
            .SingleOrDefault(dt => dt.Name == expectedName);

        // Assert
        documentType.ShouldNotBeNull();
        documentType.Name.ShouldBe(expectedName);
        documentType.Value.ShouldBe(expectedValue);
    }
}
