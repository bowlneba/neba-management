using AngleSharp.Dom;
using Bunit;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Neba;
using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Neba.Tests;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Refit;

namespace Neba.WebTests.History.Champions;

public sealed class YearViewTests : TestContextWrapper
{
    private void SetupMockApiService(ErrorOr<IReadOnlyCollection<BowlerTitleViewModel>> result)
    {
        var mockNebaApi = new Mock<INebaApi>();

        if (result.IsError)
        {
            // Mock failure scenario - throw HttpRequestException to simulate network error
            mockNebaApi
                .Setup(x => x.GetAllTitlesAsync())
                .ThrowsAsync(new HttpRequestException("Simulated API error"));
        }
        else
        {
            // Mock success scenario - convert ViewModels back to DTOs for the API response
            var dtos = result.Value.Select(vm => new BowlerTitleResponse
            {
                BowlerId = vm.BowlerId,
                BowlerName = vm.BowlerName,
                TournamentMonth = Month.FromValue(vm.TournamentMonth),
                TournamentYear = vm.TournamentYear,
                TournamentType = vm.TournamentType
            }).ToList();

            var collectionResponse = new CollectionResponse<BowlerTitleResponse> { Items = dtos };
            using var apiResponse = new Refit.ApiResponse<CollectionResponse<BowlerTitleResponse>>(
                new HttpResponseMessage(System.Net.HttpStatusCode.OK),
                collectionResponse,
                new RefitSettings());

            mockNebaApi
                .Setup(x => x.GetAllTitlesAsync())
                .ReturnsAsync(apiResponse);
        }

        // Use the real NebaApiService with the mocked INebaApi
        // The service will call GetAllTitlesAsync and transform it to GetTitlesByYearAsync
        var nebaApiService = new NebaApiService(mockNebaApi.Object);
        TestContext.Services.AddSingleton(nebaApiService);
    }

    [Fact]
    public async Task ShouldDisplayEmptyMessageWhenNoTitles()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>();
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        cut.Markup.ShouldContain("No titles found");
    }

    [Fact]
    public async Task ShouldGroupTitlesByYear()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentMonth: 12, tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentMonth: 11, tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentMonth: 10, tournamentYear: 2023)
        };
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        IReadOnlyList<IElement> yearHeaders = cut.FindAll(".year-header");
        yearHeaders.Count.ShouldBe(2); // 2024 and 2023
    }

    [Fact]
    public async Task ShouldDisplayYearHeadersWithTitleCounts()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024)
        };
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        IElement yearHeader = cut.Find(".year-header h3");
        yearHeader.TextContent.ShouldContain("2024");
        yearHeader.TextContent.ShouldContain("2 titles");
    }

    [Fact]
    public async Task ShouldUseSingularFormForOneTitle()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create()
        };
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        IElement yearHeader = cut.Find(".year-header h3");
        yearHeader.TextContent.ShouldContain("1 title");
    }

    [Fact]
    public async Task ShouldExpandAllSectionsByDefault()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2023)
        };
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        IReadOnlyList<IElement> expandedSections = cut.FindAll(".year-expanded");
        expandedSections.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldToggleSectionWhenHeaderClicked()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create()
        };
        SetupMockApiService(titles);

        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Act - Click the toggle button
        IElement toggleButton = cut.Find(".year-header");
        await toggleButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert - Section should be collapsed
        IReadOnlyList<IElement> collapsedSections = cut.FindAll(".year-collapsed");
        collapsedSections.Count.ShouldBe(1);

        // Act - Click again
        await toggleButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert - Section should be expanded again
        IReadOnlyList<IElement> expandedSections = cut.FindAll(".year-expanded");
        expandedSections.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldDisplayTableWithCorrectColumns()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create()
        };
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        IElement thead = cut.Find("thead");
        thead.TextContent.ShouldContain("Month");
        thead.TextContent.ShouldContain("Tournament Type");
        thead.TextContent.ShouldContain("Champions");
    }

    [Fact]
    public async Task ShouldDisplayChampionNamesAsClickableButtons()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(bowlerName: "Alice Smith")
        };
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        IElement championButton = cut.Find(".champion-name");
        championButton.TagName.ShouldBe("BUTTON");
        championButton.TextContent.ShouldContain("Alice Smith");
    }

    [Fact]
    public async Task ShouldSortYearsDescending()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentYear: 2022),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2023)
        };
        SetupMockApiService(titles);

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>();

        await Task.Delay(200); // Wait for async rendering

        // Assert
        IReadOnlyList<IElement> yearHeaders = cut.FindAll(".year-header h3");
        yearHeaders[0].TextContent.ShouldContain("2024");
        yearHeaders[1].TextContent.ShouldContain("2023");
        yearHeaders[2].TextContent.ShouldContain("2022");
    }

    [Fact]
    public async Task ShouldInvokeOnChampionClickWhenChampionNameClicked()
    {
        // Arrange
        BowlerTitleViewModel? clickedChampion = null;
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(bowlerName: "Alice")
        };
        SetupMockApiService(titles);

        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(
                this, champion => clickedChampion = champion)));

        await Task.Delay(200); // Wait for async rendering

        // Act
        IElement championButton = cut.Find(".champion-name");
        await championButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        clickedChampion.ShouldNotBeNull();
        clickedChampion.BowlerName.ShouldBe("Alice");
    }
}
