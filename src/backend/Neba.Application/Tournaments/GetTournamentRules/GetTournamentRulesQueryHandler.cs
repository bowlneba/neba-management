using Neba.Application.Messaging;
using Neba.Application.Documents;

namespace Neba.Application.Tournaments.GetTournamentRules;

/// <summary>
/// Handles queries to retrieve tournament rules documentation as HTML.
/// </summary>
/// <param name="documentsService">Service for retrieving documents from configured sources.</param>
internal sealed class GetTournamentRulesQueryHandler(IDocumentsService documentsService)
        : IQueryHandler<GetTournamentRulesQuery, string>
{
    private const string TournamentRulesDocumentName = "tournament-rules";
    private readonly IDocumentsService _documentsService = documentsService;

    /// <summary>
    /// Retrieves the tournament rules document as HTML.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tournament rules as an HTML string.</returns>
    public async Task<string> HandleAsync(
        GetTournamentRulesQuery request,
        CancellationToken cancellationToken)
    {
        string htmlContent = await _documentsService.GetDocumentAsHtmlAsync(TournamentRulesDocumentName, cancellationToken);

        return htmlContent;
    }
}
