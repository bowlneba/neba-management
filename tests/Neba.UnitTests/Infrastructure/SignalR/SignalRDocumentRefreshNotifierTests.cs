using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neba.Application.Documents;
using Neba.Infrastructure.SignalR;

namespace Neba.UnitTests.Infrastructure.SignalR;

public sealed class SignalRDocumentRefreshNotifierTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<SignalRDocumentRefreshNotifier>> _mockLogger;
    private readonly SignalRDocumentRefreshNotifier _notifier;

    public SignalRDocumentRefreshNotifierTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<SignalRDocumentRefreshNotifier>>();

        // Setup IsEnabled to return true so LoggerMessage source-generated methods will call Log
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _notifier = new SignalRDocumentRefreshNotifier(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterHub_WithValidParameters_RegistersHub()
    {
        // Arrange
        var groupName = "test-group";

        // Act
        _notifier.RegisterHub<TestHub>(groupName);

        // Assert - Verify by attempting to notify (should not log unregistered message)
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // This should not log hub group not registered
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Completed);

        // Verify notification was sent (hub was registered)
        mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
                It.IsAny<object?[]>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task RegisterHub_WithMultipleHubs_RegistersAllHubs()
    {
        // Arrange & Act
        _notifier.RegisterHub<TestHub>("group1");
        _notifier.RegisterHub<AnotherTestHub>("group2");

        // Assert - Both should be registered
        var mockHubContext1 = SetupMockHubContext<TestHub>();
        var mockHubContext2 = SetupMockHubContext<AnotherTestHub>();
        var mockClientProxy1 = new Mock<IClientProxy>();
        var mockClientProxy2 = new Mock<IClientProxy>();

        mockHubContext1.Setup(h => h.Clients.Group("group1"))
            .Returns(mockClientProxy1.Object);
        mockHubContext2.Setup(h => h.Clients.Group("group2"))
            .Returns(mockClientProxy2.Object);

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext1.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<AnotherTestHub>)))
            .Returns(mockHubContext2.Object);

        await _notifier.NotifyStatusAsync("group1", DocumentRefreshStatus.Completed);
        await _notifier.NotifyStatusAsync("group2", DocumentRefreshStatus.Completed);

        // Verify both notifications were sent
        mockClientProxy1.Verify(
            cp => cp.SendCoreAsync("DocumentRefreshStatusChanged", It.IsAny<object?[]>(), default),
            Times.Once);
        mockClientProxy2.Verify(
            cp => cp.SendCoreAsync("DocumentRefreshStatusChanged", It.IsAny<object?[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task RegisterHub_WithSameGroupNameTwice_ReplacesRegistration()
    {
        // Arrange
        var groupName = "test-group";

        // Act
        _notifier.RegisterHub<TestHub>(groupName);
        _notifier.RegisterHub<AnotherTestHub>(groupName);

        // Assert - Should use the last registered hub type
        var mockHubContext = SetupMockHubContext<AnotherTestHub>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<AnotherTestHub>)))
            .Returns(mockHubContext.Object);

        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Completed);

        // Verify notification was sent using the last registered hub
        mockClientProxy.Verify(
            cp => cp.SendCoreAsync("DocumentRefreshStatusChanged", It.IsAny<object?[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithNullHubGroupName_ReturnsEarlyAndLogs()
    {
        // Arrange
        string? hubGroupName = null;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockServiceProvider.Verify(sp => sp.GetService(It.IsAny<Type>()), Times.Never);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithEmptyHubGroupName_ReturnsEarlyAndLogs()
    {
        // Arrange
        var hubGroupName = string.Empty;

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockServiceProvider.Verify(sp => sp.GetService(It.IsAny<Type>()), Times.Never);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithWhitespaceHubGroupName_ReturnsEarlyAndLogs()
    {
        // Arrange
        var hubGroupName = "   ";

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockServiceProvider.Verify(sp => sp.GetService(It.IsAny<Type>()), Times.Never);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithUnregisteredHubGroup_ReturnsEarlyAndLogs()
    {
        // Arrange
        var hubGroupName = "unregistered-group";

        // Act
        await _notifier.NotifyStatusAsync(hubGroupName, DocumentRefreshStatus.Completed);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockServiceProvider.Verify(sp => sp.GetService(It.IsAny<Type>()), Times.Never);
    }

    [Fact]
    public async Task NotifyStatusAsync_WhenHubContextIsNull_ReturnsEarly()
    {
        // Arrange
        var groupName = "test-group";
        _notifier.RegisterHub<TestHub>(groupName);

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns((object?)null);

        // Act
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Completed);

        // Assert - Should not throw, just return early
        _mockServiceProvider.Verify(sp => sp.GetService(typeof(IHubContext<TestHub>)), Times.Once);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithRegisteredHub_SendsNotification()
    {
        // Arrange
        var groupName = "test-group";
        var status = DocumentRefreshStatus.Completed;
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);

        _notifier.RegisterHub<TestHub>(groupName);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // Act
        await _notifier.NotifyStatusAsync(groupName, status);

        // Assert
        mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
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
        var groupName = "test-group";
        var status = DocumentRefreshStatus.Failed;
        var errorMessage = "An error occurred";
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);

        _notifier.RegisterHub<TestHub>(groupName);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // Act
        await _notifier.NotifyStatusAsync(groupName, status, errorMessage);

        // Assert
        mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
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
        var groupName = "test-group";
        var status = DocumentRefreshStatus.Retrieving;
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy = new Mock<IClientProxy>();
        var cancellationToken = new CancellationToken();

        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);

        _notifier.RegisterHub<TestHub>(groupName);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // Act
        await _notifier.NotifyStatusAsync(groupName, status, cancellationToken: cancellationToken);

        // Assert
        mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
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
        var groupName = "test-group";
        var status = DocumentRefreshStatus.FromName(statusName);
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);

        _notifier.RegisterHub<TestHub>(groupName);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // Act
        await _notifier.NotifyStatusAsync(groupName, status);

        // Assert
        mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    ReferenceEquals(args[0], status)),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyStatusAsync_WhenSendAsyncThrowsException_LogsAndRethrows()
    {
        // Arrange
        var groupName = "test-group";
        var status = DocumentRefreshStatus.Failed;
        var exception = new InvalidOperationException("SignalR error");
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);

        mockClientProxy.Setup(cp => cp.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        _notifier.RegisterHub<TestHub>(groupName);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // Act & Assert
        var thrownException = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _notifier.NotifyStatusAsync(groupName, status));

        thrownException.ShouldBe(exception);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyStatusAsync_WithMultipleSequentialCalls_SendsAllNotifications()
    {
        // Arrange
        var groupName = "test-group";
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients.Group(groupName))
            .Returns(mockClientProxy.Object);

        _notifier.RegisterHub<TestHub>(groupName);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // Act
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Retrieving);
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Uploading);
        await _notifier.NotifyStatusAsync(groupName, DocumentRefreshStatus.Completed);

        // Assert
        mockClientProxy.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
                It.IsAny<object?[]>(),
                default),
            Times.Exactly(3));
    }

    [Fact]
    public async Task NotifyStatusAsync_WithDifferentGroupNames_SendsToCorrectGroups()
    {
        // Arrange
        var groupName1 = "group1";
        var groupName2 = "group2";
        var mockHubContext = SetupMockHubContext<TestHub>();
        var mockClientProxy1 = new Mock<IClientProxy>();
        var mockClientProxy2 = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients.Group(groupName1))
            .Returns(mockClientProxy1.Object);
        mockHubContext.Setup(h => h.Clients.Group(groupName2))
            .Returns(mockClientProxy2.Object);

        _notifier.RegisterHub<TestHub>(groupName1);
        _notifier.RegisterHub<TestHub>(groupName2);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IHubContext<TestHub>)))
            .Returns(mockHubContext.Object);

        // Act
        await _notifier.NotifyStatusAsync(groupName1, DocumentRefreshStatus.Completed);
        await _notifier.NotifyStatusAsync(groupName2, DocumentRefreshStatus.Failed);

        // Assert
        mockClientProxy1.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
                It.Is<object?[]>(args => ReferenceEquals(args[0], DocumentRefreshStatus.Completed)),
                default),
            Times.Once);

        mockClientProxy2.Verify(
            cp => cp.SendCoreAsync(
                "DocumentRefreshStatusChanged",
                It.Is<object?[]>(args => ReferenceEquals(args[0], DocumentRefreshStatus.Failed)),
                default),
            Times.Once);
    }

    private static Mock<IHubContext<THub>> SetupMockHubContext<THub>()
        where THub : Hub
    {
        var mockHubContext = new Mock<IHubContext<THub>>();
        var mockHubClients = new Mock<IHubClients>();
        mockHubContext.Setup(h => h.Clients).Returns(mockHubClients.Object);
        return mockHubContext;
    }

    // Test hub classes for testing purposes
#pragma warning disable CA1034 // Nested types should not be visible - test helper classes
    public sealed class TestHub : Hub { }
    public sealed class AnotherTestHub : Hub { }
#pragma warning restore CA1034
}
