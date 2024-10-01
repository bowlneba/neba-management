using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Neba.Application.Messaging;

namespace Neba.Application.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, ErrorOr<TResponse>>
    where TRequest : IBaseCommand
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<ErrorOr<TResponse>> Handle(TRequest request, RequestHandlerDelegate<ErrorOr<TResponse>> next, CancellationToken cancellationToken)
    {
        _logger.LogBeginValidation(request.GetType().Name);

        var validationResults =
            await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(request, cancellationToken)));

        _logger.LogValidationResults(request.GetType().Name, validationResults);

        var validationErrors = validationResults.SelectMany(result => result.Errors).ToList();

        _logger.LogValidationComplete(request.GetType().Name);

        return validationErrors.Count == 0
            ? await next()
            : validationErrors.ConvertAll(validationError =>
                Error.Validation(validationError.ErrorCode, validationError.ErrorMessage));
    }
}

internal static partial class ValidationBehaviorLogMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Beginning Validation for {RequestType}")]
    public static partial void LogBeginValidation(this ILogger logger, string requestType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Validation results for {RequestType}: {@ValidationResults}")]
    public static partial void LogValidationResults(this ILogger logger, string requestType, ValidationResult[] validationResults);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Validation complete for {RequestType}")]
    public static partial void LogValidationComplete(this ILogger logger, string requestType);
}