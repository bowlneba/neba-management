
namespace Neba.Application.Bowlers.BowlerTitles;

/// <summary>
/// Provides query operations for retrieving bowler-related data from the data store.
/// </summary>
public interface IWebsiteTitleQueryRepository
{
    /// <summary>
    /// Asynchronously retrieves a summary of titles for all bowlers.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BowlerTitleSummaryDto"/>.</returns>
    Task<IReadOnlyCollection<BowlerTitleSummaryDto>> ListTitleSummariesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a collection of all bowler titles, including bowler and tournament details for each title.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BowlerTitleDto"/>.</returns>
    Task<IReadOnlyCollection<BowlerTitleDto>> ListTitlesAsync(CancellationToken cancellationToken);
}
