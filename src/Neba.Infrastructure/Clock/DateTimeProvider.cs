using Neba.Application.Clock;

namespace Neba.Infrastructure.Clock;

internal sealed class DateTimeProvider
    : IDateTimeProvider
{
    public DateTime UtcNow
        => DateTime.UtcNow;
}
