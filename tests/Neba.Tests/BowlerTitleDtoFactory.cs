using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Domain.Bowlers;
using Neba.Domain.Tournaments;

namespace Neba.Tests;

public static class BowlerTitleDtoFactory
{
    public const string BowlerName = "John Doe";

    public static BowlerTitleDto Create(
        BowlerId? bowlerId = null,
        string? bowlerName = null,
        Month? tournamentMonth = null,
        int? tournamentYear = null,
        TournamentType? tournamentType = null
        )
            => new()
            {
                BowlerId = bowlerId ?? BowlerId.New(),
                BowlerName = bowlerName ?? BowlerName,
                TournamentMonth = tournamentMonth ?? Month.January,
                TournamentYear = tournamentYear ?? 2020,
                TournamentType = tournamentType ?? TournamentType.Singles
            };

    public static BowlerTitleDto Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerTitleDto> Bogus(
        int count,
        int? seed = null
        )
    {
        Faker<BowlerTitleDto> faker = new Faker<BowlerTitleDto>()
            .RuleFor(bowler => bowler.BowlerId, _ => BowlerId.New())
            .RuleFor(bowler => bowler.BowlerName, f => f.Name.FullName())
            .RuleFor(bowler => bowler.TournamentMonth, f => f.PickRandom(Month.List.ToArray()))
            .RuleFor(bowler => bowler.TournamentYear, f => f.Date.Past(70).Year)
            .RuleFor(bowler => bowler.TournamentType, f => f.PickRandom(TournamentType.List.ToArray()));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
