namespace Neba.Website.Application.BowlingCenters;

/// <summary>
/// Provides read-only access to bowling center details used by the public website experience.
/// </summary>
/// <remarks>
/// This interface is query-only to keep separation from commands and simplify caching strategies for the public directory.
/// </remarks>
public interface IWebsiteBowlingCenterQueryRepository
{
    /// <summary>
    /// Retrieves the set of bowling centers available for website display.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the retrieval operation.</param>
    /// <returns>Immutable collection of centers with contact and geolocation metadata.</returns>
    Task<IReadOnlyCollection<BowlingCenterDto>> ListBowlingCentersAsync(CancellationToken cancellationToken);
}
