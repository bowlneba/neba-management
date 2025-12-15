namespace Neba.Application.Documents;

/// <summary>
/// Provides methods for retrieving documents as HTML.
/// </summary>
public interface IDocumentsService
{
    /// <summary>
    /// Retrieves the specified document as an HTML string.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTML representation of the document.</returns>
    Task<string> GetDocumentAsHtmlAsync(string documentId);
}
