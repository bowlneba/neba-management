namespace Neba.Application.Documents;

/// <summary>
/// Provides methods for retrieving documents as HTML.
/// </summary>
public interface IDocumentsService
{
    /// <summary>
    /// Retrieves the specified document as an HTML string by its logical name.
    /// </summary>
    /// <param name="documentName">The logical name of the document to retrieve (e.g., "tournament-rules"). This name is mapped to the actual document source in the infrastructure layer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTML representation of the document.</returns>
    /// <exception cref="ArgumentException">Thrown when the document name is not found in the configuration.</exception>
    Task<string> GetDocumentAsHtmlAsync(string documentName, CancellationToken cancellationToken);
}
