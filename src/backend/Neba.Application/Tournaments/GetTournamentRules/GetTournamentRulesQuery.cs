
using Neba.Application.Messaging;

namespace Neba.Application.Tournaments.GetTournamentRules;

/// <summary>
/// Query to retrieve the rules for a tournament as an HTML string.
/// </summary>
public sealed record GetTournamentRulesQuery
    : IQuery<string>;
