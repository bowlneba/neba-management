using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Api.Endpoints.Website.History.Champions;
using Neba.Domain.Bowlers;
using Neba.Tests;
using Neba.Contracts.History.Champions;
using Neba.Domain.Tournaments;

namespace Neba.UnitTests.Mapping.Api;

public sealed class ChampionsMappingExtensionsTests
{
    [Fact]
    public void BowlerTitleCountDto_ToResponseModel_MapsBowlerIdCorrectly()
    {
        // Arrange
        BowlerId bowlerId = BowlerId.New();
        BowlerTitleCountDto dto = BowlerTitleCountDtoFactory.Create(bowlerId: bowlerId);

        // Act
        GetBowlerTitleCountsResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerId.ShouldBe(bowlerId.Value);
    }

    [Fact]
    public void BowlerTitleCountDto_ToResponseModel_MapsBowlerNameCorrectly()
    {
        // Arrange
        const string bowlerName = "Jane Smith";
        BowlerTitleCountDto dto = BowlerTitleCountDtoFactory.Create(bowlerName: bowlerName);

        // Act
        GetBowlerTitleCountsResponse response = dto.ToResponseModel();

        // Assert
        response.BowlerName.ShouldBe(bowlerName);
    }

    [Fact]
    public void BowlerTitleCountDto_ToResponseModel_MapsTitleCountCorrectly()
    {
        // Arrange
        const int titleCount = 5;
        BowlerTitleCountDto dto = BowlerTitleCountDtoFactory.Create(titleCount: titleCount);

        // Act
        GetBowlerTitleCountsResponse response = dto.ToResponseModel();

        // Assert
        response.TitleCount.ShouldBe(titleCount);
    }
}
