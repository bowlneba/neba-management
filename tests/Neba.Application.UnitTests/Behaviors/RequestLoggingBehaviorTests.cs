using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Neba.Application.Behaviors;

namespace Neba.Application.UnitTests.Behaviors;

public sealed class RequestLoggingBehaviorTests
{
    private readonly ILogger<TestRequest> _mockLogger;
    private readonly RequestHandlerDelegate<ErrorOr<TestResponse>> _mockNext;

    private readonly RequestLoggingBehavior<TestRequest, ErrorOr<TestResponse>> _requestLoggingBehavior;

    public RequestLoggingBehaviorTests()
    {
        _mockLogger = Substitute.For<ILogger<TestRequest>>();
        _mockNext = Substitute.For<RequestHandlerDelegate<ErrorOr<TestResponse>>>();

        _requestLoggingBehavior = new RequestLoggingBehavior<TestRequest, ErrorOr<TestResponse>>(_mockLogger);
    }

    [Fact]
    public async Task Handle_RequestIsCancelled_ReturnsErrorIndicatingRequestWasCancelled()
    {
        // Arrange
        var request = new TestRequest();
        var cancellationToken = new CancellationToken(true);

        // Act
        var response = await _requestLoggingBehavior.Handle(request, _mockNext, cancellationToken);

        // Assert
        response.IsError.Should().BeTrue();
        response.Errors.Should().ContainSingle();
        response.Errors.First().NumericType.Should().Be(499);
        response.Errors.First().Code.Should().Be("OperationCancelled");
        response.Errors.First().Description.Should().Be("The operation was canceled.");
    }

    [Fact]
    public async Task Handle_RequestIsNotCancelled_ReturnsResponse()
    {
        // Arrange
        var request = new TestRequest();
        var cancellationToken = new CancellationToken(false);
        var response = new TestResponse();

        _mockNext().Returns(response);

        // Act
        var result = await _requestLoggingBehavior.Handle(request, _mockNext, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(response);
    }
}