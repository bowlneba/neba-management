using Bunit;
using ErrorOr;
using Microsoft.AspNetCore.Components;
using Moq;
using Neba.Contracts.History.Champions;
using Neba.Web.Server.Components;
using Neba.Web.Server.History.Champions;
using Neba.Web.Server.Services;
using Shouldly;

namespace Neba.WebTests.History.Champions;

public sealed class ChampionsTests : TestContextWrapper
{
    private readonly Mock<NebaApiService> _mockApiService;

    public ChampionsTests()
    {
        _mockApiService = new Mock<NebaApiService>(MockBehavior.Strict);
        TestContext.Services.AddSingleton(_mockApiService.Object);
    }

    [Fact]
    public async Task ShouldLoadChampionsOnInitialization()
    {
        // Arrange
        var expectedChampions = new GetBowlerTitleCountsResponse(
            Items: new[]
            {
                new BowlerTitleCountResponse(
                    BowlerId: Guid.NewGuid(),
                    BowlerName: "John Doe",
                    Titles: 5,
                    HallOfFame: false
                ),
                new BowlerTitleCountResponse(
                    BowlerId: Guid.NewGuid(),
                    BowlerName: "Jane Smith",
                    Titles: 10,
                    HallOfFame: true
                )
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(expectedChampions));

        // Act
        var cut = Render<Champions>();

        // Wait for async initialization
        await Task.Delay(100);

        // Assert
        _mockApiService.Verify(x => x.GetBowlerTitleCountsAsync(), Times.Once);

        // Should render CardGridLayout
        var cardLayout = cut.FindComponent<CardGridLayout>();
        cardLayout.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldDisplayLoadingIndicatorWhileLoading()
    {
        // Arrange
        var tcs = new TaskCompletionSource<ErrorOr<GetBowlerTitleCountsResponse>>();

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .Returns(tcs.Task);

        // Act
        var cut = Render<Champions>();

        // Assert - loading indicator should be visible
        var loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.Instance.IsVisible.ShouldBeTrue();
        loadingIndicator.Instance.Text.ShouldBe("Loading champions...");

        // Complete the task
        tcs.SetResult(ErrorOrFactory.From(new GetBowlerTitleCountsResponse(
            Items: Array.Empty<BowlerTitleCountResponse>()
        )));
    }

    [Fact]
    public async Task ShouldHideLoadingIndicatorAfterDataLoads()
    {
        // Arrange
        var response = new GetBowlerTitleCountsResponse(
            Items: new[]
            {
                new BowlerTitleCountResponse(
                    BowlerId: Guid.NewGuid(),
                    BowlerName: "John Doe",
                    Titles: 5,
                    HallOfFame: false
                )
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<Champions>();

        // Wait for async initialization
        await Task.Delay(100);

        // Assert
        var loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.Instance.IsVisible.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldDisplayErrorWhenLoadingFails()
    {
        // Arrange
        var error = Error.Failure("TestError", "Failed to load champions");

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(error);

        // Act
        var cut = Render<Champions>();

        // Wait for async initialization
        await Task.Delay(100);

        // Assert
        var alert = cut.FindComponent<Neba.Web.Server.Components.Notifications.NebaAlert>();
        alert.ShouldNotBeNull();
        alert.Instance.Message.ShouldContain("Failed to load champions");
    }

    [Fact]
    public async Task ShouldDismissErrorAlert()
    {
        // Arrange
        var error = Error.Failure("TestError", "Failed to load champions");

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(error);

        var cut = Render<Champions>();
        await Task.Delay(100);

        // Act - Dismiss the alert
        var alert = cut.FindComponent<Neba.Web.Server.Components.Notifications.NebaAlert>();
        await alert.Instance.OnDismiss.InvokeAsync();

        // Assert - Error message should be cleared
        var alerts = cut.FindComponents<Neba.Web.Server.Components.Notifications.NebaAlert>();
        alerts.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRenderPageTitle()
    {
        // Arrange
        var response = new GetBowlerTitleCountsResponse(
            Items: Array.Empty<BowlerTitleCountResponse>()
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<Champions>();
        await Task.Delay(100);

        // Assert
        var title = cut.Find("h1");
        title.TextContent.ShouldBe("Champions");

        var description = cut.Find("p.text-sm");
        description.TextContent.ShouldBe("NEBA tournament title leaders throughout history");
    }

    [Fact]
    public async Task ShouldRenderCardGridLayoutWithChampions()
    {
        // Arrange
        var response = new GetBowlerTitleCountsResponse(
            Items: new[]
            {
                new BowlerTitleCountResponse(
                    BowlerId: Guid.NewGuid(),
                    BowlerName: "John Doe",
                    Titles: 5,
                    HallOfFame: false
                ),
                new BowlerTitleCountResponse(
                    BowlerId: Guid.NewGuid(),
                    BowlerName: "Jane Smith",
                    Titles: 10,
                    HallOfFame: true
                )
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<Champions>();
        await Task.Delay(100);

        // Assert
        var cardLayout = cut.FindComponent<CardGridLayout>();
        cardLayout.ShouldNotBeNull();
        cardLayout.Instance.Champions.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldNotRenderCardGridLayoutWhenNoChampions()
    {
        // Arrange
        var response = new GetBowlerTitleCountsResponse(
            Items: Array.Empty<BowlerTitleCountResponse>()
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<Champions>();
        await Task.Delay(100);

        // Assert
        var cardLayouts = cut.FindComponents<CardGridLayout>();
        cardLayouts.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldOpenModalWhenChampionClicked()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitleCountsResponse(
            Items: new[]
            {
                new BowlerTitleCountResponse(
                    BowlerId: bowlerId,
                    BowlerName: "John Doe",
                    Titles: 5,
                    HallOfFame: false
                )
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        var cut = Render<Champions>();
        await Task.Delay(100);

        // Act - Click on champion card
        var cardLayout = cut.FindComponent<CardGridLayout>();
        var champion = new BowlerTitleCountViewModel
        {
            BowlerId = bowlerId,
            BowlerName = "John Doe",
            Titles = 5,
            HallOfFame = false
        };
        await cardLayout.Instance.OnChampionClick.InvokeAsync(champion);

        // Assert
        var modal = cut.FindComponent<BowlerTitlesModal>();
        modal.Instance.IsOpen.ShouldBeTrue();
        modal.Instance.BowlerId.ShouldBe(bowlerId);
        modal.Instance.BowlerName.ShouldBe("John Doe");
        modal.Instance.HallOfFame.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldPassHallOfFameStatusToModal()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitleCountsResponse(
            Items: new[]
            {
                new BowlerTitleCountResponse(
                    BowlerId: bowlerId,
                    BowlerName: "Hall of Famer",
                    Titles: 20,
                    HallOfFame: true
                )
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        var cut = Render<Champions>();
        await Task.Delay(100);

        // Act
        var cardLayout = cut.FindComponent<CardGridLayout>();
        var champion = new BowlerTitleCountViewModel
        {
            BowlerId = bowlerId,
            BowlerName = "Hall of Famer",
            Titles = 20,
            HallOfFame = true
        };
        await cardLayout.Instance.OnChampionClick.InvokeAsync(champion);

        // Assert
        var modal = cut.FindComponent<BowlerTitlesModal>();
        modal.Instance.HallOfFame.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRenderBowlerTitlesModal()
    {
        // Arrange
        var response = new GetBowlerTitleCountsResponse(
            Items: Array.Empty<BowlerTitleCountResponse>()
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<Champions>();
        await Task.Delay(100);

        // Assert
        var modal = cut.FindComponent<BowlerTitlesModal>();
        modal.ShouldNotBeNull();
        modal.Instance.IsOpen.ShouldBeFalse(); // Initially closed
    }

    [Fact]
    public async Task ShouldCloseModalWhenOnCloseInvoked()
    {
        // Arrange
        var bowlerId = Guid.NewGuid();
        var response = new GetBowlerTitleCountsResponse(
            Items: new[]
            {
                new BowlerTitleCountResponse(
                    BowlerId: bowlerId,
                    BowlerName: "John Doe",
                    Titles: 5,
                    HallOfFame: false
                )
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        var cut = Render<Champions>();
        await Task.Delay(100);

        // Open modal
        var cardLayout = cut.FindComponent<CardGridLayout>();
        var champion = new BowlerTitleCountViewModel
        {
            BowlerId = bowlerId,
            BowlerName = "John Doe",
            Titles = 5,
            HallOfFame = false
        };
        await cardLayout.Instance.OnChampionClick.InvokeAsync(champion);

        var modal = cut.FindComponent<BowlerTitlesModal>();
        modal.Instance.IsOpen.ShouldBeTrue();

        // Act - Close modal
        await modal.Instance.OnClose.InvokeAsync();

        // Assert
        modal.Instance.IsOpen.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldConvertApiResponseToViewModels()
    {
        // Arrange
        var bowlerId1 = Guid.NewGuid();
        var bowlerId2 = Guid.NewGuid();
        var response = new GetBowlerTitleCountsResponse(
            Items: new[]
            {
                new BowlerTitleCountResponse(
                    BowlerId: bowlerId1,
                    BowlerName: "John Doe",
                    Titles: 5,
                    HallOfFame: false
                ),
                new BowlerTitleCountResponse(
                    BowlerId: bowlerId2,
                    BowlerName: "Jane Smith",
                    Titles: 10,
                    HallOfFame: true
                )
            }
        );

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(ErrorOrFactory.From(response));

        // Act
        var cut = Render<Champions>();
        await Task.Delay(100);

        // Assert
        var cardLayout = cut.FindComponent<CardGridLayout>();
        var champions = cardLayout.Instance.Champions;

        champions.Count.ShouldBe(2);
        champions[0].BowlerId.ShouldBe(bowlerId1);
        champions[0].BowlerName.ShouldBe("John Doe");
        champions[0].Titles.ShouldBe(5);
        champions[0].HallOfFame.ShouldBeFalse();

        champions[1].BowlerId.ShouldBe(bowlerId2);
        champions[1].BowlerName.ShouldBe("Jane Smith");
        champions[1].Titles.ShouldBe(10);
        champions[1].HallOfFame.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHideLoadingIndicatorEvenWhenErrorOccurs()
    {
        // Arrange
        var error = Error.Failure("TestError", "Something went wrong");

        _mockApiService
            .Setup(x => x.GetBowlerTitleCountsAsync())
            .ReturnsAsync(error);

        // Act
        var cut = Render<Champions>();
        await Task.Delay(100);

        // Assert
        var loadingIndicator = cut.FindComponent<NebaLoadingIndicator>();
        loadingIndicator.Instance.IsVisible.ShouldBeFalse();
    }
}
