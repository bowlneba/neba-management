namespace Neba.Application;

/// <summary>
/// Provides abstraction for date and time operations.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets today's date in the user's local timezone.
    /// </summary>
    /// <value>
    /// A <see cref="DateOnly"/> representing the current date in the user's timezone.
    /// </value>
    DateOnly Today { get; }
}
