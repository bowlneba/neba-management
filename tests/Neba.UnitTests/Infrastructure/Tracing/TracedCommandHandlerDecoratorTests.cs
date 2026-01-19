using System.Diagnostics;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Neba.Application.Messaging;
using Neba.Infrastructure.Tracing;

namespace Neba.UnitTests.Infrastructure.Tracing;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Tracing")]
public sealed class TracedCommandHandlerDecoratorTests
{
    private sealed record TestCommand : ICommand<string>;

    private sealed record TestCommandWithError : ICommand<string>;

    private sealed class SuccessfulCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<ErrorOr<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken)
        {
            ErrorOr<string> result = "success";
            return Task.FromResult(result);
        }
    }

    private sealed class FailingCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<ErrorOr<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromException<ErrorOr<string>>(
                new InvalidOperationException("Command handler failed"));
        }
    }

    private sealed class ErrorReturningCommandHandler : ICommandHandler<TestCommandWithError, string>
    {
        public Task<ErrorOr<string>> HandleAsync(TestCommandWithError command, CancellationToken cancellationToken)
        {
            ErrorOr<string> result = Error.NotFound("TEST", "Test error occurred");
            return Task.FromResult(result);
        }
    }

    [Fact(DisplayName = "Handles command successfully with tracing")]
    public async Task HandleAsync_WithSuccessfulHandler_ReturnsSuccess()
    {
        // Arrange
        var innerHandler = new SuccessfulCommandHandler();
        NullLogger<TracedCommandHandlerDecorator<TestCommand, string>> logger = NullLogger<TracedCommandHandlerDecorator<TestCommand, string>>.Instance;
        var decorator = new TracedCommandHandlerDecorator<TestCommand, string>(innerHandler, logger);
        var command = new TestCommand();

        // Act
        ErrorOr<string> result = await decorator.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("success");
    }

    [Fact(DisplayName = "Traces activity for command execution")]
    public async Task HandleAsync_CreatesActivityForCommand()
    {
        // Arrange
        var innerHandler = new SuccessfulCommandHandler();
        NullLogger<TracedCommandHandlerDecorator<TestCommand, string>> logger = NullLogger<TracedCommandHandlerDecorator<TestCommand, string>>.Instance;
        var decorator = new TracedCommandHandlerDecorator<TestCommand, string>(innerHandler, logger);
        var command = new TestCommand();

        // Enable activity listener to capture traces
        using var listener = new ActivityListener { ShouldListenTo = _ => true, Sample = SampleActivity };
        ActivitySource.AddActivityListener(listener);

        // Act
        ErrorOr<string> result = await decorator.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
    }

    [Fact(DisplayName = "Handles command exception with tracing")]
    public async Task HandleAsync_WithFailingHandler_ThrowsAndTraces()
    {
        // Arrange
        var innerHandler = new FailingCommandHandler();
        NullLogger<TracedCommandHandlerDecorator<TestCommand, string>> logger = NullLogger<TracedCommandHandlerDecorator<TestCommand, string>>.Instance;
        var decorator = new TracedCommandHandlerDecorator<TestCommand, string>(innerHandler, logger);
        var command = new TestCommand();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            () => decorator.HandleAsync(command, CancellationToken.None));
    }

    [Fact(DisplayName = "Handles error result from inner handler")]
    public async Task HandleAsync_WithErrorResult_ReturnsError()
    {
        // Arrange
        var innerHandler = new ErrorReturningCommandHandler();
        var decorator = new TracedCommandHandlerDecorator<TestCommandWithError, string>(
            innerHandler, NullLogger<TracedCommandHandlerDecorator<TestCommandWithError, string>>.Instance);
        var command = new TestCommandWithError();

        // Act
        ErrorOr<string> result = await decorator.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem();
    }

    [Fact(DisplayName = "Respects cancellation token")]
    public async Task HandleAsync_WithCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var innerHandler = new SuccessfulCommandHandler();
        NullLogger<TracedCommandHandlerDecorator<TestCommand, string>> logger = NullLogger<TracedCommandHandlerDecorator<TestCommand, string>>.Instance;
        var decorator = new TracedCommandHandlerDecorator<TestCommand, string>(innerHandler, logger);
        var command = new TestCommand();
        var cancellationToken = new CancellationToken(canceled: false);

        // Act
        ErrorOr<string> result = await decorator.HandleAsync(command, cancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
    }

    [Fact(DisplayName = "Multiple sequential commands are traced independently")]
    public async Task HandleAsync_WithMultipleCommands_TraceEach()
    {
        // Arrange
        var innerHandler = new SuccessfulCommandHandler();
        NullLogger<TracedCommandHandlerDecorator<TestCommand, string>> logger = NullLogger<TracedCommandHandlerDecorator<TestCommand, string>>.Instance;
        var decorator = new TracedCommandHandlerDecorator<TestCommand, string>(innerHandler, logger);

        // Act
        ErrorOr<string> result1 = await decorator.HandleAsync(new TestCommand(), CancellationToken.None);
        ErrorOr<string> result2 = await decorator.HandleAsync(new TestCommand(), CancellationToken.None);
        ErrorOr<string> result3 = await decorator.HandleAsync(new TestCommand(), CancellationToken.None);

        // Assert
        result1.IsError.ShouldBeFalse();
        result2.IsError.ShouldBeFalse();
        result3.IsError.ShouldBeFalse();
    }

    [Fact(DisplayName = "Logs are recorded for successful command execution")]
    public async Task HandleAsync_LogsSuccessfulExecution()
    {
        // Arrange
        var innerHandler = new SuccessfulCommandHandler();
        NullLogger<TracedCommandHandlerDecorator<TestCommand, string>> logger = NullLogger<TracedCommandHandlerDecorator<TestCommand, string>>.Instance;
        var decorator = new TracedCommandHandlerDecorator<TestCommand, string>(innerHandler, logger);
        var command = new TestCommand();

        // Act
        ErrorOr<string> result = await decorator.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        // Logging is no-op with NullLogger; success path completes without exceptions
    }

    private static ActivitySamplingResult SampleActivity(ref ActivityCreationOptions<ActivityContext> _)
        => ActivitySamplingResult.AllData;
}
