using ErrorOr;
using Neba.Website.Application.Documents.Bylaws;

namespace Neba.UnitTests.Website.Documents.Bylaws;

public sealed class RefreshBylawsCacheCommandHandlerTests
{
    private readonly Mock<IBylawsSyncBackgroundJob> _bylawsSyncJobMock;
    private readonly RefreshBylawsCacheCommandHandler _handler;

    public RefreshBylawsCacheCommandHandlerTests()
    {
        _bylawsSyncJobMock = new Mock<IBylawsSyncBackgroundJob>();
        _handler = new RefreshBylawsCacheCommandHandler(_bylawsSyncJobMock.Object);
    }

    [Fact(DisplayName = "Triggers immediate sync and returns job ID")]
    public async Task HandleAsync_ShouldTriggerImmediateSyncAndReturnJobId()
    {
        // Arrange
        const string expectedJobId = "job-12345";
        _bylawsSyncJobMock
            .Setup(j => j.TriggerImmediateSync())
            .Returns(expectedJobId);

        var command = new RefreshBylawsCacheCommand();

        // Act
        ErrorOr<string> result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(expectedJobId);
        _bylawsSyncJobMock.Verify(j => j.TriggerImmediateSync(), Times.Once);
    }

    [Fact(DisplayName = "Respects cancellation token during execution")]
    public async Task HandleAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        const string expectedJobId = "job-67890";

        _bylawsSyncJobMock
            .Setup(j => j.TriggerImmediateSync())
            .Returns(expectedJobId);

        var command = new RefreshBylawsCacheCommand();

        // Act
        ErrorOr<string> result = await _handler.HandleAsync(command, cts.Token);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(expectedJobId);
    }
}
