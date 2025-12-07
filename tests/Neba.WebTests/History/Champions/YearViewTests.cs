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
    [Fact]
    public void ShouldDisplayEmptyMessageWhenNoTitles()
    {
        // Arrange
        var titlesByYear = new List<TitlesByYearViewModel>();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

        // Assert
        cut.Markup.ShouldContain("No titles found");
    }

    [Fact]
    public void ShouldGroupTitlesByYear()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentMonth: 12, tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentMonth: 11, tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentMonth: 10, tournamentYear: 2023)
        };

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

        // Assert
        IReadOnlyList<IElement> yearHeaders = cut.FindAll(".year-header");
        yearHeaders.Count.ShouldBe(2); // 2024 and 2023
    }

    [Fact]
    public void ShouldDisplayYearHeadersWithTitleCounts()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024)
        };

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

        // Assert
        IElement yearHeader = cut.Find(".year-header h3");
        yearHeader.TextContent.ShouldContain("2024");
        yearHeader.TextContent.ShouldContain("2 titles");
    }

    [Fact]
    public void ShouldUseSingularFormForOneTitle()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create()
        };

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

        // Assert
        IElement yearHeader = cut.Find(".year-header h3");
        yearHeader.TextContent.ShouldContain("1 title");
    }

    [Fact]
    public void ShouldExpandAllSectionsByDefault()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2023)
        };

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

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

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

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
    public void ShouldDisplayTableWithCorrectColumns()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create()
        };

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

        // Assert
        IElement thead = cut.Find("thead");
        thead.TextContent.ShouldContain("Month");
        thead.TextContent.ShouldContain("Tournament Type");
        thead.TextContent.ShouldContain("Champions");
    }

    [Fact]
    public void ShouldDisplayChampionNamesAsClickableButtons()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(bowlerName: "Alice Smith")
        };

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

        // Assert
        IElement championButton = cut.Find(".champion-name");
        championButton.TagName.ShouldBe("BUTTON");
        championButton.TextContent.ShouldContain("Alice Smith");
    }

    [Fact]
    public void ShouldSortYearsDescending()
    {
        // Arrange
        var titles = new List<BowlerTitleViewModel>
        {
            BowlerTitleViewModelFactory.Create(tournamentYear: 2022),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2024),
            BowlerTitleViewModelFactory.Create(tournamentYear: 2023)
        };

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        // Act
        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(this, _ => { }))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

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

        var titlesByYear = titles
            .GroupBy(t => t.TournamentYear)
            .Select(g => new TitlesByYearViewModel { Year = g.Key, Titles = g.ToList() })
            .ToList();

        IRenderedComponent<YearView> cut = Render<YearView>(parameters => parameters
            .Add(p => p.TitlesByYear, titlesByYear)
            .Add(p => p.IsLoading, false)
            .Add(p => p.ErrorMessage, null)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleViewModel>(
                this, champion => clickedChampion = champion))
            .Add(p => p.OnErrorDismiss, EventCallback.Factory.Create(this, () => { })));

        // Act
        IElement championButton = cut.Find(".champion-name");
        await championButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        clickedChampion.ShouldNotBeNull();
        clickedChampion.BowlerName.ShouldBe("Alice");
    }
}
