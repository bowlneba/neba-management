using Neba.Application.Storage;
using Neba.Tests.Website;
using Neba.Website.Application.Awards;
using Neba.Website.Application.Awards.HallOfFame;

namespace Neba.UnitTests.Website.Awards.HallOfFame;

public sealed class ListHallOfFameInductionsQueryHandlerTests
{
    private readonly Mock<IWebsiteAwardQueryRepository> _awardQueryRepositoryMock;
    private readonly Mock<IStorageService> _storageServiceMock;

    private readonly ListHallOfFameInductionsQueryHandler _handler;

    public ListHallOfFameInductionsQueryHandlerTests()
    {
        _awardQueryRepositoryMock = new Mock<IWebsiteAwardQueryRepository>(MockBehavior.Strict);
        _storageServiceMock = new Mock<IStorageService>(MockBehavior.Strict);

        _handler = new ListHallOfFameInductionsQueryHandler(
            _awardQueryRepositoryMock.Object,
            _storageServiceMock.Object);
    }

    [Fact(DisplayName = "Returns Hall of Fame inductions with photo URIs")]
    public async Task HandleAsync_ShouldReturnInductionsWithPhotoUris()
    {
        // Arrange
        var query = new ListHallOfFameInductionsQuery();

        IReadOnlyCollection<HallOfFameInductionDto> inductions = HallOfFameInductionDtoFactory.Bogus(50);

        _awardQueryRepositoryMock
            .Setup(repo => repo.ListHallOfFameInductionsAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(inductions);

        foreach (HallOfFameInductionDto? induction in inductions.Where(i => i.Photo != null))
        {
            var expectedUri = new Uri($"https://storage.example.com/{induction.Photo!.Container}/{induction.Photo.Path}");
            _storageServiceMock
                .Setup(s => s.GetBlobUri(induction.Photo!.Container, induction.Photo.Path))
                .Returns(expectedUri);
        }

        // Act
        IReadOnlyCollection<HallOfFameInductionDto> result
            = await _handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(inductions.Count);

        foreach (HallOfFameInductionDto dto in result)
        {
            HallOfFameInductionDto expectedInduction = inductions.First(i => i.BowlerName == dto.BowlerName && i.Year == dto.Year);

            dto.BowlerName.ShouldBe(expectedInduction.BowlerName);
            dto.Year.ShouldBe(expectedInduction.Year);
            dto.Categories.ShouldBe(expectedInduction.Categories);

            if (expectedInduction.Photo != null)
            {
                dto.PhotoUri.ShouldNotBeNull();
                Uri expectedUri = _storageServiceMock.Object.GetBlobUri(
                    expectedInduction.Photo.Container,
                    expectedInduction.Photo.Path);
                dto.PhotoUri.ShouldBe(expectedUri);
            }
            else
            {
                dto.PhotoUri.ShouldBeNull();
            }
        }
    }
}
