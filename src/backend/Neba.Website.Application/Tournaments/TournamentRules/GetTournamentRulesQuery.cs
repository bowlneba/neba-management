
using Neba.Application.Documents;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Tournaments.TournamentRules;

/// <summary>
/// Query to retrieve the rules for a tournament as a DocumentDto.
/// </summary>
public sealed record GetTournamentRulesQuery
    : IQuery<DocumentDto>;
