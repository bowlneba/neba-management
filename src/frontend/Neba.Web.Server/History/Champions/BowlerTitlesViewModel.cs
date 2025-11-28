namespace Neba.Web.Server.History.Champions;

internal sealed record BowlerTitlesViewModel
{
    public required string BowlerName { get; init; }

    public required IReadOnlyCollection<TitlesViewModel> Titles { get; init; }
}

internal sealed record TitlesViewModel
{
    public required string TournamentDate { get; init; }

    public required string TournamentType { get; init; }
}
