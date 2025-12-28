using System.Globalization;
using ErrorOr;
using Neba.Application.Messaging;
using Neba.Domain.Identifiers;
using Neba.Tests.Website;
using Neba.Website.Application.Bowlers;
using Neba.Website.Application.Bowlers.BowlerTitles;

namespace Neba.UnitTests.Website.Bowlers.BowlerTitles;

public sealed class BowlerTitlesQueryHandlerTests
{
    private static readonly string[] ExpectedBowlerTags = ["website", "website:bowlers", "website:bowler:01ARZ3NDEKTSV4RRFFQ69G5FAV"];

    private readonly Mock<IWebsiteBowlerQueryRepository> _mockWebsiteBowlerQueryRepository;

    private readonly BowlerTitlesQueryHandler _queryHandler;

    public BowlerTitlesQueryHandlerTests()
    {
        _mockWebsiteBowlerQueryRepository = new Mock<IWebsiteBowlerQueryRepository>(MockBehavior.Strict);

        _queryHandler = new BowlerTitlesQueryHandler(
            _mockWebsiteBowlerQueryRepository.Object);
    }

    [Fact(DisplayName = "Returns bowler not found error when bowler does not exist")]
    public async Task HandleAsync_ShouldReturnBowlerNotFoundError_WhenBowlerDoesNotExist()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitlesQuery query = new() { BowlerId = bowlerId };

        _mockWebsiteBowlerQueryRepository
            .Setup(repo => repo.GetBowlerTitlesAsync(bowlerId, TestContext.Current.CancellationToken))
            .ReturnsAsync((BowlerTitlesDto?)null);

        // Act
        ErrorOr<BowlerTitlesDto> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("Bowler.NotFound");
        result.FirstError.Description.ShouldBe("Bowler was not found.");
        result.FirstError.Metadata!["bowlerId"].ShouldBe(bowlerId);

        Console.WriteLine(result.FirstError);
    }

    [Fact(DisplayName = "Returns bowler titles when bowler exists")]
    public async Task HandleAsync_ShouldReturnBowlerTitlesDto_WhenBowlerExists()
    {
        // Arrange
        BowlerTitlesDto bowler = BowlerTitlesDtoFactory.Bogus();
        BowlerTitlesQuery query = new() { BowlerId = bowler.BowlerId };

        _mockWebsiteBowlerQueryRepository
            .Setup(repo => repo.GetBowlerTitlesAsync(bowler.BowlerId, TestContext.Current.CancellationToken))
            .ReturnsAsync(bowler);

        // Act
        ErrorOr<BowlerTitlesDto> result = await _queryHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(bowler);
    }

    [Fact(DisplayName = "Query implements ICachedQuery interface")]
    public void Query_ShouldImplementICachedQuery()
    {
        // Arrange & Act
        var query = new BowlerTitlesQuery { BowlerId = BowlerId.New() };

        // Assert
        query.ShouldBeAssignableTo<ICachedQuery<ErrorOr<BowlerTitlesDto>>>();
    }

    [Fact(DisplayName = "Query cache key follows naming convention")]
    public void Query_CacheKey_ShouldFollowConvention()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV", CultureInfo.InvariantCulture);
        var query = new BowlerTitlesQuery { BowlerId = bowlerId };

        // Act
        string key = query.Key;

        // Assert
        key.ShouldBe("website:query:BowlerTitlesQuery:01ARZ3NDEKTSV4RRFFQ69G5FAV");
        key.ShouldSatisfyAllConditions(
            k => k.ShouldNotBeNullOrWhiteSpace(),
            k => k.Length.ShouldBeLessThanOrEqualTo(512),
            k => k.Split(':').Length.ShouldBeGreaterThanOrEqualTo(3),
            k => k.Split(':').ShouldAllBe(p => !string.IsNullOrWhiteSpace(p))
        );
        key.Split(':')[0].ShouldBe("website");
        key.Split(':')[1].ShouldBe("query");
        key.Split(':')[2].ShouldBe("BowlerTitlesQuery");
        key.Split(':')[3].ShouldBe("01ARZ3NDEKTSV4RRFFQ69G5FAV");
    }

    [Fact(DisplayName = "Query cache expiry is 7 days")]
    public void Query_CacheExpiry_ShouldBeDefault7Days()
    {
        // Arrange
        var query = new BowlerTitlesQuery { BowlerId = BowlerId.New() };

        // Act & Assert
        ICachedQuery<ErrorOr<BowlerTitlesDto>> cachedQuery = query;
        TimeSpan expiry = cachedQuery.Expiry;

        // Assert
        expiry.ShouldBe(TimeSpan.FromDays(7));
    }

    [Fact(DisplayName = "Query cache tags include bowler hierarchy")]
    public void Query_CacheTags_ShouldIncludeBowlerHierarchy()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV", CultureInfo.InvariantCulture);
        var query = new BowlerTitlesQuery { BowlerId = bowlerId };

        // Act
        IReadOnlyCollection<string> tags = query.Tags;

        // Assert
        tags.ShouldBeEquivalentTo(ExpectedBowlerTags);
    }
}
