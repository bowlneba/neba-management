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
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="BowlerOfTheYearDto"/>.</returns>
    Task<IReadOnlyCollection<BowlerOfTheYearDto>> ListBowlerOfTheYearAwardsAsync(CancellationToken cancellationToken);

}
