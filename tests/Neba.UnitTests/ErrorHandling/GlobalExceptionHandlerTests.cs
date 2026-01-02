using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Neba.Api.ErrorHandling;
using System.Text.Json;

namespace Neba.UnitTests.ErrorHandling;

[Trait("Category", "Unit")]
[Trait("Component", "ErrorHandling")]
public sealed class GlobalExceptionHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _sut = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetStatusCodeTo500()
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

    [Fact]
    public async Task TryHandleAsync_ShouldReturnProblemDetailsWithCorrectTitle()
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

    [Fact]
    public async Task TryHandleAsync_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        const string exceptionMessage = "Something went wrong in the application";
        var exception = new InvalidOperationException(exceptionMessage);
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe(exceptionMessage);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnProblemDetailsWithStatus500()
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

    [Fact]
    public async Task TryHandleAsync_ShouldReturnTrue()
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

    [Fact]
    public async Task TryHandleAsync_ShouldHandleArgumentNullException()
    {
        // Arrange
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3928 // The parameter name is not declared in the argument list
        var exception = new ArgumentNullException("param", "Parameter cannot be null");
#pragma warning restore S3928
#pragma warning restore CA2208
        DefaultHttpContext httpContext = CreateHttpContext();
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        bool result = await _sut.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        result.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        ProblemDetails problemDetails = await DeserializeProblemDetails(httpContext);
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldNotBeNullOrEmpty();
        problemDetails.Detail.ShouldContain("Parameter cannot be null");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldHandleInvalidOperationException()
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
        problemDetails.Detail.ShouldBe("Invalid operation performed");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldHandleExceptionsWithInnerExceptions()
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
        problemDetails.Detail.ShouldBe("Outer error");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        DefaultHttpContext httpContext = CreateHttpContext();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<TaskCanceledException>(async () =>
            await _sut.TryHandleAsync(httpContext, exception, cts.Token));
    }

    [Fact]
    public async Task TryHandleAsync_ShouldWriteResponseAsJson()
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

        ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseText, JsonOptions);

        problemDetails.ShouldNotBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldHandleMultipleConsecutiveExceptions()
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

        problemDetails1.Detail.ShouldBe("First error");
        problemDetails2.Detail.ShouldBe("Second error");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
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
}
