using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Neba.Api.ErrorHandling;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Neba.UnitTests.ErrorHandling;

[
    Trait("Category", "Unit"),
    Trait("Component", "ErrorHandling")
]
public sealed class GlobalExceptionHandlerTests : IDisposable
{
    private const string GenericDetail = "An error occurred while processing your request";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly ServiceProvider _serviceProvider;
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _serviceProvider = BuildServiceProvider();
        _problemDetailsService = _serviceProvider.GetRequiredService<IProblemDetailsService>();
        _sut = new GlobalExceptionHandler(_problemDetailsService, NullLogger<GlobalExceptionHandler>.Instance);
    }

    [Fact(DisplayName = "Sets 500 status code for unhandled exceptions")]
    public async Task TryHandleAsync_WhenExceptionThrown_SetsInternalServerErrorStatusCode()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact(DisplayName = "Writes RFC 9457 title")]
    public async Task TryHandleAsync_WhenExceptionThrown_WritesProblemDetailsTitle()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Title.ShouldBe("An unexpected error occurred.");
    }

    [Fact(DisplayName = "Returns generic detail instead of exception message")]
    public async Task TryHandleAsync_WhenExceptionThrown_ReturnsGenericDetail()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong in the application");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe(GenericDetail);
    }

    [Fact(DisplayName = "Serializes 500 into problem details")]
    public async Task TryHandleAsync_WhenExceptionThrown_ReturnsInternalServerErrorStatusInBody()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact(DisplayName = "Returns true after writing problem details")]
    public async Task TryHandleAsync_WhenExceptionThrown_ReturnsTrue()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        bool result = await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact(DisplayName = "Handles argument null exceptions with generic response")]
    public async Task TryHandleAsync_WhenArgumentNullExceptionThrown_WritesGenericProblemDetails()
    {
        // Arrange
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3928 // The parameter name is not declared in the argument list
        var exception = new ArgumentNullException("param", "Parameter cannot be null");
#pragma warning restore S3928, CA2208
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        bool result = await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        result.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe(GenericDetail);
    }

    [Fact(DisplayName = "Handles invalid operation exceptions with generic response")]
    public async Task TryHandleAsync_WhenInvalidOperationExceptionThrown_WritesGenericProblemDetails()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation performed");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        bool result = await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        result.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe(GenericDetail);
    }

    [Fact(DisplayName = "Handles exceptions with inner exceptions generically")]
    public async Task TryHandleAsync_WhenInnerExceptionsPresent_WritesGenericProblemDetails()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new InvalidOperationException("Outer error", innerException);
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        bool result = await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        result.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe(GenericDetail);
    }
    
    [Fact(DisplayName = "Writes application/problem+json body")]
    public async Task TryHandleAsync_WhenExceptionThrown_WritesProblemDetailsAsJson()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        string responseText = await reader.ReadToEndAsync();
        responseText.ShouldNotBeNullOrWhiteSpace();
        httpContext.Response.ContentType.ShouldStartWith("application/problem+json");

        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseText, JsonOptions);

        problemDetails.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Handles multiple consecutive exceptions consistently")]
    public async Task TryHandleAsync_WhenInvokedMultipleTimes_WritesProblemDetailsEachTime()
    {
        // Arrange
        var exception1 = new InvalidOperationException("First error");
        var exception2 = new InvalidOperationException("Second error");
        DefaultHttpContext httpContext1 = CreateHttpContext();
        DefaultHttpContext httpContext2 = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        bool result1 = await _sut.TryHandleAsync(httpContext1, exception1, cancellationToken);
        bool result2 = await _sut.TryHandleAsync(httpContext2, exception2, cancellationToken);

        // Assert
        result1.ShouldBeTrue();
        result2.ShouldBeTrue();

        ProblemDetails problemDetails1 = await DeserializeProblemDetails(httpContext1);
        ProblemDetails problemDetails2 = await DeserializeProblemDetails(httpContext2);

        problemDetails1.Detail.ShouldBe(GenericDetail);
        problemDetails2.Detail.ShouldBe(GenericDetail);
    }

    [Fact(DisplayName = "Includes trace id extension from trace identifier")]
    public async Task TryHandleAsync_WhenExceptionThrown_IncludesTraceIdExtension()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();

        problemDetails.Extensions.TryGetValue("traceId", out object? traceIdValue).ShouldBeTrue();
        traceIdValue.ShouldNotBeNull();

        string? traceId = traceIdValue switch
        {
            JsonElement jsonElement => jsonElement.GetString(),
            JsonValue jsonValue => jsonValue.GetValue<string>(),
            string str => str,
            _ => null,
        };

        traceId.ShouldBe(httpContext.TraceIdentifier);
    }

    [Fact(DisplayName = "Sets RFC 9457 type and instance")]
    public async Task TryHandleAsync_WhenExceptionThrown_SetsProblemDetailsTypeAndInstance()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Type.ShouldBe("https://datatracker.ietf.org/doc/html/rfc9457");
        problemDetails.Instance.ShouldBe(httpContext.Request.Path);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    private DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext httpContext = new()
        {
            RequestServices = _serviceProvider,
            TraceIdentifier = Guid.NewGuid().ToString(),
        };

        httpContext.Request.Path = "/test";
        httpContext.Response.Body = new MemoryStream();

        return httpContext;
    }

    private static async Task<ProblemDetails> DeserializeProblemDetails(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        string responseText = await reader.ReadToEndAsync();

        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseText, JsonOptions);

        return problemDetails ?? throw new InvalidOperationException("Failed to deserialize ProblemDetails");
    }

    private static ServiceProvider BuildServiceProvider()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddProblemDetails();

        return services.BuildServiceProvider();
    }
}
