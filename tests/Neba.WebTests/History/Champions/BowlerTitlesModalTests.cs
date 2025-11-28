using Bunit;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Moq;
using Neba.Contracts.History.Champions;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Shouldly;

namespace Neba.WebTests.History.Champions;

public sealed class BowlerTitlesModalTests : TestContextWrapper
{
    private readonly Mock<NebaApiService> _mockApiService;

    public BowlerTitlesModalTests()
    {
        _mockApiService = new Mock<NebaApiService>(MockBehavior.Strict);
        TestContext.Services.AddSingleton(_mockApiService.Object);
    }

    [Fact]
    public void ShouldNotRenderWhenIsOpenIsFalse()
    {
        // Arrange & Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Assert
        cut.Markup.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderWhenIsOpenIsTrue()
    {
        // Arrange & Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Assert
        cut.Markup.ShouldNotBeEmpty();
        var modal = cut.FindAll(".bowler-titles-modal");
        modal.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldDisplayDefaultTitleWhenBowlerNameNotProvided()
    {
        // Arrange & Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Assert
        var title = cut.Find(".neba-modal-title");
        title.TextContent.ShouldBe("Bowler Titles");
    }

    [Fact]
    public void ShouldDisplayBowlerNameAsTitle()
    {
        // Arrange & Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerName, "John Doe")
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Assert
        var title = cut.Find(".neba-modal-title");
        title.TextContent.ShouldBe("John Doe");
    }

    [Fact]
    public async Task ShouldInvokeOnCloseWhenCloseButtonClicked()
    {
        // Arrange
        var closeInvoked = false;
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeInvoked = true)));

        // Act
        var closeButton = cut.Find(".neba-modal-close");
        await closeButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        closeInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldLoadTitlesWhenModalOpenedWithBowlerId()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var expectedTitles = new GetBowlerTitlesResponse(
            BowlerName: "John Doe",
            Titles: new[]
            {
                new TitleResponse(TournamentDate: "2024-01-15", TournamentType: "Scratch Singles"),
                new TitleResponse(TournamentDate: "2024-02-20", TournamentType: "Handicap Doubles")
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(ErrorOrFactory.From(expectedTitles));

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.BowlerName, "John Doe")
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        _mockApiService.Verify(x => x.GetBowlerTitlesAsync(bowlerId), Times.Once);

        // Should display titles
        var titleRows = cut.FindAll(".flex.border-b");
        titleRows.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldDisplayLoadingStateWhileLoadingTitles()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<ErrorOr<GetBowlerTitlesResponse>>();

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .Returns(tcs.Task);

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Assert - loading state should be visible
        var loadingText = cut.Find("p.text-gray-600");
        loadingText.TextContent.ShouldBe("Loading titles...");

        // Complete the task
        tcs.SetResult(ErrorOrFactory.From(new GetBowlerTitlesResponse(
            BowlerName: "Test",
            Titles: Array.Empty<TitleResponse>()
        )));
    }

    [Fact]
    public async Task ShouldDisplayErrorWhenLoadingFails()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var error = Error.Failure("TestError", "Failed to load titles");

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(error);

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        var alertComponent = cut.FindComponent<Neba.Web.Server.Components.Notifications.NebaAlert>();
        alertComponent.ShouldNotBeNull();
        alertComponent.Instance.Message.ShouldContain("Failed to load titles");
    }

    [Fact]
    public async Task ShouldDisplayEmptyStateWhenNoTitles()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var emptyResponse = new GetBowlerTitlesResponse(
            BowlerName: "John Doe",
            Titles: Array.Empty<TitleResponse>()
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(ErrorOrFactory.From(emptyResponse));

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        var emptyMessage = cut.Find(".text-center.py-12");
        emptyMessage.TextContent.ShouldContain("No titles found for this bowler");
    }

    [Fact]
    public async Task ShouldDisplayHallOfFameBadgeWhenHallOfFameIsTrue()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitlesResponse(
            BowlerName: "John Doe",
            Titles: new[]
            {
                new TitleResponse(TournamentDate: "2024-01-15", TournamentType: "Scratch Singles")
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.BowlerName, "John Doe")
            .Add(p => p.HallOfFame, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        var hofImages = cut.FindAll("img[alt='Hall of Fame']");
        hofImages.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldNotDisplayHallOfFameBadgeWhenHallOfFameIsFalse()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitlesResponse(
            BowlerName: "John Doe",
            Titles: new[]
            {
                new TitleResponse(TournamentDate: "2024-01-15", TournamentType: "Scratch Singles")
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.BowlerName, "John Doe")
            .Add(p => p.HallOfFame, false)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        var hofImages = cut.FindAll("img[alt='Hall of Fame']");
        hofImages.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderTitlesWithCorrectData()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitlesResponse(
            BowlerName: "John Doe",
            Titles: new[]
            {
                new TitleResponse(TournamentDate: "2024-01-15", TournamentType: "Scratch Singles"),
                new TitleResponse(TournamentDate: "2024-02-20", TournamentType: "Handicap Doubles"),
                new TitleResponse(TournamentDate: "2024-03-10", TournamentType: "Team Event")
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        var titleRows = cut.FindAll(".flex.border-b");
        titleRows.Count.ShouldBe(3);

        // Check first title
        titleRows[0].TextContent.ShouldContain("#1");
        titleRows[0].TextContent.ShouldContain("2024-01-15");
        titleRows[0].TextContent.ShouldContain("Scratch Singles");

        // Check second title
        titleRows[1].TextContent.ShouldContain("#2");
        titleRows[1].TextContent.ShouldContain("2024-02-20");
        titleRows[1].TextContent.ShouldContain("Handicap Doubles");
    }

    [Fact]
    public async Task ShouldDisplayTitleCount()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitlesResponse(
            BowlerName: "John Doe",
            Titles: new[]
            {
                new TitleResponse(TournamentDate: "2024-01-15", TournamentType: "Scratch Singles"),
                new TitleResponse(TournamentDate: "2024-02-20", TournamentType: "Handicap Doubles")
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Wait for async operation
        await Task.Delay(100);

        // Assert
        var titleCountElement = cut.Find(".text-2xl.sm\\:text-3xl");
        titleCountElement.TextContent.ShouldBe("2");
    }

    [Fact]
    public async Task ShouldResetStateWhenModalCloses()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitlesResponse(
            BowlerName: "John Doe",
            Titles: new[]
            {
                new TitleResponse(TournamentDate: "2024-01-15", TournamentType: "Scratch Singles")
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitlesAsync(bowlerId))
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act - Open modal
        var cut = Render<BowlerTitlesModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        await Task.Delay(100);

        // Verify titles loaded
        var titleRows = cut.FindAll(".flex.border-b");
        titleRows.ShouldNotBeEmpty();

        // Act - Close modal
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.BowlerId, bowlerId)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => { })));

        // Assert - Modal should not render when closed
        cut.Markup.ShouldBeEmpty();
    }
}
