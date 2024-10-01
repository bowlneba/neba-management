using ErrorOr;
using FluentValidation;
using MediatR;
using Neba.Application.Messaging;

namespace Neba.Application.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, ErrorOr<TResponse>>
    where TRequest : IBaseCommand
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<ErrorOr<TResponse>> Handle(TRequest request, RequestHandlerDelegate<ErrorOr<TResponse>> next, CancellationToken cancellationToken)
    {
        var validationResults =
            await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(request, cancellationToken)));

        var validationErrors = validationResults.SelectMany(result => result.Errors).ToList();

        return validationErrors.Count == 0
            ? await next()
            : validationErrors.ConvertAll(validationError =>
                Error.Validation(validationError.ErrorCode, validationError.ErrorMessage));
    }
}