using ErrorOr;
using FluentValidation;
using MediatR;
using Neba.Application.Behaviors;

namespace Neba.Application.UnitTests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private ValidationBehavior<TestCommandRequest, TestResponse> _validationBehavior = null!;
    private IEnumerable<IValidator<TestCommandRequest>> _validators = [];

    [Fact]
    public async Task Handle_WhenValidatorsIsEmpty_CallsAndReturnsNext()
    {
        _validationBehavior = new ValidationBehavior<TestCommandRequest, TestResponse>(_validators);

        var request = new TestCommandRequest();
        var response = new TestResponse();
        var next = new RequestHandlerDelegate<ErrorOr<TestResponse>>(() => Task.FromResult(response.ToErrorOr()));

        var result = await _validationBehavior.Handle(request, next, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task Handle_WhenValidatorsIsNotEmptyAndAllValidatorsPass_CallsAndReturnsNext()
    {
        var request = new TestCommandRequest();
        var response = new TestResponse();
        var next = new RequestHandlerDelegate<ErrorOr<TestResponse>>(() => Task.FromResult(response.ToErrorOr()));

        _validators = [new ValidTestCommandRequest()];
        _validationBehavior = new ValidationBehavior<TestCommandRequest, TestResponse>(_validators);

        var result = await _validationBehavior.Handle(request, next, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task Handle_WhenValidatorsIsNotEmptyAndSomeValidatorsFail_ReturnsValidationErrors()
    {
        var request = new TestCommandRequest();
        var next = new RequestHandlerDelegate<ErrorOr<TestResponse>>(() =>
            Task.FromResult(new TestResponse().ToErrorOr()));

        _validators =
            [new InvalidTestCommandRequest1(), new ValidTestCommandRequest(), new InvalidTestCommandRequest2()];
        _validationBehavior = new ValidationBehavior<TestCommandRequest, TestResponse>(_validators);

        var result = await _validationBehavior.Handle(request, next, CancellationToken.None);

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