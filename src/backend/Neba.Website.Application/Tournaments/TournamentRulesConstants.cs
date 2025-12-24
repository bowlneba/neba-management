namespace Neba.Website.Application.Tournaments;

/// <summary>
/// Constants used for storing and retrieving the tournament rules document.
/// </summary>
public static class TournamentRulesConstants
{
    /// <summary>
    /// The name of the container (or blob/container) where tournament rules documents are stored.
    /// </summary>
    public const string ContainerName = "documents";

    /// <summary>
    /// The logical name of the tournament rules document resource.
    /// </summary>
    public const string DocumentKey = "tournament-rules";

    /// <summary>
    /// The filename used when the tournament rules are persisted as an HTML file.
    /// </summary>
    public const string FileName = "tournament-rules.html";

}
