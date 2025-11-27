using Neba.Application.Bowlers.BowlerTitleCounts;

namespace Neba.Application.Bowlers;

/// <summary>
/// Provides query operations for retrieving bowler-related data from the data store.
/// </summary>
public interface IWebsiteBowlerQueryRepository
{
    /// <summary>
    /// Asynchronously retrieves a collection of bowlers and their total title counts.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BowlerTitleCountDto"/>.</returns>
    Task<IReadOnlyCollection<BowlerTitleCountDto>> GetBowlerTitleCountsAsync(CancellationToken cancellationToken);
}
