using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.TournamentRules;

/// <summary>
/// Command to request a refresh of the tournament rules cache.
/// </summary>
/// <remarks>
/// Handled by an application service to rebuild or update any cached
/// representation of tournament rules. The command produces a <see cref="string"/>
/// response which typically contains a result identifier, cache key or
/// a short status message indicating the outcome.
/// </remarks>
/// <seealso cref="ICommand{TResponse}"/>
public sealed record RefreshTournamentRulesCacheCommand
    : ICommand<string>;
