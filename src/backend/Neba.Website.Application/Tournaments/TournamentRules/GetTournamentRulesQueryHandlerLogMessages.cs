using Microsoft.Extensions.Logging;

namespace Neba.Website.Application.Tournaments.TournamentRules;

internal static partial class GetTournamentRulesQueryHandlerLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Tournament rules document not found in storage, retrieving from source")]
    public static partial void LogRetrievingFromSource(this ILogger<GetTournamentRulesQueryHandler> logger);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Triggered background job to sync tournament rules document to storage")]
    public static partial void LogTriggeredBackgroundSync(this ILogger<GetTournamentRulesQueryHandler> logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to trigger background sync for tournament rules document, but continuing with response")]
    public static partial void LogFailedToTriggerBackgroundSync(
        this ILogger<GetTournamentRulesQueryHandler> logger,
        Exception ex);
}
