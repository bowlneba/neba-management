using Microsoft.Extensions.Logging;
using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Application.Storage;

namespace Neba.Website.Application.Tournaments.TournamentRules;

#pragma warning disable CA1812 // Internal class is instantiated through dependency injection

/// <summary>
/// Handles queries to retrieve tournament rules documentation as HTML.
/// </summary>
/// <remarks>
/// Uses a cache-aside pattern: attempts to retrieve from storage first (fast path),
/// then falls back to source, and triggers background sync (slow path).
/// </remarks>
/// <param name="storageService">Service for retrieving documents from storage.</param>
/// <param name="documentsService">Service for retrieving documents from source.</param>
/// <param name="tournamentRulesSyncJob">Background job for syncing documents to storage.</param>
/// <param name="logger">Logger for diagnostic information.</param>
internal sealed class GetTournamentRulesQueryHandler(
    IStorageService storageService,
    IDocumentsService documentsService,
    ITournamentRulesSyncBackgroundJob tournamentRulesSyncJob,
    ILogger<GetTournamentRulesQueryHandler> logger)
        : IQueryHandler<GetTournamentRulesQuery, DocumentDto>
{

    /// <summary>
    /// Retrieves the tournament rules document as HTML.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tournament rules as a DocumentDto.</returns>
    public async Task<DocumentDto> HandleAsync(
        GetTournamentRulesQuery request,
        CancellationToken cancellationToken)
    {
        // Fast path: try to get from the storage cache
        if (await storageService.ExistsAsync(
            TournamentRulesConstants.ContainerName,
            TournamentRulesConstants.FileName,
            cancellationToken))
        {
            return await storageService.GetContentWithMetadataAsync(
                TournamentRulesConstants.ContainerName,
                TournamentRulesConstants.FileName,
                cancellationToken);
        }

        // Slow path: get from source and trigger background sync
        logger.LogRetrievingFromSource();

        string documentHtml = await documentsService.GetDocumentAsHtmlAsync(
            TournamentRulesConstants.DocumentKey,
            cancellationToken);

        // Trigger the background job to cache the document (fire-and-forget)
        try
        {
            tournamentRulesSyncJob.TriggerImmediateSync();
            logger.LogTriggeredBackgroundSync();
        }
#pragma warning disable CA1031 // Catching all exceptions is intentional to ensure resilience - background job failure should not break the request
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger.LogFailedToTriggerBackgroundSync(ex);
        }

        return new DocumentDto
        {
            Content = documentHtml,
            Metadata = new Dictionary<string, string>
            {
                ["LastUpdatedUtc"] = DateTimeOffset.UtcNow.ToString("o"),
                ["LastUpdatedBy"] = "System"
            }
        };
    }
}
