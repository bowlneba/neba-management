using Neba.Application.Clock;

namespace Neba.Infrastructure.Clock;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "This class is instantiated by the dependency injection container.")]
internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow
        => DateTime.UtcNow;
}
