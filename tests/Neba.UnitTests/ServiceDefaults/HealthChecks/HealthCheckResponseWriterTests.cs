using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neba.ServiceDefaults.HealthChecks;

namespace Neba.UnitTests.ServiceDefaults.HealthChecks;

[Trait("Category", "Unit")]
[Trait("Component", "ServiceDefaults.HealthChecks")]
public sealed class HealthCheckResponseWriterTests
{
    [Fact(DisplayName = "Default returns a response writer delegate")]
    public void Default_ReturnsResponseWriterDelegate()
    {
        // Act
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();

        // Assert
        writer.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Default writer sets content type to application/json")]
    public async Task Default_SetsContentTypeToApplicationJson()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();
        HealthReport healthReport = CreateHealthReport(HealthStatus.Healthy);

        // Act
        await writer(httpContext, healthReport);

        // Assert
        httpContext.Response.ContentType.ShouldStartWith("application/json");
    }

    [Fact(DisplayName = "Default writer writes healthy status correctly")]
    public async Task Default_WritesHealthyStatus()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReport(HealthStatus.Healthy);

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        json.RootElement.GetProperty("status").GetString().ShouldBe("Healthy");
    }

    [Fact(DisplayName = "Default writer writes unhealthy status correctly")]
    public async Task Default_WritesUnhealthyStatus()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReport(HealthStatus.Unhealthy);

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        json.RootElement.GetProperty("status").GetString().ShouldBe("Unhealthy");
    }

    [Fact(DisplayName = "Default writer writes degraded status correctly")]
    public async Task Default_WritesDegradedStatus()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReport(HealthStatus.Degraded);

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        json.RootElement.GetProperty("status").GetString().ShouldBe("Degraded");
    }

    [Fact(DisplayName = "Default writer includes totalDuration in response")]
    public async Task Default_IncludesTotalDurationInResponse()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReport(HealthStatus.Healthy);

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        json.RootElement.TryGetProperty("totalDuration", out _).ShouldBeTrue();
    }

    [Fact(DisplayName = "Default writer includes checks array in response")]
    public async Task Default_IncludesChecksArrayInResponse()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReportWithEntries();

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        json.RootElement.TryGetProperty("checks", out JsonElement checks).ShouldBeTrue();
        checks.ValueKind.ShouldBe(JsonValueKind.Array);
    }

    [Fact(DisplayName = "Default writer includes check details in response")]
    public async Task Default_IncludesCheckDetailsInResponse()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReportWithEntries();

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        JsonElement checks = json.RootElement.GetProperty("checks");
        checks.GetArrayLength().ShouldBe(2);

        // Find the database check (order may vary in dictionary iteration)
        bool foundDatabase = false;
        foreach (JsonElement check in checks.EnumerateArray())
        {
            if (check.GetProperty("name").GetString() == "database")
            {
                foundDatabase = true;
                check.GetProperty("status").GetString().ShouldBe("Healthy");
                check.GetProperty("description").GetString().ShouldBe("Database is healthy");
                break;
            }
        }
        foundDatabase.ShouldBeTrue();
    }

    [Fact(DisplayName = "Default writer includes exception message when present")]
    public async Task Default_IncludesExceptionMessageWhenPresent()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReportWithException();

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        JsonElement checks = json.RootElement.GetProperty("checks");
        JsonElement failedCheck = checks[0];
        failedCheck.GetProperty("exception").GetString().ShouldBe("Connection refused");
    }

    [Fact(DisplayName = "Default writer includes data dictionary when present")]
    public async Task Default_IncludesDataDictionaryWhenPresent()
    {
        // Arrange
        Func<HttpContext, HealthReport, Task> writer = HealthCheckResponseWriter.Default();
        DefaultHttpContext httpContext = new();
        var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        HealthReport healthReport = CreateHealthReportWithData();

        // Act
        await writer(httpContext, healthReport);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        string responseContent = await reader.ReadToEndAsync();
        using JsonDocument json = JsonDocument.Parse(responseContent);
        JsonElement checks = json.RootElement.GetProperty("checks");
        JsonElement checkWithData = checks[0];
        checkWithData.TryGetProperty("data", out JsonElement data).ShouldBeTrue();
        data.TryGetProperty("version", out _).ShouldBeTrue();
    }

    private static HealthReport CreateHealthReport(HealthStatus status)
    {
        var entries = new Dictionary<string, HealthReportEntry>();
        return new HealthReport(entries, status, TimeSpan.FromMilliseconds(50));
    }

    private static HealthReport CreateHealthReportWithEntries()
    {
        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new HealthReportEntry(
                HealthStatus.Healthy,
                "Database is healthy",
                TimeSpan.FromMilliseconds(10),
                exception: null,
                data: null),
            ["cache"] = new HealthReportEntry(
                HealthStatus.Healthy,
                "Cache is healthy",
                TimeSpan.FromMilliseconds(5),
                exception: null,
                data: null)
        };
        return new HealthReport(entries, HealthStatus.Healthy, TimeSpan.FromMilliseconds(15));
    }

    private static HealthReport CreateHealthReportWithException()
    {
        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new HealthReportEntry(
                HealthStatus.Unhealthy,
                "Database is unhealthy",
                TimeSpan.FromMilliseconds(1000),
                exception: new InvalidOperationException("Connection refused"),
                data: null)
        };
        return new HealthReport(entries, HealthStatus.Unhealthy, TimeSpan.FromMilliseconds(1000));
    }

    private static HealthReport CreateHealthReportWithData()
    {
        var data = new Dictionary<string, object>
        {
            ["version"] = "1.0.0",
            ["connections"] = 5
        };
        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["service"] = new HealthReportEntry(
                HealthStatus.Healthy,
                "Service is healthy",
                TimeSpan.FromMilliseconds(10),
                exception: null,
                data: data)
        };
        return new HealthReport(entries, HealthStatus.Healthy, TimeSpan.FromMilliseconds(10));
    }
}
