using Neba.Application.Messaging;
using Neba.Application.Storage;

namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Handles queries to retrieve organization bylaws documentation as HTML.
/// </summary>
/// <param name="storageService">Service for retrieving documents from configured sources.</param>
internal sealed class GetBylawsQueryHandler(IStorageService storageService)
        : IQueryHandler<GetBylawsQuery, string>
{
    internal const string BylawsDocumentName = "bylaws";
    private readonly IStorageService _storageService = storageService;

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
        string htmlContent = await _storageService.GetContentAsync("documents", "bylaws.html", cancellationToken);

        return htmlContent;
    }
}
