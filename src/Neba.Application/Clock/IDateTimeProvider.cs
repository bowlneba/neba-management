namespace Neba.Application.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
