using System.Globalization;
using Neba.Application.Caching;
using Neba.Domain.Identifiers;

namespace Neba.UnitTests.Caching;

public sealed class CacheTagTests
{
    [Fact(DisplayName = "Documents tags include all three levels")]
    public void Documents_ReturnsCacheTagsWithThreeLevels()
    {
        // Arrange
        const string documentKey = "bylaws";

        // Act
        string[] tags = CacheTags.Documents(documentKey);

        // Assert
        tags.Length.ShouldBe(3);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:documents");
        tags[2].ShouldBe("website:document:bylaws");
    }

    [Fact(DisplayName = "Bowler tags include bowler ID in entity tag")]
    public void Bowler_ReturnsCacheTagsWithBowlerId()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV", CultureInfo.InvariantCulture);

        // Act
        string[] tags = CacheTags.Bowler(bowlerId);

        // Assert
        tags.Length.ShouldBe(3);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:bowlers");
        tags[2].ShouldBe("website:bowler:01ARZ3NDEKTSV4RRFFQ69G5FAV");
    }

    [Fact(DisplayName = "AllBowlers tags include only context and type levels")]
    public void AllBowlers_ReturnsCacheTagsWithTwoLevels()
    {
        // Act
        string[] tags = CacheTags.AllBowlers();

        // Assert
        tags.Length.ShouldBe(2);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:bowlers");
    }

    [Fact(DisplayName = "Tournament tags include tournament ID in entity tag")]
    public void Tournament_ReturnsCacheTagsWithTournamentId()
    {
        // Arrange
        const string tournamentId = "01ARZ3NDEKTSV4RRFFQ69G5FAV";

        // Act
        string[] tags = CacheTags.Tournament(tournamentId);

        // Assert
        tags.Length.ShouldBe(3);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:tournaments");
        tags[2].ShouldBe("website:tournament:01ARZ3NDEKTSV4RRFFQ69G5FAV");
    }

    [Fact(DisplayName = "AllTournaments tags include only context and type levels")]
    public void AllTournaments_ReturnsCacheTagsWithTwoLevels()
    {
        // Act
        string[] tags = CacheTags.AllTournaments();

        // Assert
        tags.Length.ShouldBe(2);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:tournaments");
    }

    [Fact(DisplayName = "Award tags include award type in entity tag")]
    public void Award_ReturnsCacheTagsWithAwardType()
    {
        // Arrange
        const string awardType = "bowler-of-the-year";

        // Act
        string[] tags = CacheTags.Award(awardType);

        // Assert
        tags.Length.ShouldBe(3);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:awards");
        tags[2].ShouldBe("website:award:bowler-of-the-year");
    }

    [Fact(DisplayName = "Award tags with high average type")]
    public void Award_HighAverage_ReturnsCacheTagsWithCorrectType()
    {
        // Act
        string[] tags = CacheTags.Award(CacheTags.AwardTypes.HighAverage);

        // Assert
        tags.Length.ShouldBe(3);
        tags[2].ShouldBe("website:award:high-average");
    }

    [Fact(DisplayName = "Award tags with high block type")]
    public void Award_HighBlock_ReturnsCacheTagsWithCorrectType()
    {
        // Act
        string[] tags = CacheTags.Award(CacheTags.AwardTypes.HighBlock);

        // Assert
        tags.Length.ShouldBe(3);
        tags[2].ShouldBe("website:award:high-block");
    }

    [Fact(DisplayName = "AllAwards tags include only context and type levels")]
    public void AllAwards_ReturnsCacheTagsWithTwoLevels()
    {
        // Act
        string[] tags = CacheTags.AllAwards();

        // Assert
        tags.Length.ShouldBe(2);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:awards");
    }

    [Fact(DisplayName = "Job tags include job type and target in entity tag")]
    public void Job_ReturnsCacheTagsWithJobTypeAndTarget()
    {
        // Arrange
        const string jobType = "doc-sync";
        const string target = "bylaws";

        // Act
        string[] tags = CacheTags.Job(jobType, target);

        // Assert
        tags.Length.ShouldBe(3);
        tags[0].ShouldBe("website");
        tags[1].ShouldBe("website:jobs");
        tags[2].ShouldBe("website:job:doc-sync:bylaws");
    }

    [Fact(DisplayName = "Job tags with document sync constant")]
    public void Job_DocumentSync_ReturnsCacheTagsWithCorrectType()
    {
        // Act
        string[] tags = CacheTags.Job(CacheTags.JobTypes.DocumentSync, "tournament-rules");

        // Assert
        tags.Length.ShouldBe(3);
        tags[2].ShouldBe("website:job:doc-sync:tournament-rules");
    }

    [Theory(DisplayName = "All tags follow ADR-003 format")]
    [InlineData("website", TestDisplayName = "Context level tag is valid")]
    [InlineData("website:documents", TestDisplayName = "Type level documents tag is valid")]
    [InlineData("website:document:bylaws", TestDisplayName = "Entity level document tag is valid")]
    [InlineData("website:bowlers", TestDisplayName = "Type level bowlers tag is valid")]
    [InlineData("website:bowler:01ARZ3NDEK", TestDisplayName = "Entity level bowler tag is valid")]
    [InlineData("website:tournaments", TestDisplayName = "Type level tournaments tag is valid")]
    [InlineData("website:tournament:01ARZ3NDEK", TestDisplayName = "Entity level tournament tag is valid")]
    [InlineData("website:awards", TestDisplayName = "Type level awards tag is valid")]
    [InlineData("website:award:bowler-of-the-year", TestDisplayName = "Entity level award tag is valid")]
    [InlineData("website:jobs", TestDisplayName = "Type level jobs tag is valid")]
    [InlineData("website:job:doc-sync:bylaws", TestDisplayName = "Entity level job tag is valid")]
    public void CacheTags_FollowAdr003Format(string tag)
    {
        // Arrange & Act
        string[] parts = tag.Split(':');

        // Assert
        parts.ShouldNotBeEmpty();
        parts.Length.ShouldBeInRange(1, 4);
        parts[0].ShouldBe("website");
        parts.ShouldAllBe(p => !string.IsNullOrWhiteSpace(p));
    }

    [Fact(DisplayName = "Different tag levels enable different invalidation scopes")]
    public void CacheTags_SupportDifferentInvalidationScopes()
    {
        // Arrange
        string[] documentTags = CacheTags.Documents("bylaws");

        // Act & Assert - Level 1: Clear all website caches
        string level1Tag = documentTags[0];
        level1Tag.ShouldBe("website");

        // Act & Assert - Level 2: Clear all document caches
        string level2Tag = documentTags[1];
        level2Tag.ShouldBe("website:documents");

        // Act & Assert - Level 3: Clear specific document cache
        string level3Tag = documentTags[2];
        level3Tag.ShouldBe("website:document:bylaws");
    }

    [Fact(DisplayName = "Award type constants are valid tag identifiers")]
    public void AwardTypes_ConstantsAreValidTagIdentifiers()
    {
        // Assert
        CacheTags.AwardTypes.BowlerOfTheYear.ShouldBe("bowler-of-the-year");
        CacheTags.AwardTypes.HighAverage.ShouldBe("high-average");
        CacheTags.AwardTypes.HighBlock.ShouldBe("high-block");
    }

    [Fact(DisplayName = "Job type constants are valid tag identifiers")]
    public void JobTypes_ConstantsAreValidTagIdentifiers()
    {
        // Assert
        CacheTags.JobTypes.DocumentSync.ShouldBe("doc-sync");
    }
}

