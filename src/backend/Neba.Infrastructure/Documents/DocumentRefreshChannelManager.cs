using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Documents;

/// <summary>
/// Manages document refresh notification channels with on-demand creation and automatic cleanup.
/// </summary>
/// <remarks>
/// Channels are created when first listener connects and removed when no listeners remain.
/// Each channel has a 5-minute maximum lifetime and 30-second idle timeout.
/// </remarks>
public sealed class DocumentRefreshChannelManager : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, ChannelState> _channels = new();
    private readonly ILogger<DocumentRefreshChannelManager> _logger;
    private readonly CancellationTokenSource _shutdownCts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentRefreshChannelManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public DocumentRefreshChannelManager(ILogger<DocumentRefreshChannelManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates a channel for the specified document type.
    /// </summary>
    /// <param name="documentType">The document type (e.g., "bylaws", "tournament-rules").</param>
    /// <returns>A channel reader for consuming status updates.</returns>
    public ChannelReader<DocumentRefreshStatusEvent> GetOrCreateChannel(string documentType)
    {
        var channelState = _channels.GetOrAdd(documentType, key =>
        {
            _logger.LogCreatingChannel(key);

            var channel = Channel.CreateUnbounded<DocumentRefreshStatusEvent>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = false
            });

            var state = new ChannelState(channel, DateTimeOffset.UtcNow);

            // Start timeout monitoring for this channel
            _ = MonitorChannelTimeoutAsync(key, state, _shutdownCts.Token);

            return state;
        });

        channelState.IncrementListeners();

        return channelState.Channel.Reader;
    }

    /// <summary>
    /// Writes a status update to the channel for the specified document type.
    /// </summary>
    /// <param name="documentType">The document type (e.g., "bylaws", "tournament-rules").</param>
    /// <param name="statusEvent">The status event to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask WriteToChannelAsync(
        string documentType,
        DocumentRefreshStatusEvent statusEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(statusEvent);

        if (_channels.TryGetValue(documentType, out ChannelState? channelState))
        {
            await channelState.Channel.Writer.WriteAsync(statusEvent, cancellationToken);
            channelState.UpdateLastActivity();

            _logger.LogWroteStatusUpdate(documentType, statusEvent.Status);
        }
        else
        {
            _logger.LogNoActiveChannel(documentType, statusEvent.Status);
        }
    }

    /// <summary>
    /// Decrements the listener count for a document type and removes channel if no listeners remain.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    public void ReleaseListener(string documentType)
    {
        if (_channels.TryGetValue(documentType, out ChannelState? channelState))
        {
            channelState.DecrementListeners();

            if (channelState.ListenerCount == 0)
            {
                _logger.LogSchedulingCleanup(documentType);

                // Schedule cleanup after a brief delay to allow for reconnections
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    if (channelState.ListenerCount == 0 &&
                        _channels.TryRemove(documentType, out _))
                    {
                        channelState.Channel.Writer.Complete();
                        _logger.LogRemovedChannel(documentType);
                    }
                });
            }
        }
    }

#pragma warning disable CA1031 // Do not catch general exception types
    private async Task MonitorChannelTimeoutAsync(
        string documentType,
        ChannelState channelState,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                var now = DateTimeOffset.UtcNow;
                var timeSinceCreation = now - channelState.CreatedAt;
                var timeSinceActivity = now - channelState.LastActivity;

                // Maximum lifetime: 5 minutes
                if (timeSinceCreation > TimeSpan.FromMinutes(5))
                {
                    _logger.LogChannelMaxLifetimeExceeded(documentType);

                    if (_channels.TryRemove(documentType, out _))
                    {
                        channelState.Channel.Writer.Complete();
                    }
                    break;
                }

                // Idle timeout: 30 seconds (only if no listeners)
                if (channelState.ListenerCount == 0 && timeSinceActivity > TimeSpan.FromSeconds(30))
                {
                    _logger.LogChannelIdleTimeoutExceeded(documentType);

                    if (_channels.TryRemove(documentType, out _))
                    {
                        channelState.Channel.Writer.Complete();
                    }
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogChannelMonitoringError(ex, documentType);
        }
    }
#pragma warning restore CA1031

    /// <summary>
    /// Disposes of the channel manager and completes all active channels.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _shutdownCts.CancelAsync();

        foreach (var kvp in _channels)
        {
            kvp.Value.Channel.Writer.Complete();
        }

        _channels.Clear();
        _shutdownCts.Dispose();

        await Task.CompletedTask;
    }

    private sealed class ChannelState
    {
        private int _listenerCount;
        private long _lastActivityTicks;

        public Channel<DocumentRefreshStatusEvent> Channel { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset LastActivity => new DateTimeOffset(_lastActivityTicks, TimeSpan.Zero);
        public int ListenerCount => _listenerCount;

        public ChannelState(Channel<DocumentRefreshStatusEvent> channel, DateTimeOffset createdAt)
        {
            Channel = channel;
            CreatedAt = createdAt;
            _lastActivityTicks = createdAt.UtcTicks;
        }

        public void IncrementListeners() => Interlocked.Increment(ref _listenerCount);
        public void DecrementListeners() => Interlocked.Decrement(ref _listenerCount);
        public void UpdateLastActivity() => Interlocked.Exchange(ref _lastActivityTicks, DateTimeOffset.UtcNow.UtcTicks);
    }
}
