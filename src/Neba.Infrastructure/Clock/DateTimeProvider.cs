using System.Diagnostics.CodeAnalysis;
using Neba.Application.Clock;

namespace Neba.Infrastructure.Clock;

[SuppressMessage("Design", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by ASP.NET Core.")]
internal sealed class DateTimeProvider
    : IDateTimeProvider
{
    public DateTime UtcNow
        => DateTime.UtcNow;
}
