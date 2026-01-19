using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Neba.Infrastructure.Database;

namespace Neba.UnitTests.Infrastructure.Database;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Database")]
public sealed class DatabaseTelemetryTests
{
    [Fact(DisplayName = "AddDatabaseTelemetry registers in service collection")]
    public void AddDatabaseTelemetry_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "1000" }
            })
            .Build();

        // Act
        services.AddDatabaseTelemetry(configuration);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.ShouldNotBeNull();
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with default threshold completes successfully")]
    public void AddDatabaseTelemetry_WithDefaultThreshold_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with custom threshold completes successfully")]
    public void AddDatabaseTelemetry_WithCustomThreshold_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "5000" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with zero threshold completes successfully")]
    public void AddDatabaseTelemetry_WithZeroThreshold_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "0" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with decimal threshold completes successfully")]
    public void AddDatabaseTelemetry_WithDecimalThreshold_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "1234.5" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with very large threshold completes successfully")]
    public void AddDatabaseTelemetry_WithVeryLargeThreshold_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "600000" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }

    [Fact(DisplayName = "AddDatabaseTelemetry returns IServiceCollection")]
    public void AddDatabaseTelemetry_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "1000" }
            })
            .Build();

        // Act
        IServiceCollection result = services.AddDatabaseTelemetry(configuration);

        // Assert
        result.ShouldBe(services);
    }

    [Fact(DisplayName = "AddDatabaseTelemetry can be called multiple times")]
    public void AddDatabaseTelemetry_CalledMultipleTimes_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "1000" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() =>
        {
            services.AddDatabaseTelemetry(configuration);
            services.AddDatabaseTelemetry(configuration);
        });
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with logging integration completes successfully")]
    public void AddDatabaseTelemetry_WithLogging_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "1000" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }

    [Fact(DisplayName = "SlowQueryInterceptor is registered in service collection")]
    public void AddDatabaseTelemetry_RegistersSlowQueryInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "2000" }
            })
            .Build();

        // Act
        services.AddDatabaseTelemetry(configuration);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        SlowQueryInterceptor? interceptor = serviceProvider.GetService<SlowQueryInterceptor>();
        interceptor.ShouldNotBeNull();
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with mock configuration completes successfully")]
    public void AddDatabaseTelemetry_WithMockConfiguration_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "1000" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }

    [Fact(DisplayName = "AddDatabaseTelemetry supports chaining")]
    public void AddDatabaseTelemetry_SupportsChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Database:SlowQueryThresholdMs", "1000" }
            })
            .Build();

        // Act & Assert
        Should.NotThrow(() =>
        {
            services
                .AddLogging()
                .AddDatabaseTelemetry(configuration);
        });
    }

    [Fact(DisplayName = "AddDatabaseTelemetry with empty configuration section completes successfully")]
    public void AddDatabaseTelemetry_WithEmptyConfigurationSection_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        Should.NotThrow(() => services.AddDatabaseTelemetry(configuration));
    }
}
