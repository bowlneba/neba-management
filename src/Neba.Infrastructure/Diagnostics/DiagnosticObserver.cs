using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Neba.Infrastructure.Diagnostics;

internal sealed class DiagnosticObserver
    : IObserver<DiagnosticListener>
{
    private readonly IObserver<KeyValuePair<string, object?>> _keyValueObserver;
    private readonly ILogger<DiagnosticObserver> _logger;

    public DiagnosticObserver(
        IObserver<KeyValuePair<string, object?>> keyValueObserver,
        ILogger<DiagnosticObserver> logger)
    {
        _keyValueObserver = keyValueObserver;
        _logger = logger;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
        => _logger.ObserverError(error);

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name == DbLoggerCategory.Name)
        {
            value.Subscribe(_keyValueObserver);
        }
    }
}
