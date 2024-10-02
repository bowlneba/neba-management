using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Neba.Application.Behaviors;

namespace Neba.Application.UnitTests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private readonly ILogger<ValidationBehavior<TestCommandRequest, TestResponse>> _logger =
        Substitute.For<ILogger<ValidationBehavior<TestCommandRequest, TestResponse>>>();
    private IEnumerable<IValidator<TestCommandRequest>> _validators = [];

    [Fact]
    public async Task Handle_WhenValidatorsIsEmpty_ShouldCallAndReturnNext()
    {
        // Arrange
        var validationBehavior = new ValidationBehavior<TestCommandRequest, TestResponse>(_validators, _logger);

        var request = new TestCommandRequest();
        var response = new TestResponse();
        var next = new RequestHandlerDelegate<ErrorOr<TestResponse>>(() => Task.FromResult(response.ToErrorOr()));

        // Act
        var result = await validationBehavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task Handle_WhenValidatorsIsNotEmptyAndAllValidatorsPass_ShouldCallAndReturnNext()
    {
        // Arrange
        _validators = [new ValidTestCommandRequest()];
        var validationBehavior = new ValidationBehavior<TestCommandRequest, TestResponse>(_validators, _logger);

        var request = new TestCommandRequest();
        var response = new TestResponse();
        var next = new RequestHandlerDelegate<ErrorOr<TestResponse>>(() => Task.FromResult(response.ToErrorOr()));

        // Act
        var result = await validationBehavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task Handle_WhenValidatorsIsNotEmptyAndSomeValidatorsFail_ShouldReturnValidationErrors()
    {
        // Arrange
        _validators = [new InvalidTestCommandRequest1(), new ValidTestCommandRequest(), new InvalidTestCommandRequest2()];
        var validationBehavior = new ValidationBehavior<TestCommandRequest, TestResponse>(_validators, _logger);

        var request = new TestCommandRequest();
        var response = new TestResponse();
        var next = new RequestHandlerDelegate<ErrorOr<TestResponse>>(() => Task.FromResult(response.ToErrorOr()));

        // Act
        var result = await validationBehavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(error =>
            error.Code == "LessThanOrEqualValidator" &&
            error.Description == "'Request Value1' must be less than or equal to '-1'.");
        result.Errors.Should().Contain(error =>
            error.Code == "GreaterThanOrEqualValidator" &&
            error.Description == "'Request Value2' must be greater than or equal to '3'.");
    }
}

internal sealed class ValidTestCommandRequest
    : AbstractValidator<TestCommandRequest>;

internal sealed class InvalidTestCommandRequest1
    : AbstractValidator<TestCommandRequest>
{
    public InvalidTestCommandRequest1()
    {
        RuleFor(request => request.RequestValue1).LessThanOrEqualTo(-1);
    }
}

internal sealed class InvalidTestCommandRequest2
    : AbstractValidator<TestCommandRequest>
{
    public InvalidTestCommandRequest2()
    {
        RuleFor(request => request.RequestValue2).GreaterThanOrEqualTo(3);
    }
}