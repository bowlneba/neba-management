using System.Globalization;
using Bogus;
using Neba.Web.Server.History.Awards;

namespace Neba.Tests.Website;

public static class BowlerOfTheYearByYearViewModelFactory
{
    public const string Season = "2025";

    public static BowlerOfTheYearByYearViewModel Create(
        string? season = null,
        Dictionary<string, string>? winnersByCategory = null)
            => new()
            {
                Season = season ?? Season,
                WinnersByCategory = winnersByCategory ?? new Dictionary<string, string>
                {
                    { "Overall", "Jane Smith" },
                    { "Senior", "John Doe" }
                }
            };

    public static BowlerOfTheYearByYearViewModel Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<BowlerOfTheYearByYearViewModel> Bogus(
        int count,
        int? seed = null)
    {
        Faker<BowlerOfTheYearByYearViewModel> faker = new Bogus.Faker<BowlerOfTheYearByYearViewModel>()
            .CustomInstantiator(f => new BowlerOfTheYearByYearViewModel
            {
                Season = f.Date.Past(50).Year.ToString(CultureInfo.CurrentCulture),
                WinnersByCategory = new Dictionary<string, string>
                {
                    { "Overall", f.Name.FullName() },
                    { "Senior", f.Name.FullName() },
                    { "Junior", f.Name.FullName() }
                }
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
