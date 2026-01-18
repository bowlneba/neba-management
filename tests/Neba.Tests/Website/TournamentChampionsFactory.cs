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
        var assignedPairs = new HashSet<(int TournamentIndex, int BowlerIndex)>();

        while (assignedPairs.Count < count)
        {
            int tournamentIndex = faker.Random.Int(0, tournaments.Length - 1);
            int bowlerIndex = faker.Random.Int(0, bowlers.Length - 1);

            if (assignedPairs.Add((tournamentIndex, bowlerIndex)))
            {
                tournaments[tournamentIndex].AddChampion(bowlers[bowlerIndex]);
            }
        }
    }
}
