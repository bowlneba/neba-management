using Neba.Application.Documents;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Handles queries to retrieve organization bylaws documentation as HTML.
/// </summary>
/// <param name="documentsService">Service for retrieving documents from configured sources.</param>
internal sealed class GetBylawsQueryHandler(IDocumentsService documentsService)
        : IQueryHandler<GetBylawsQuery, string>
{
    private const string BylawsDocumentName = "bylaws";
    private readonly IDocumentsService _documentsService = documentsService;

    /// <summary>
    /// Retrieves the organization bylaws document as HTML.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organization bylaws as an HTML string.</returns>
    public async Task<string> HandleAsync(
        GetBylawsQuery request,
        CancellationToken cancellationToken)
    {
        string htmlContent = await _documentsService.GetDocumentAsHtmlAsync(BylawsDocumentName, cancellationToken);

        return htmlContent;
    }
}
