using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Application.Storage;

namespace Neba.Website.Application.Tournaments.GetTournamentRules;

#pragma warning disable CA1812 // Internal class is instantiated through dependency injection

/// <summary>
/// Handles queries to retrieve tournament rules documentation as HTML.
/// </summary>
/// <param name="storageService">Service for retrieving documents from configured sources.</param>
internal sealed class GetTournamentRulesQueryHandler(IStorageService storageService)
        : IQueryHandler<GetTournamentRulesQuery, DocumentDto>
{
    private readonly IStorageService _storageService = storageService;
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
        DocumentDto document = await _storageService.GetContentWithMetadataAsync(
            TournamentRulesConstants.ContainerName,
            TournamentRulesConstants.DocumentKey,
            cancellationToken);

        return document;
    }
}
