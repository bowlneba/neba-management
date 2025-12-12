using Neba.Application.Awards.BowlerOfTheYear;
using Neba.Application.Awards.HighAverage;
using Neba.Application.Awards.HighBlock;

namespace Neba.Application.Awards;

/// <summary>
/// Interface for querying Bowler of the Year awards from the website database.
/// </summary>
public interface IWebsiteAwardQueryRepository
{
    /// <summary>
    /// Asynchronously retrieves a collection of all Bowler of the Year awards.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BowlerOfTheYearAwardDto"/>.</returns>
    Task<IReadOnlyCollection<BowlerOfTheYearAwardDto>> ListBowlerOfTheYearAwardsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a collection of all High Block awards.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="HighBlockAwardDto"/>.</returns>
    Task<IReadOnlyCollection<HighBlockAwardDto>> ListHigh5GameBlockAwardsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a collection of all High Average awards.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="HighAverageAwardDto"/>.</returns>
    Task<IReadOnlyCollection<HighAverageAwardDto>> ListHighAverageAwardsAsync(CancellationToken cancellationToken);
}
