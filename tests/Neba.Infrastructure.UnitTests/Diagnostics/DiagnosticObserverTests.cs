using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neba.Infrastructure.Diagnostics;

namespace Neba.Infrastructure.UnitTests.Diagnostics;

public sealed class DiagnosticObserverTests
{
    private readonly IObserver<KeyValuePair<string, object?>> _keyValueObserver;
    private readonly ILogger<DiagnosticObserver> _logger;
    private readonly DiagnosticObserver _observer;

    public DiagnosticObserverTests()
    {
        _logger = Substitute.For<ILogger<DiagnosticObserver>>();
        _keyValueObserver = Substitute.For<IObserver<KeyValuePair<string, object?>>>();
        _observer = new DiagnosticObserver(_keyValueObserver, _logger);
    }

    [Fact]
    public void OnNext_SubscribesToDiagnosticListener_WhenNameMatchesDbLoggerCategoryName()
    {
        var diagnosticListener = Substitute.For<DiagnosticListener>(DbLoggerCategory.Name);
        diagnosticListener.IsEnabled(Arg.Any<string>()).Returns(true);

        _observer.OnNext(diagnosticListener);

        diagnosticListener.Received().Subscribe(_keyValueObserver);
    }

    [Fact]
    public void OnNext_DoesNotSubscribeToDiagnosticListener_WhenNameDoesNotMatchDbLoggerCategoryName()
    {
        var diagnosticListener = Substitute.For<DiagnosticListener>("NonMatchingName");
        diagnosticListener.IsEnabled(Arg.Any<string>()).Returns(true);

        _observer.OnNext(diagnosticListener);

        diagnosticListener.DidNotReceive().Subscribe(_keyValueObserver);
    }
}