using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Neba.Domain;

namespace Neba.Website.Infrastructure.Database.Converters;

internal sealed class HallOfFameCategoryValueConverter
    : ValueConverter<IReadOnlyCollection<HallOfFameCategory>, int>
{
    public HallOfFameCategoryValueConverter()
        : base(
            categories => categories.Aggregate(0, (value, category) => value | category.Value),
            value => HallOfFameCategory.FromValue(value).ToArray().AsReadOnly())
    { }
}
