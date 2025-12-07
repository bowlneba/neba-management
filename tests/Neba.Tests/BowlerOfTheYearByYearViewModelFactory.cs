using System.Globalization;
using Bogus;
using Neba.Web.Server.History.Awards;

namespace Neba.Tests;

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
            .RuleFor(x => x.Season, f => f.Date.Past(50).Year.ToString(CultureInfo.CurrentCulture))
            .RuleFor(x => x.WinnersByCategory, f => new Dictionary<string, string>
            {
                { "Overall", f.Name.FullName() },
                { "Senior", f.Name.FullName() },
                { "Junior", f.Name.FullName() }
            });

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
