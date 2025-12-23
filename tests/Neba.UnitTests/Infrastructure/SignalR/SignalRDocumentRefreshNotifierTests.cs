using Microsoft.AspNetCore.SignalR;
using Neba.Application.Documents;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.SignalR;

public sealed class SignalRDocumentRefreshNotifierTests
{
    private readonly Mock<IHubContext<DocumentRefreshSignalRHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockHubClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly SignalRDocumentRefreshNotifier _notifier;

    public SignalRDocumentRefreshNotifierTests()
    {
        _mockHubContext = new Mock<IHubContext<DocumentRefreshSignalRHub>>();
        _mockHubClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);
        _mockHubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _notifier = new SignalRDocumentRefreshNotifier(_mockHubContext.Object);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithNullHubGroupName_ReturnsEarlyWithoutSending()
    {
        // Arrange
        string? hubGroupName = null;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithEmptyHubGroupName_ReturnsEarlyWithoutSending()
    {
        // Arrange
        var hubGroupName = string.Empty;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithWhitespaceHubGroupName_ReturnsEarlyWithoutSending()
    {
        // Arrange
        var hubGroupName = "   ";

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithValidGroupName_SendsNotificationToCorrectGroup()
    {
        // Arrange
        var groupName = "bylaws-refresh";
        var status = DocumentRefreshStatus.Completed;

        // Act
        await _notifier.NotifyStatusAsync(groupName, status);

        // Assert
        _mockHubClients.Verify(c => c.Group(groupName), Times.Once);
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChange",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    ReferenceEquals(args[0], status) &&
                    args[1] == null),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithErrorMessage_SendsNotificationWithErrorMessage()
    {
        // Arrange
        var groupName = "tournament-rules-refresh";
        var status = DocumentRefreshStatus.Failed;
        var errorMessage = "An error occurred";

        // Act
        await _notifier.NotifyStatusAsync(groupName, status, errorMessage);

        // Assert
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChange",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    ReferenceEquals(args[0], status) &&
                    args[1] as string == errorMessage),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithCancellationToken_PassesCancellationToken()
    {
        // Arrange
        var groupName = "bylaws-refresh";
        var status = DocumentRefreshStatus.Retrieving;
        var cancellationToken = new CancellationToken();

        // Act
        await _notifier.NotifyStatusAsync(groupName, status, cancellationToken: cancellationToken);

        // Assert
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChange",
                It.Is<object?[]>(args => args.Length == 2),
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData("Retrieving")]
    [InlineData("Uploading")]
    [InlineData("Completed")]
    [InlineData("Failed")]
    public async Task NotifyStatusAsync_WithDifferentStatuses_SendsCorrectStatus(string statusName)
    {
        // Arrange
        var groupName = "test-refresh";
        var status = DocumentRefreshStatus.FromName(statusName);

        // Act
        await _notifier.NotifyStatusAsync(groupName, status);

        // Assert
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChange",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    ReferenceEquals(args[0], status)),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithMultipleSequentialCalls_SendsAllNotifications()
    {
        // Arrange
        var groupName = "bylaws-refresh";

        // Act
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Retrieving);
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Uploading);
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChange",
                It.IsAny<object?[]>(),
                default),
            Times.Exactly(3));
    }

    [Fact]
    public async Task NotifyStatusAsync_WithDifferentGroupNames_SendsToCorrectGroups()
    {
        // Arrange
        var groupName1 = "bylaws-refresh";
        var groupName2 = "tournament-rules-refresh";

        // Act
        await _notifier.NotifyStatusAsync(groupName1, DocumentRefreshStatus.Completed);
        await _notifier.NotifyStatusAsync(groupName2, DocumentRefreshStatus.Failed, "Error occurred");

        // Assert
        _mockHubClients.Verify(c => c.Group(groupName1), Times.Once);
        _mockHubClients.Verify(c => c.Group(groupName2), Times.Once);
        _mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChange",
                It.IsAny<object?[]>(),
                default),
            Times.Exactly(2));
    }
}
