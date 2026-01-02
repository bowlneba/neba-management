using ErrorOr;
using Neba.Domain.Contact;
using Neba.Domain.Geography;

namespace Neba.UnitTests.Domain.Contact;

public sealed class AddressTests
{
    #region US Address Tests

    [Fact(DisplayName = "Creates valid US address with all required fields")]
    public void CreateUsAddress_WithValidRequiredFields_ReturnsAddress()
    {
        // Arrange
        const string street = "123 Main St";
        const string city = "Springfield";
        UsState state = UsState.Illinois;
        const string zipCode = "62701";

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, state, zipCode);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Street.ShouldBe(street);
        result.Value.Unit.ShouldBeNull();
        result.Value.City.ShouldBe(city);
        result.Value.Region.ShouldBe("IL");
        result.Value.Country.ShouldBe(Country.UnitedStates);
        result.Value.PostalCode.ShouldBe(zipCode);
        result.Value.Coordinates.ShouldBeNull();
    }

    [Fact(DisplayName = "Creates valid US address with optional unit and coordinates")]
    public void CreateUsAddress_WithUnitAndCoordinates_ReturnsAddress()
    {
        // Arrange
        const string street = "456 Oak Ave";
        const string unit = "Apt 2B";
        const string city = "Boston";
        UsState state = UsState.Massachusetts;
        const string zipCode = "02101";
        ErrorOr<Coordinates> coordinatesResult = Coordinates.Create(42.3601, -71.0589);
        Coordinates coordinates = coordinatesResult.Value;

        // Act
        ErrorOr<Address> result = Address.Create(street, unit, city, state, zipCode, coordinates);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Street.ShouldBe(street);
        result.Value.Unit.ShouldBe(unit);
        result.Value.City.ShouldBe(city);
        result.Value.Region.ShouldBe("MA");
        result.Value.Country.ShouldBe(Country.UnitedStates);
        result.Value.PostalCode.ShouldBe(zipCode);
        result.Value.Coordinates.ShouldNotBeNull();
        result.Value.Coordinates.ShouldBe(coordinates);
    }

    [Theory(DisplayName = "Creates valid US address with different ZIP code formats and normalizes them")]
    [InlineData("12345", "12345", TestDisplayName = "5-digit ZIP code is valid and stored without formatting")]
    [InlineData("12345-6789", "123456789", TestDisplayName = "ZIP+4 format with dash is normalized to remove dash")]
    [InlineData("123456789", "123456789", TestDisplayName = "ZIP+4 format without dash is stored as-is")]
    public void CreateUsAddress_WithValidZipCodeFormats_ReturnsAddress(string inputZipCode, string expectedZipCode)
    {
        // Arrange
        const string street = "789 Elm St";
        const string city = "Portland";
        UsState state = UsState.Oregon;

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, state, inputZipCode);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.PostalCode.ShouldBe(expectedZipCode);
    }

    [Fact(DisplayName = "Rejects US address with null state")]
    public void CreateUsAddress_WithNullState_ThrowsArgumentNullException()
    {
        // Arrange
        const string street = "123 Main St";
        const string city = "Springfield";
        const string zipCode = "62701";

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Address.Create(street, null, city, (UsState)null!, zipCode));
    }

    [Theory(DisplayName = "Rejects US address with invalid street")]
    [InlineData(null, TestDisplayName = "Null street is rejected")]
    [InlineData("", TestDisplayName = "Empty street is rejected")]
    [InlineData("   ", TestDisplayName = "Whitespace-only street is rejected")]
    public void CreateUsAddress_WithInvalidStreet_ReturnsError(string? street)
    {
        // Arrange
        const string city = "Springfield";
        UsState state = UsState.Illinois;
        const string zipCode = "62701";

        // Act
        ErrorOr<Address> result = Address.Create(street!, null, city, state, zipCode);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.StreetIsRequired.Code);
    }

    [Theory(DisplayName = "Rejects US address with invalid city")]
    [InlineData(null, TestDisplayName = "Null city is rejected")]
    [InlineData("", TestDisplayName = "Empty city is rejected")]
    [InlineData("   ", TestDisplayName = "Whitespace-only city is rejected")]
    public void CreateUsAddress_WithInvalidCity_ReturnsError(string? city)
    {
        // Arrange
        const string street = "123 Main St";
        UsState state = UsState.Illinois;
        const string zipCode = "62701";

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city!, state, zipCode);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.CityIsRequired.Code);
    }

    [Theory(DisplayName = "Rejects US address with invalid ZIP code")]
    [InlineData(null, TestDisplayName = "Null ZIP code is rejected")]
    [InlineData("", TestDisplayName = "Empty ZIP code is rejected")]
    [InlineData("   ", TestDisplayName = "Whitespace-only ZIP code is rejected")]
    public void CreateUsAddress_WithNullOrEmptyZipCode_ReturnsError(string? zipCode)
    {
        // Arrange
        const string street = "123 Main St";
        const string city = "Springfield";
        UsState state = UsState.Illinois;

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, state, zipCode!);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.PostalCodeIsRequired.Code);
    }

    [Theory(DisplayName = "Rejects US address with malformed ZIP code")]
    [InlineData("1234", TestDisplayName = "4-digit ZIP is invalid")]
    [InlineData("123456", TestDisplayName = "6-digit ZIP is invalid")]
    [InlineData("12345-678", TestDisplayName = "ZIP+3 is invalid")]
    [InlineData("ABCDE", TestDisplayName = "Alphabetic ZIP is invalid")]
    [InlineData("12-345", TestDisplayName = "Misplaced hyphen is invalid")]
    public void CreateUsAddress_WithMalformedZipCode_ReturnsError(string zipCode)
    {
        // Arrange
        const string street = "123 Main St";
        const string city = "Springfield";
        UsState state = UsState.Illinois;

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, state, zipCode);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.InvalidPostalCode(zipCode).Code);
        result.FirstError.Description.ShouldBe("The postal code is not valid.");
        result.FirstError.Metadata!["InvalidPostalCode"].ShouldBe(zipCode);
    }

    #endregion

    #region Canadian Address Tests

    [Fact(DisplayName = "Creates valid Canadian address with all required fields")]
    public void CreateCanadianAddress_WithValidRequiredFields_ReturnsAddress()
    {
        // Arrange
        const string street = "123 Maple St";
        const string city = "Toronto";
        CanadianProvince province = CanadianProvince.Ontario;
        const string postalCode = "M5H 2N2";

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, province, postalCode);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Street.ShouldBe(street);
        result.Value.Unit.ShouldBeNull();
        result.Value.City.ShouldBe(city);
        result.Value.Region.ShouldBe("ON");
        result.Value.Country.ShouldBe(Country.Canada);
        result.Value.PostalCode.ShouldBe("M5H2N2");
        result.Value.Coordinates.ShouldBeNull();
    }

    [Fact(DisplayName = "Creates valid Canadian address with optional unit and coordinates")]
    public void CreateCanadianAddress_WithUnitAndCoordinates_ReturnsAddress()
    {
        // Arrange
        const string street = "456 Oak Ave";
        const string unit = "Suite 100";
        const string city = "Vancouver";
        CanadianProvince province = CanadianProvince.BritishColumbia;
        const string postalCode = "V6B 1A1";
        ErrorOr<Coordinates> coordinatesResult = Coordinates.Create(49.2827, -123.1207);
        Coordinates coordinates = coordinatesResult.Value;

        // Act
        ErrorOr<Address> result = Address.Create(street, unit, city, province, postalCode, coordinates);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Street.ShouldBe(street);
        result.Value.Unit.ShouldBe(unit);
        result.Value.City.ShouldBe(city);
        result.Value.Region.ShouldBe("BC");
        result.Value.Country.ShouldBe(Country.Canada);
        result.Value.PostalCode.ShouldBe("V6B1A1");
        result.Value.Coordinates.ShouldNotBeNull();
        result.Value.Coordinates.ShouldBe(coordinates);
    }

    [Theory(DisplayName = "Creates valid Canadian address with different postal code formats and normalizes them")]
    [InlineData("K1A 0B1", "K1A0B1", TestDisplayName = "Postal code with space is normalized to remove space and uppercase")]
    [InlineData("K1A0B1", "K1A0B1", TestDisplayName = "Postal code without space is stored uppercase without formatting")]
    [InlineData("k1a 0b1", "K1A0B1", TestDisplayName = "Lowercase postal code is normalized to uppercase without space")]
    [InlineData("K1a 0B1", "K1A0B1", TestDisplayName = "Mixed case postal code is normalized to uppercase without space")]
    [InlineData("k1a0b1", "K1A0B1", TestDisplayName = "Lowercase without space is normalized to uppercase")]
    public void CreateCanadianAddress_WithValidPostalCodeFormats_ReturnsAddress(string inputPostalCode, string expectedPostalCode)
    {
        // Arrange
        const string street = "789 Pine Rd";
        const string city = "Ottawa";
        CanadianProvince province = CanadianProvince.Ontario;

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, province, inputPostalCode);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.PostalCode.ShouldBe(expectedPostalCode);
    }

    [Fact(DisplayName = "Normalizes Canadian postal code to uppercase with space")]
    public void CreateCanadianAddress_WithLowercasePostalCode_NormalizesToUppercaseWithSpace()
    {
        // Arrange
        const string street = "123 Maple St";
        const string city = "Toronto";
        CanadianProvince province = CanadianProvince.Ontario;
        const string postalCode = "m5h2n2";

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, province, postalCode);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.PostalCode.ShouldBe("M5H2N2");
    }

    [Fact(DisplayName = "Rejects Canadian address with null province")]
    public void CreateCanadianAddress_WithNullProvince_ThrowsArgumentNullException()
    {
        // Arrange
        const string street = "123 Maple St";
        const string city = "Toronto";
        const string postalCode = "M5H 2N2";

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Address.Create(street, null, city, (CanadianProvince)null!, postalCode));
    }

    [Theory(DisplayName = "Rejects Canadian address with invalid street")]
    [InlineData(null, TestDisplayName = "Null street is rejected")]
    [InlineData("", TestDisplayName = "Empty street is rejected")]
    [InlineData("   ", TestDisplayName = "Whitespace-only street is rejected")]
    public void CreateCanadianAddress_WithInvalidStreet_ReturnsError(string? street)
    {
        // Arrange
        const string city = "Toronto";
        CanadianProvince province = CanadianProvince.Ontario;
        const string postalCode = "M5H 2N2";

        // Act
        ErrorOr<Address> result = Address.Create(street!, null, city, province, postalCode);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.StreetIsRequired.Code);
    }

    [Theory(DisplayName = "Rejects Canadian address with invalid city")]
    [InlineData(null, TestDisplayName = "Null city is rejected")]
    [InlineData("", TestDisplayName = "Empty city is rejected")]
    [InlineData("   ", TestDisplayName = "Whitespace-only city is rejected")]
    public void CreateCanadianAddress_WithInvalidCity_ReturnsError(string? city)
    {
        // Arrange
        const string street = "123 Maple St";
        CanadianProvince province = CanadianProvince.Ontario;
        const string postalCode = "M5H 2N2";

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city!, province, postalCode);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.CityIsRequired.Code);
    }

    [Theory(DisplayName = "Rejects Canadian address with invalid postal code")]
    [InlineData(null, TestDisplayName = "Null postal code is rejected")]
    [InlineData("", TestDisplayName = "Empty postal code is rejected")]
    [InlineData("   ", TestDisplayName = "Whitespace-only postal code is rejected")]
    public void CreateCanadianAddress_WithNullOrEmptyPostalCode_ReturnsError(string? postalCode)
    {
        // Arrange
        const string street = "123 Maple St";
        const string city = "Toronto";
        CanadianProvince province = CanadianProvince.Ontario;

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, province, postalCode!);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.PostalCodeIsRequired.Code);
    }

    [Theory(DisplayName = "Rejects Canadian address with malformed postal code")]
    [InlineData("12345", TestDisplayName = "Numeric postal code is invalid")]
    [InlineData("K1A", TestDisplayName = "Too short postal code is invalid")]
    [InlineData("K1A 0B", TestDisplayName = "Incomplete postal code is invalid")]
    [InlineData("D1A 0B1", TestDisplayName = "Invalid first letter D is rejected")]
    [InlineData("K1D 0B1", TestDisplayName = "Invalid third letter D is rejected")]
    [InlineData("K1A 0D1", TestDisplayName = "Invalid fifth letter D is rejected")]
    [InlineData("K1A-0B1", TestDisplayName = "Hyphen separator is invalid")]
    public void CreateCanadianAddress_WithMalformedPostalCode_ReturnsError(string postalCode)
    {
        // Arrange
        const string street = "123 Maple St";
        const string city = "Toronto";
        CanadianProvince province = CanadianProvince.Ontario;

        // Act
        ErrorOr<Address> result = Address.Create(street, null, city, province, postalCode);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(AddressErrors.InvalidPostalCode(postalCode).Code);
        result.FirstError.Description.ShouldBe("The postal code is not valid.");
        result.FirstError.Metadata!["InvalidPostalCode"].ShouldBe(postalCode);
    }

    #endregion
}
