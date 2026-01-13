using Neba.Application;

namespace Neba.Infrastructure;

internal sealed class DateTimeProvider
    : IDateTimeProvider
{
    public DateOnly Today
        => DateOnly.FromDateTime(DateTime.UtcNow);
}
