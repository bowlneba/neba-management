using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Neba.Application.Behaviors;

internal sealed class RequestLoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IErrorOr
{
    private readonly ILogger<TRequest> _logger;

    public RequestLoggingBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = request.GetType().Name;

        try
        {
            _logger.RequestBeginning(name);

            cancellationToken.ThrowIfCancellationRequested();

            var response = await next();

            if (!response.IsError)
            {
                _logger.RequestEnding(name);
            }
            else
            {
                using (LogContext.PushProperty("Error", response.Errors))
                {
                    _logger.RequestError(name);
                }
            }

            return response;
        }
        catch (OperationCanceledException ex)
        {
            _logger.RequestCancelled(ex, name);

            var error = Error.Custom(499, "OperationCancelled", ex.Message);

            return (dynamic)error;
        }
    }
}

internal static partial class RequestLoggingBehaviorMessages
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Executing request {RequestName}")]
    public static partial void RequestBeginning(this ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Request {RequestName} executed successfully")]
    public static partial void RequestEnding(this ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Request {RequestName} failed")]
    public static partial void RequestError(this ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Request {RequestName} was cancelled")]
    public static partial void RequestCancelled(this ILogger logger, OperationCanceledException ex, string requestName);
}