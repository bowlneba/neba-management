
namespace Neba.Web.Server.Documents;

/// <summary>
/// View model for a document, including content and metadata.
/// </summary>
public sealed record DocumentViewModel<T>
{
    /// <summary>
    /// The main content of the document.
    /// </summary>
    public required T Content { get; init; }

    /// <summary>
    /// Key-value metadata associated with the document.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }
}