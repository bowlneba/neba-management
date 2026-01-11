namespace Neba.Website.Application.Tournaments;

/// <summary>
/// Defines methods for querying tournament data in the website application.
/// </summary>
public interface IWebsiteTournamentQueryRepository
{
    /// <summary>
    /// Lists tournaments that occur after the specified date.
    /// </summary>
    /// <param name="afterDate">The date after which tournaments should be listed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of tournament summaries.</returns>
    Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsAfterDateAsync(DateOnly afterDate, CancellationToken cancellationToken);

    /// <summary>
    /// Lists tournaments occurring in the specified year.
    /// </summary>
    /// <param name="year">The year for which to list tournaments.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of tournament summaries.</returns>
    Task<IReadOnlyCollection<TournamentSummaryDto>> ListTournamentsInYearAsync(int year, CancellationToken cancellationToken);
}
