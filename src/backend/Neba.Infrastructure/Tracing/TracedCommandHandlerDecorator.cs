using System.Diagnostics;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Neba.Application.Messaging;
using Neba.ServiceDefaults.Telemetry;

namespace Neba.Infrastructure.Tracing;

/// <summary>
/// Decorator that adds distributed tracing to command handlers via OpenTelemetry.
/// Creates a span for each command execution with relevant metadata.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
internal sealed class TracedCommandHandlerDecorator<TCommand, TResponse>
    : ICommandHandler<TCommand, TResponse>
      where TCommand : ICommand<TResponse>
{
    private static readonly ActivitySource s_activitySource = new("Neba.Handlers");

    private readonly ICommandHandler<TCommand, TResponse> _innerHandler;
    private readonly ILogger<TracedCommandHandlerDecorator<TCommand, TResponse>> _logger;
    private readonly string _commandType;
    private readonly string _responseType;

    public TracedCommandHandlerDecorator(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<TracedCommandHandlerDecorator<TCommand, TResponse>> logger)
    {
        _innerHandler = innerHandler;
        _logger = logger;
        _commandType = typeof(TCommand).Name;
        _responseType = typeof(TResponse).Name;
    }

    public async Task<ErrorOr<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        using Activity? activity = s_activitySource.StartActivity($"command.{_commandType}");

        activity?.SetCodeAttributes(_commandType, "Neba.Handlers");
        activity?.SetTag("handler.type", "command");
        activity?.SetTag("response.type", _responseType);

        long startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            ErrorOr<TResponse> result = await _innerHandler.HandleAsync(command, cancellationToken);

            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            activity?.SetTag("command.duration_ms", durationMs);

            if (result.IsError)
            {
                activity?.SetTag("command.success", false);
                activity?.SetTag("error.count", result.Errors.Count);
                activity?.SetTag("error.codes", string.Join(", ", result.Errors.Select(e => e.Code)));
                activity?.SetStatus(ActivityStatusCode.Error, result.FirstError.Description);

                _logger.LogCommandExecutionReturnedErrors(_commandType, durationMs, result.Errors.Count);
            }
            else
            {
                activity?.SetTag("command.success", true);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }

            return result;
        }
        catch (Exception ex)
        {
            double durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            activity?.SetTag("command.duration_ms", durationMs);
            activity?.SetTag("command.success", false);
            activity?.SetExceptionTags(ex);

            _logger.LogCommandExecutionFailed(_commandType, durationMs, ex);

            throw;
        }
    }
}
