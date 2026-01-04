using Neba.Website.Domain.Bowlers;
using Neba.Website.Domain.Tournaments;

namespace Neba.Tests.Website;

public static class TournamentChampionsFactory
{
    /// <summary>
    /// Assigns champions to tournaments by calling AddChampion on each tournament.
    /// </summary>
    /// <param name="tournaments">The collection of tournaments to assign champions to.</param>
    /// <param name="bowlers">The collection of bowlers to choose champions from.</param>
    /// <param name="count">The number of champion assignments to create.</param>
    public static void Bogus(
        Tournament[] tournaments,
        Bowler[] bowlers,
        int count)
    {
        var faker = new Bogus.Faker();

        for (int i = 0; i < count; i++)
        {

            Tournament tournament = faker.PickRandom(tournaments);
            Bowler bowler = faker.PickRandom(bowlers);

            tournament.AddChampion(bowler);
        }
    }
}
