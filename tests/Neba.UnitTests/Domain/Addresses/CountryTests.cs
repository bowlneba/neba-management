using Neba.Domain.Addresses;

namespace Neba.UnitTests.Domain.Addresses;

public sealed class CountryTests
{
    [Fact(DisplayName = "Country should have two predefined countries")]
    public void Country_ShouldHaveTwoPredefinedCountries()
    {
        // Arrange & Act
        IReadOnlyCollection<Country> countries = Country.List;

        // Assert
        countries.Count.ShouldBe(2);
    }

    [Theory(DisplayName = "Country abbreviations should be correct")]
    [InlineData("United States", "US", TestDisplayName = "United States abbreviation should be US")]
    [InlineData("Canada", "CA", TestDisplayName = "Canada abbreviation should be CA")]
    public void Country_ShouldHaveCorrectNameAndValue(string countryName, string expectedValue)
    {
        // Act
        Country country = Country.FromName(countryName);

        // Assert
        country.Name.ShouldBe(countryName);
        country.Value.ShouldBe(expectedValue);
    }
}
