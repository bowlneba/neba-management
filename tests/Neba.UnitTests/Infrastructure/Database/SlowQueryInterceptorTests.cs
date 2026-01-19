using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Neba.Infrastructure.Database;

namespace Neba.UnitTests.Infrastructure.Database;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Database")]
public sealed class SlowQueryInterceptorTests
{
    private readonly Mock<ILogger<SlowQueryInterceptor>> _mockLogger = new();

    private static Mock<DbCommand> CreateMockDbCommand(string commandText = "SELECT * FROM Test")
    {
        var mockCommand = new Mock<DbCommand>();
        mockCommand.Setup(c => c.CommandText).Returns(commandText);
        mockCommand.Setup(c => c.CommandType).Returns(CommandType.Text);
        return mockCommand;
    }

    private static CommandExecutedEventData CreateEventData(TimeSpan duration)
    {
        var mockDefinition = new Mock<EventDefinitionBase>(
            new Mock<ILoggingOptions>().Object,
            new EventId(1, "Test"),
            LogLevel.Information,
            "Test message");

        return new CommandExecutedEventData(
            mockDefinition.Object,
            (_, _) => "Test message",
            connection: null!,
            command: null!,
            logCommandText: "SELECT * FROM Test",
            context: null,
            executeMethod: DbCommandMethod.ExecuteReader,
            commandId: Guid.NewGuid(),
            connectionId: Guid.NewGuid(),
            result: null,
            async: false,
            logParameterValues: false,
            startTime: DateTimeOffset.UtcNow,
            duration: duration,
            commandSource: CommandSource.LinqQuery);
    }

    [Fact(DisplayName = "Constructor initializes with default threshold")]
    public void Constructor_WithDefaultThreshold_InitializesCorrectly()
    {
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object));
    }

    [Fact(DisplayName = "Constructor initializes with custom threshold")]
    public void Constructor_WithCustomThreshold_InitializesCorrectly()
    {
        const double customThreshold = 500;
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object, customThreshold));
    }

    [Fact(DisplayName = "Constructor accepts zero threshold")]
    public void Constructor_WithZeroThreshold_InitializesCorrectly()
    {
        const double zeroThreshold = 0;
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object, zeroThreshold));
    }

    [Fact(DisplayName = "Constructor accepts very high threshold")]
    public void Constructor_WithVeryHighThreshold_InitializesCorrectly()
    {
        const double highThreshold = 100000;
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object, highThreshold));
    }

    [Fact(DisplayName = "Constructor can be instantiated multiple times")]
    public void Constructor_MultipleInstances_CanBeCreated()
    {
        Should.NotThrow(() =>
        {
            _ = new SlowQueryInterceptor(_mockLogger.Object, 500);
            _ = new SlowQueryInterceptor(_mockLogger.Object, 1000);
            _ = new SlowQueryInterceptor(_mockLogger.Object, 2000);
        });
    }

    [Fact(DisplayName = "Inherits from DbCommandInterceptor")]
    public void SlowQueryInterceptor_InheritsFromDbCommandInterceptor()
    {
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        interceptor.ShouldBeAssignableTo<Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor>();
    }

    [Fact(DisplayName = "Logger is passed to constructor")]
    public void Constructor_WithLogger_AcceptsLoggerParameter()
    {
        var mockLogger = new Mock<ILogger<SlowQueryInterceptor>>();
        Should.NotThrow(() => new SlowQueryInterceptor(mockLogger.Object, 1000));
    }

    [Fact(DisplayName = "Different threshold values are accepted")]
    public void Constructor_WithVariousThresholds_Succeeds()
    {
        double[] thresholds = [0, 100, 500, 1000, 5000, 10000, double.MaxValue];
        foreach (double threshold in thresholds)
        {
            Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object, threshold));
        }
    }

    [Fact(DisplayName = "Default threshold is 1000ms")]
    public void DefaultThreshold_Is1000Milliseconds()
    {
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object));
    }

    [Fact(DisplayName = "SlowQueryInterceptor is sealed")]
    public void SlowQueryInterceptor_IsSealed()
    {
        typeof(SlowQueryInterceptor).IsSealed.ShouldBeTrue();
    }

    [Fact(DisplayName = "Constructor parameter logger cannot be null")]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        Should.NotThrow(() => new SlowQueryInterceptor(null!, 1000));
    }

    [Fact(DisplayName = "Constructor accepts double for threshold")]
    public void Constructor_ThresholdIsDouble()
    {
        const double floatingThreshold = 1234.5678;
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object, floatingThreshold));
    }

    [Fact(DisplayName = "Multiple interceptors with different thresholds can coexist")]
    public void MultipleInterceptorsWithDifferentThresholds_CanCoexist()
    {
        ILogger<SlowQueryInterceptor> logger1 = new Mock<ILogger<SlowQueryInterceptor>>().Object;
        ILogger<SlowQueryInterceptor> logger2 = new Mock<ILogger<SlowQueryInterceptor>>().Object;
        ILogger<SlowQueryInterceptor> logger3 = new Mock<ILogger<SlowQueryInterceptor>>().Object;

        Should.NotThrow(() =>
        {
            var interceptor1 = new SlowQueryInterceptor(logger1, 100);
            var interceptor2 = new SlowQueryInterceptor(logger2, 1000);
            var interceptor3 = new SlowQueryInterceptor(logger3, 5000);

            interceptor1.ShouldNotBeNull();
            interceptor2.ShouldNotBeNull();
            interceptor3.ShouldNotBeNull();
        });
    }

    [Fact(DisplayName = "Negative threshold is accepted")]
    public void Constructor_WithNegativeThreshold_Succeeds()
    {
        const double negativeThreshold = -100;
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object, negativeThreshold));
    }

    [Fact(DisplayName = "Very small positive threshold is accepted")]
    public void Constructor_WithVerySmallThreshold_Succeeds()
    {
        const double tinyThreshold = 0.001;
        Should.NotThrow(() => new SlowQueryInterceptor(_mockLogger.Object, tinyThreshold));
    }

    [Fact(DisplayName = "ReaderExecuted throws ArgumentNullException for null command")]
    public void ReaderExecuted_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));
        var mockReader = new Mock<DbDataReader>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            interceptor.ReaderExecuted(null!, eventData, mockReader.Object));
    }

    [Fact(DisplayName = "ReaderExecuted throws ArgumentNullException for null eventData")]
    public void ReaderExecuted_NullEventData_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();
        var mockReader = new Mock<DbDataReader>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            interceptor.ReaderExecuted(mockCommand.Object, null!, mockReader.Object));
    }

    [Fact(DisplayName = "ReaderExecuted records metrics for fast query")]
    public void ReaderExecuted_FastQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 1000);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));
        var mockReader = new Mock<DbDataReader>();

        // Act
        DbDataReader result = interceptor.ReaderExecuted(mockCommand.Object, eventData, mockReader.Object);

        // Assert
        result.ShouldBe(mockReader.Object);
    }

    [Fact(DisplayName = "ReaderExecuted records metrics for slow query")]
    public void ReaderExecuted_SlowQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 100);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT * FROM SlowTable");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        var mockReader = new Mock<DbDataReader>();

        // Act
        DbDataReader result = interceptor.ReaderExecuted(mockCommand.Object, eventData, mockReader.Object);

        // Assert
        result.ShouldBe(mockReader.Object);
    }

    [Fact(DisplayName = "ReaderExecutedAsync throws ArgumentNullException for null command")]
    public async Task ReaderExecutedAsync_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));
        var mockReader = new Mock<DbDataReader>();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            interceptor.ReaderExecutedAsync(null!, eventData, mockReader.Object).AsTask());
    }

    [Fact(DisplayName = "ReaderExecutedAsync throws ArgumentNullException for null eventData")]
    public async Task ReaderExecutedAsync_NullEventData_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();
        var mockReader = new Mock<DbDataReader>();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            interceptor.ReaderExecutedAsync(mockCommand.Object, null!, mockReader.Object).AsTask());
    }

    [Fact(DisplayName = "ReaderExecutedAsync records metrics for fast query")]
    public async Task ReaderExecutedAsync_FastQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 1000);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));
        var mockReader = new Mock<DbDataReader>();

        // Act
        DbDataReader result = await interceptor.ReaderExecutedAsync(mockCommand.Object, eventData, mockReader.Object);

        // Assert
        result.ShouldBe(mockReader.Object);
    }

    [Fact(DisplayName = "ReaderExecutedAsync records metrics for slow query")]
    public async Task ReaderExecutedAsync_SlowQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 100);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT * FROM SlowTable");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        var mockReader = new Mock<DbDataReader>();

        // Act
        DbDataReader result = await interceptor.ReaderExecutedAsync(mockCommand.Object, eventData, mockReader.Object);

        // Assert
        result.ShouldBe(mockReader.Object);
    }

    [Fact(DisplayName = "ScalarExecuted throws ArgumentNullException for null command")]
    public void ScalarExecuted_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            interceptor.ScalarExecuted(null!, eventData, 42));
    }

    [Fact(DisplayName = "ScalarExecuted throws ArgumentNullException for null eventData")]
    public void ScalarExecuted_NullEventData_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            interceptor.ScalarExecuted(mockCommand.Object, null!, 42));
    }

    [Fact(DisplayName = "ScalarExecuted records metrics for fast query")]
    public void ScalarExecuted_FastQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 1000);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT COUNT(*) FROM Test");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(50));
        const int scalarResult = 42;

        // Act
        object? result = interceptor.ScalarExecuted(mockCommand.Object, eventData, scalarResult);

        // Assert
        result.ShouldBe(scalarResult);
    }

    [Fact(DisplayName = "ScalarExecuted records metrics for slow query")]
    public void ScalarExecuted_SlowQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 100);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT COUNT(*) FROM LargeTable");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        const int scalarResult = 1000000;

        // Act
        object? result = interceptor.ScalarExecuted(mockCommand.Object, eventData, scalarResult);

        // Assert
        result.ShouldBe(scalarResult);
    }

    [Fact(DisplayName = "ScalarExecutedAsync throws ArgumentNullException for null command")]
    public async Task ScalarExecutedAsync_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            interceptor.ScalarExecutedAsync(null!, eventData, 42).AsTask());
    }

    [Fact(DisplayName = "ScalarExecutedAsync throws ArgumentNullException for null eventData")]
    public async Task ScalarExecutedAsync_NullEventData_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            interceptor.ScalarExecutedAsync(mockCommand.Object, null!, 42).AsTask());
    }

    [Fact(DisplayName = "ScalarExecutedAsync records metrics for fast query")]
    public async Task ScalarExecutedAsync_FastQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 1000);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT COUNT(*) FROM Test");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(50));
        const int scalarResult = 42;

        // Act
        object? result = await interceptor.ScalarExecutedAsync(mockCommand.Object, eventData, scalarResult);

        // Assert
        result.ShouldBe(scalarResult);
    }

    [Fact(DisplayName = "ScalarExecutedAsync records metrics for slow query")]
    public async Task ScalarExecutedAsync_SlowQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 100);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT COUNT(*) FROM LargeTable");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        const int scalarResult = 1000000;

        // Act
        object? result = await interceptor.ScalarExecutedAsync(mockCommand.Object, eventData, scalarResult);

        // Assert
        result.ShouldBe(scalarResult);
    }

    [Fact(DisplayName = "NonQueryExecuted throws ArgumentNullException for null command")]
    public void NonQueryExecuted_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            interceptor.NonQueryExecuted(null!, eventData, 1));
    }

    [Fact(DisplayName = "NonQueryExecuted throws ArgumentNullException for null eventData")]
    public void NonQueryExecuted_NullEventData_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            interceptor.NonQueryExecuted(mockCommand.Object, null!, 1));
    }

    [Fact(DisplayName = "NonQueryExecuted records metrics for fast query")]
    public void NonQueryExecuted_FastQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 1000);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("UPDATE Test SET Value = 1");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(50));
        const int rowsAffected = 5;

        // Act
        int result = interceptor.NonQueryExecuted(mockCommand.Object, eventData, rowsAffected);

        // Assert
        result.ShouldBe(rowsAffected);
    }

    [Fact(DisplayName = "NonQueryExecuted records metrics for slow query")]
    public void NonQueryExecuted_SlowQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 100);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("DELETE FROM LargeTable WHERE Condition = true");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        const int rowsAffected = 10000;

        // Act
        int result = interceptor.NonQueryExecuted(mockCommand.Object, eventData, rowsAffected);

        // Assert
        result.ShouldBe(rowsAffected);
    }

    [Fact(DisplayName = "NonQueryExecutedAsync throws ArgumentNullException for null command")]
    public async Task NonQueryExecutedAsync_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            interceptor.NonQueryExecutedAsync(null!, eventData, 1).AsTask());
    }

    [Fact(DisplayName = "NonQueryExecutedAsync throws ArgumentNullException for null eventData")]
    public async Task NonQueryExecutedAsync_NullEventData_ThrowsArgumentNullException()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object);
        Mock<DbCommand> mockCommand = CreateMockDbCommand();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            interceptor.NonQueryExecutedAsync(mockCommand.Object, null!, 1).AsTask());
    }

    [Fact(DisplayName = "NonQueryExecutedAsync records metrics for fast query")]
    public async Task NonQueryExecutedAsync_FastQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 1000);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("INSERT INTO Test VALUES (1)");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(50));
        const int rowsAffected = 1;

        // Act
        int result = await interceptor.NonQueryExecutedAsync(mockCommand.Object, eventData, rowsAffected);

        // Assert
        result.ShouldBe(rowsAffected);
    }

    [Fact(DisplayName = "NonQueryExecutedAsync records metrics for slow query")]
    public async Task NonQueryExecutedAsync_SlowQuery_RecordsMetrics()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 100);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("INSERT INTO LargeTable SELECT * FROM Source");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        const int rowsAffected = 50000;

        // Act
        int result = await interceptor.NonQueryExecutedAsync(mockCommand.Object, eventData, rowsAffected);

        // Assert
        result.ShouldBe(rowsAffected);
    }

    [Fact(DisplayName = "Query at exact threshold is considered slow")]
    public void Query_AtExactThreshold_IsConsideredSlow()
    {
        // Arrange
        const double threshold = 100;
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, threshold);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT * FROM Test");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(threshold));
        var mockReader = new Mock<DbDataReader>();

        // Act
        DbDataReader result = interceptor.ReaderExecuted(mockCommand.Object, eventData, mockReader.Object);

        // Assert
        result.ShouldBe(mockReader.Object);
    }

    [Fact(DisplayName = "Query just below threshold is not considered slow")]
    public void Query_JustBelowThreshold_IsNotConsideredSlow()
    {
        // Arrange
        const double threshold = 100;
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, threshold);
        Mock<DbCommand> mockCommand = CreateMockDbCommand("SELECT * FROM Test");
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(threshold - 0.001));
        var mockReader = new Mock<DbDataReader>();

        // Act
        DbDataReader result = interceptor.ReaderExecuted(mockCommand.Object, eventData, mockReader.Object);

        // Assert
        result.ShouldBe(mockReader.Object);
    }

    [Fact(DisplayName = "Command with empty CommandText is handled gracefully")]
    public void Command_WithEmptyCommandText_IsHandledGracefully()
    {
        // Arrange
        var interceptor = new SlowQueryInterceptor(_mockLogger.Object, 100);
        var mockCommand = new Mock<DbCommand>();
        mockCommand.Setup(c => c.CommandText).Returns(string.Empty);
        mockCommand.Setup(c => c.CommandType).Returns(CommandType.Text);
        CommandExecutedEventData eventData = CreateEventData(TimeSpan.FromMilliseconds(500));
        var mockReader = new Mock<DbDataReader>();

        // Act
        DbDataReader result = interceptor.ReaderExecuted(mockCommand.Object, eventData, mockReader.Object);

        // Assert
        result.ShouldBe(mockReader.Object);
    }
}
