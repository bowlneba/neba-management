namespace Neba.Web.Server.History.Champions;

public sealed record BowlerTitleCountViewModel
{
    public required Guid BowlerId { get; init; }

    public required string BowlerName { get; init; }

    public required int Titles { get; init; }
}
