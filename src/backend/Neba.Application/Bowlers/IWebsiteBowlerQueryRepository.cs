using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Bowlers;

namespace Neba.Application.Bowlers;

/// <summary>
/// Provides query operations for retrieving bowler-related data from the data store.
/// </summary>
public interface IWebsiteBowlerQueryRepository
{
    /// <summary>
    /// Asynchronously retrieves the detailed titles for a specific bowler.
    /// </summary>
    /// <param name="bowlerId">The unique identifier of the bowler.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BowlerTitlesDto"/> if found; otherwise, null.</returns>
    Task<BowlerTitlesDto?> GetBowlerTitlesAsync(BowlerId bowlerId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a collection of all bowler titles, including bowler and tournament details for each title.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BowlerTitleDto"/>.</returns>
    Task<IReadOnlyCollection<BowlerTitleDto>> GetTitlesAsync(CancellationToken cancellationToken);
}
