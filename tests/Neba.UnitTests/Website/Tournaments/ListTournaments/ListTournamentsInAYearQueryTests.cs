using Neba.Application.Caching;
using Neba.Website.Application.Tournaments.ListTournaments;

namespace Neba.UnitTests.Website.Tournaments.ListTournaments;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Tournaments.ListTournaments")]
public sealed class ListTournamentsInAYearQueryTests
{
    [Fact(DisplayName = "Year property reflects initialization value")]
    public void Constructor_WhenYearProvided_SetsYear()
    {
        // Arrange
        var query = new ListTournamentInAYearQuery
        {
            Year = 2025
        };

        // Act
        int year = query.Year;

        // Assert
        year.ShouldBe(2025);
    }

    [Fact(DisplayName = "Builds cache key using query name and year")]
    public void Key_WhenAccessed_ReturnsCacheKeyForYear()
    {
        // Arrange
        var query = new ListTournamentInAYearQuery
        {
            Year = 2027
        };

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe(CacheKeys.Queries.Build(nameof(ListTournamentInAYearQuery), 2027));
    }

    [Fact(DisplayName = "Uses fourteen day cache expiry")]
    public void Expiry_WhenAccessed_ReturnsFourteenDays()
    {
        // Arrange
        var query = new ListTournamentInAYearQuery
        {
            Year = 2026
        };

        // Act
        TimeSpan expiry = query.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(14));
    }

    [Fact(DisplayName = "Includes all tournaments cache tag")]
    public void Tags_WhenAccessed_ReturnsAllTournamentsCacheTags()
    {
        // Arrange
        var query = new ListTournamentInAYearQuery
        {
            Year = 2024
        };

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBe(CacheTags.AllTournaments());
    }
}
