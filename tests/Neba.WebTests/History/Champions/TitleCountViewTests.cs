using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.History.Champions;
using Shouldly;

namespace Neba.WebTests.History.Champions;

public sealed class TitleCountViewTests : TestContextWrapper
{
    [Fact]
    public void ShouldRenderEmptyWhenNoChampions()
    {
        // Arrange & Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, new List<BowlerTitleSummaryViewModel>()));

        // Assert
        var sections = cut.FindAll(".tier-elite-section, .tier-mid-section, .tier-standard-section");
        sections.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldGroupChampionsByTitleCount()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Bob", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Charlie", TitleCount = 3, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert - Should have 2 groups (5 titles and 3 titles)
        var headers = cut.FindAll("h2");
        headers.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldApplyEliteTierStylesForTwentyOrMoreTitles()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Elite Bowler", TitleCount = 25, HallOfFame = true }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var eliteSection = cut.Find(".tier-elite-section");
        eliteSection.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldApplyMidTierStylesForTenToNineteenTitles()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Mid Tier Bowler", TitleCount = 15, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var midSection = cut.Find(".tier-mid-section");
        midSection.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldApplyStandardTierStylesForLessThanTenTitles()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Standard Bowler", TitleCount = 5, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var standardSection = cut.Find(".tier-standard-section");
        standardSection.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldDisplayCorrectHeaderText()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Bob", TitleCount = 5, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var header = cut.Find("h2");
        header.TextContent.ShouldContain("5 Titles");
        header.TextContent.ShouldContain("2 bowlers");
    }

    [Fact]
    public void ShouldUseSingularFormForOneTitle()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 1, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var header = cut.Find("h2");
        header.TextContent.ShouldContain("1 Title");
        header.TextContent.ShouldContain("1 bowler");
    }

    [Fact]
    public void ShouldExpandAllSectionsByDefault()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Bob", TitleCount = 3, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var expandedSections = cut.FindAll(".tier-expanded");
        expandedSections.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldToggleSectionWhenHeaderClicked()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false }
        };

        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Act - Click the toggle button
        var toggleButton = cut.Find("button[type='button']");
        toggleButton.Click();

        // Assert - Section should be collapsed
        var collapsedSections = cut.FindAll(".tier-collapsed");
        collapsedSections.Count.ShouldBe(1);

        // Act - Click again
        toggleButton.Click();

        // Assert - Section should be expanded again
        var expandedSections = cut.FindAll(".tier-expanded");
        expandedSections.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldRenderChampionCards()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Bob", TitleCount = 5, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var cards = cut.FindAll("button.group");
        cards.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldDisplayBowlerNamesInCards()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice Smith", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Bob Jones", TitleCount = 5, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        cut.Markup.ShouldContain("Alice Smith");
        cut.Markup.ShouldContain("Bob Jones");
    }

    [Fact]
    public void ShouldDisplayHallOfFameBadgeForHallOfFamers()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Hall of Famer", TitleCount = 20, HallOfFame = true }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var hofImage = cut.Find("img[alt='Hall of Fame']");
        hofImage.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldNotDisplayHallOfFameBadgeForNonHallOfFamers()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Regular Bowler", TitleCount = 5, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var hofImages = cut.FindAll("img[alt='Hall of Fame']");
        hofImages.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldInvokeOnChampionClickWhenCardClicked()
    {
        // Arrange
        BowlerTitleSummaryViewModel? clickedChampion = null;
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false }
        };

        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions)
            .Add(p => p.OnChampionClick, EventCallback.Factory.Create<BowlerTitleSummaryViewModel>(
                this, champion => clickedChampion = champion)));

        // Act
        var card = cut.Find("button.group");
        await card.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        clickedChampion.ShouldNotBeNull();
        clickedChampion.BowlerName.ShouldBe("Alice");
    }

    [Fact]
    public void ShouldOrderChampionsAlphabeticallyWithinGroup()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Zara", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Mike", TitleCount = 5, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var cards = cut.FindAll("button.group h3");
        cards[0].TextContent.ShouldContain("Alice");
        cards[1].TextContent.ShouldContain("Mike");
        cards[2].TextContent.ShouldContain("Zara");
    }

    [Fact]
    public void ShouldOrderGroupsByTitleCountDescending()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Low", TitleCount = 3, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Mid", TitleCount = 10, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "High", TitleCount = 25, HallOfFame = true }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var headers = cut.FindAll("h2");
        headers[0].TextContent.ShouldContain("25 Titles");
        headers[1].TextContent.ShouldContain("10 Titles");
        headers[2].TextContent.ShouldContain("3 Titles");
    }

    [Fact]
    public void ShouldApplyCorrectGridClasses()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Alice", TitleCount = 5, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var grid = cut.Find(".grid");
        grid.ClassList.ShouldContain("grid-cols-1");
        grid.ClassList.ShouldContain("sm:grid-cols-2");
        grid.ClassList.ShouldContain("lg:grid-cols-3");
        grid.ClassList.ShouldContain("xl:grid-cols-4");
    }

    [Fact]
    public void ShouldHandleMultipleGroupsCorrectly()
    {
        // Arrange
        var champions = new List<BowlerTitleSummaryViewModel>
        {
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Elite 1", TitleCount = 25, HallOfFame = true },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Elite 2", TitleCount = 25, HallOfFame = true },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Mid 1", TitleCount = 10, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Standard 1", TitleCount = 3, HallOfFame = false },
            new() { BowlerId = Guid.NewGuid(), BowlerName = "Standard 2", TitleCount = 3, HallOfFame = false }
        };

        // Act
        var cut = Render<TitleCountView>(parameters => parameters
            .Add(p => p.Champions, champions));

        // Assert
        var sections = cut.FindAll(".tier-elite-section, .tier-mid-section, .tier-standard-section");
        sections.Count.ShouldBe(3);

        var cards = cut.FindAll("button.group");
        cards.Count.ShouldBe(5);
    }
}
