namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Constants used for storing and retrieving the association's bylaws document.
/// </summary>
public static class BylawConstants
{
    /// <summary>
    /// The name of the container (or blob/container) where bylaws documents are stored.
    /// </summary>
    public const string BylawsContainerName = "documents";

    /// <summary>
    /// The logical name of the bylaws document resource.
    /// </summary>
    public const string BylawsDocumentName = "bylaws";

    /// <summary>
    /// The filename used when the bylaws are persisted as an HTML file.
    /// </summary>
    public const string BylawsFileName = "bylaws.html";

}
