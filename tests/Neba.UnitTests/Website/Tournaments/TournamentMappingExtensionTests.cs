using Neba.Domain.Identifiers;
using Neba.Domain.Tournaments;
using Neba.Tests.Website;
using Neba.Web.Server.Tournaments;
using Neba.Website.Contracts.Tournaments;

namespace Neba.UnitTests.Website.Tournaments;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Tournaments")]
public sealed class TournamentMappingExtensionTests
{
    [Fact(DisplayName = "Maps id from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapId()
    {
        // Arrange
        TournamentId tournamentId = TournamentId.New();
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(id: tournamentId);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Id.ShouldBe(tournamentId);
    }

    [Fact(DisplayName = "Maps name from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapName()
    {
        // Arrange
        const string name = "Spring Championship 2025";
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(name: name);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "Maps thumbnail URL from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapThumbnailUrl()
    {
        // Arrange
        var thumbnailUrl = new Uri("https://example.com/tournament.jpg");
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(thumbnailUrl: thumbnailUrl);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.ThumbnailUrl.ShouldBe(thumbnailUrl);
    }

    [Fact(DisplayName = "Maps null thumbnail URL from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_WithNullThumbnailUrl_ShouldMapNullThumbnailUrl()
    {
        // Arrange
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(thumbnailUrl: null);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.ThumbnailUrl.ShouldBe(new Uri("/images/tournaments/default-logo.jpg", UriKind.Relative));
    }

    [Fact(DisplayName = "Maps bowling center id from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapBowlingCenterId()
    {
        // Arrange
        BowlingCenterId bowlingCenterId = BowlingCenterId.New();
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(bowlingCenterId: bowlingCenterId);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlingCenterId.ShouldBe(bowlingCenterId);
    }

    [Fact(DisplayName = "Maps null bowling center id to empty id in view model")]
    public void TournamentSummaryResponse_ToViewModel_WithNullBowlingCenterId_ShouldMapEmptyId()
    {
        // Arrange
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(bowlingCenterId: null);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlingCenterId.ShouldBe(BowlingCenterId.Empty);
    }

    [Fact(DisplayName = "Maps bowling center name from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapBowlingCenterName()
    {
        // Arrange
        const string bowlingCenterName = "Strike Zone Bowling";
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(bowlingCenterName: bowlingCenterName);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlingCenterName.ShouldBe(bowlingCenterName);
    }

    [Fact(DisplayName = "Maps null bowling center name to 'TBD' in view model")]
    public void TournamentSummaryResponse_ToViewModel_WithNullBowlingCenterName_ShouldMapTBD()
    {
        // Arrange
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(bowlingCenterName: null);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.BowlingCenterName.ShouldBe("TBD");
    }

    [Fact(DisplayName = "Maps start date from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapStartDate()
    {
        // Arrange
        DateOnly startDate = new(2025, 3, 15);
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(startDate: startDate);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.StartDate.ShouldBe(startDate);
    }

    [Fact(DisplayName = "Maps end date from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapEndDate()
    {
        // Arrange
        DateOnly endDate = new(2025, 3, 17);
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(endDate: endDate);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.EndDate.ShouldBe(endDate);
    }

    [Fact(DisplayName = "Maps tournament type name from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapTournamentTypeName()
    {
        // Arrange
        TournamentType tournamentType = TournamentType.Doubles;
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(tournamentType: tournamentType);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.TournamentType.ShouldBe(tournamentType.Name);
    }

    [Fact(DisplayName = "Maps pattern length category name from response to view model")]
    public void TournamentSummaryResponse_ToViewModel_ShouldMapPatternLengthCategoryName()
    {
        // Arrange
        PatternLengthCategory patternLengthCategory = PatternLengthCategory.LongPattern;
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(patternLengthCategory: patternLengthCategory);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.PatternLengthCategory.ShouldBe(patternLengthCategory.Name);
    }

    [Fact(DisplayName = "Maps null pattern length category to null in view model")]
    public void TournamentSummaryResponse_ToViewModel_WithNullPatternLengthCategory_ShouldMapNull()
    {
        // Arrange
        TournamentSummaryResponse response = TournamentSummaryResponseFactory.Create(patternLengthCategory: null);

        // Act
        TournamentSummaryViewModel viewModel = response.ToViewModel();

        // Assert
        viewModel.PatternLengthCategory.ShouldBeNull();
    }
}
