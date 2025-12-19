using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Tests;
using Neba.Tests.Website;
using Neba.Web.Server.History.Champions;

namespace Neba.WebTests.History.Champions;

public sealed class TitleCountViewTests : TestContextWrapper
{
    [Fact]
    public void ShouldRenderEmptyWhenNoChampions()
    {
        // Arrange & Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, new List<BowlerTitleSummaryViewModel>()));

        // Assert
        IReadOnlyList<IElement> sections = cut.FindAll(".tier-elite-section, .tier-mid-section, .tier-standard-section");
        sections.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldGroupChampionsByTitleCount()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 5),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 5),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 3)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert - Should have 2 groups (5 titles and 3 titles)
        IReadOnlyList<IElement> headers = cut.FindAll("h2");
        headers.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldApplyEliteTierStylesForTwentyOrMoreTitles()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 25, hallOfFame: true)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IElement eliteSection = cut.Find(".tier-elite-section");
        eliteSection.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldApplyMidTierStylesForTenToNineteenTitles()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 15)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IElement midSection = cut.Find(".tier-mid-section");
        midSection.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldApplyStandardTierStylesForLessThanTenTitles()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create()
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IElement standardSection = cut.Find(".tier-standard-section");
        standardSection.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldDisplayCorrectHeaderText()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(),
            BowlerTitleSummaryViewModelFactory.Create()
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IElement header = cut.Find("h2");
        header.TextContent.ShouldContain("5 Titles");
        header.TextContent.ShouldContain("2 bowlers");
    }

    [Fact]
    public void ShouldUseSingularFormForOneTitle()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 1)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IElement header = cut.Find("h2");
        header.TextContent.ShouldContain("1 Title");
        header.TextContent.ShouldContain("1 bowler");
    }

    [Fact]
    public void ShouldExpandAllSectionsByDefault()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 5),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 3)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IReadOnlyList<IElement> expandedSections = cut.FindAll(".tier-expanded");
        expandedSections.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldToggleSectionWhenHeaderClicked()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create()
        };

        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Act - Click the toggle button
        IElement toggleButton = cut.Find("button[type='button']");
        toggleButton.Click();

        // Assert - Section should be collapsed
        IReadOnlyList<IElement> collapsedSections = cut.FindAll(".tier-collapsed");
        collapsedSections.Count.ShouldBe(1);

        // Act - Click again
        toggleButton.Click();

        // Assert - Section should be expanded again
        IReadOnlyList<IElement> expandedSections = cut.FindAll(".tier-expanded");
        expandedSections.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldRenderChampionCards()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(),
            BowlerTitleSummaryViewModelFactory.Create()
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IReadOnlyList<IElement> cards = cut.FindAll("button.group");
        cards.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldDisplayBowlerNamesInCards()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(bowlerName: "Alice Smith"),
            BowlerTitleSummaryViewModelFactory.Create(bowlerName: "Bob Jones")
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        cut.Markup.ShouldContain("Alice Smith");
        cut.Markup.ShouldContain("Bob Jones");
    }

    [Fact]
    public void ShouldDisplayHallOfFameBadgeForHallOfFamers()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 20, hallOfFame: true)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IElement hofImage = cut.Find("img[alt='Hall of Fame']");
        hofImage.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldNotDisplayHallOfFameBadgeForNonHallOfFamers()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create()
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IReadOnlyList<IElement> hofImages = cut.FindAll("img[alt='Hall of Fame']");
        hofImages.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldInvokeOnChampionClickWhenCardClicked()
    {
        // Arrange
        BowlerTitleSummaryViewModel? clickedChampion = null;
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(bowlerName: "Alice")
        };

        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleSummaryViewModel>(
                this, champion => clickedChampion = champion)));

        // Act
        IElement card = cut.Find("button.group");
        await card.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        clickedChampion.ShouldNotBeNull();
        clickedChampion.BowlerName.ShouldBe("Alice");
    }

    [Fact]
    public void ShouldOrderChampionsAlphabeticallyWithinGroup()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(bowlerName: "Zara"),
            BowlerTitleSummaryViewModelFactory.Create(bowlerName: "Alice"),
            BowlerTitleSummaryViewModelFactory.Create(bowlerName: "Mike")
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IReadOnlyList<IElement> cards = cut.FindAll("button.group h3");
        cards[0].TextContent.ShouldContain("Alice");
        cards[1].TextContent.ShouldContain("Mike");
        cards[2].TextContent.ShouldContain("Zara");
    }

    [Fact]
    public void ShouldOrderGroupsByTitleCountDescending()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 3),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 10),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 25, hallOfFame: true)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IReadOnlyList<IElement> headers = cut.FindAll("h2");
        headers[0].TextContent.ShouldContain("25 Titles");
        headers[1].TextContent.ShouldContain("10 Titles");
        headers[2].TextContent.ShouldContain("3 Titles");
    }

    [Fact]
    public void ShouldApplyCorrectGridClasses()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create()
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IElement grid = cut.Find(".grid");
        grid.ClassList.ShouldContain("grid-cols-1");
        grid.ClassList.ShouldContain("sm:grid-cols-2");
        grid.ClassList.ShouldContain("lg:grid-cols-3");
        grid.ClassList.ShouldContain("xl:grid-cols-4");
    }

    [Fact]
    public void ShouldHandleMultipleGroupsCorrectly()
    {
        // Arrange
        List<BowlerTitleSummaryViewModel> champions = new List<BowlerTitleSummaryViewModel>
        {
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 25, hallOfFame: true),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 25, hallOfFame: true),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 10),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 3),
            BowlerTitleSummaryViewModelFactory.Create(titleCount: 3)
        };

        // Act
        IRenderedComponent<TitleCountView> cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        IReadOnlyList<IElement> sections = cut.FindAll(".tier-elite-section, .tier-mid-section, .tier-standard-section");
        sections.Count.ShouldBe(3);

        IReadOnlyList<IElement> cards = cut.FindAll("button.group");
        cards.Count.ShouldBe(5);
    }
}
