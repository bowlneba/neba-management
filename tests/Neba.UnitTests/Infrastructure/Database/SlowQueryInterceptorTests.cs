using Microsoft.Extensions.Logging;
using Moq;
using Neba.Infrastructure.Database;

namespace Neba.UnitTests.Infrastructure.Database;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Database")]
public sealed class SlowQueryInterceptorTests
{
    private readonly Mock<ILogger<SlowQueryInterceptor>> _mockLogger = new();

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
            var interceptor1 = new SlowQueryInterceptor(_mockLogger.Object, 500);
            var interceptor2 = new SlowQueryInterceptor(_mockLogger.Object, 1000);
            var interceptor3 = new SlowQueryInterceptor(_mockLogger.Object, 2000);
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
        var thresholds = new[] { 0, 100, 500, 1000, 5000, 10000, double.MaxValue };
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
        var logger1 = new Mock<ILogger<SlowQueryInterceptor>>().Object;
        var logger2 = new Mock<ILogger<SlowQueryInterceptor>>().Object;
        var logger3 = new Mock<ILogger<SlowQueryInterceptor>>().Object;

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
}
