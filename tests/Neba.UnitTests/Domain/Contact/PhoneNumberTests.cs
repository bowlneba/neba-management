using ErrorOr;
using Neba.Domain.Contact;

namespace Neba.UnitTests.Domain.Contact;

public sealed class PhoneNumberTests
{
    #region North American Phone Number Tests - Happy Path

    [Fact(DisplayName = "Creates valid North American phone number with 10 digits")]
    public void CreateNorthAmerican_WithValid10DigitNumber_ReturnsPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "5551234567";

        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.CountryCode.ShouldBe("1");
        result.Value.Number.ShouldBe("5551234567");
        result.Value.Extension.ShouldBeNull();
    }

    [Fact(DisplayName = "Creates valid North American phone number with 11 digits starting with 1")]
    public void CreateNorthAmerican_With11DigitsStartingWith1_ReturnsPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "15551234567";

        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.CountryCode.ShouldBe("1");
        result.Value.Number.ShouldBe("5551234567");
        result.Value.Extension.ShouldBeNull();
    }

    [Theory(DisplayName = "Creates valid North American phone number with various formatting")]
    [InlineData("555-123-4567", TestDisplayName = "Accepts dashes in phone number")]
    [InlineData("(555) 123-4567", TestDisplayName = "Accepts parentheses and dashes")]
    [InlineData("555.123.4567", TestDisplayName = "Accepts dots in phone number")]
    [InlineData("+1 555-123-4567", TestDisplayName = "Accepts plus sign and country code")]
    [InlineData("1-555-123-4567", TestDisplayName = "Accepts leading 1 with dashes")]
    [InlineData("+1 (555) 123-4567", TestDisplayName = "Accepts full formatting with country code")]
    public void CreateNorthAmerican_WithFormattedNumber_ReturnsPhoneNumber(string formattedNumber)
    {
        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(formattedNumber);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.CountryCode.ShouldBe("1");
        result.Value.Number.ShouldBe("5551234567");
    }

    [Fact(DisplayName = "Creates valid phone number with extension")]
    public void CreateNorthAmerican_WithExtension_ReturnsPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "5551234567";
        const string extension = "123";

        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber, extension);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.CountryCode.ShouldBe("1");
        result.Value.Number.ShouldBe("5551234567");
        result.Value.Extension.ShouldBe("123");
    }

    [Theory(DisplayName = "Creates valid phone number with formatted extension")]
    [InlineData("x123", TestDisplayName = "Extension with x prefix")]
    [InlineData("ext. 123", TestDisplayName = "Extension with ext. prefix")]
    [InlineData("#123", TestDisplayName = "Extension with # prefix")]
    public void CreateNorthAmerican_WithFormattedExtension_ReturnsPhoneNumberWithCleanedExtension(string formattedExtension)
    {
        // Arrange
        const string phoneNumber = "5551234567";

        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber, formattedExtension);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Extension.ShouldBe("123");
    }

    [Theory(DisplayName = "Accepts valid area codes with first digit 2-9")]
    [InlineData("2551234567", TestDisplayName = "Area code starting with 2")]
    [InlineData("3551234567", TestDisplayName = "Area code starting with 3")]
    [InlineData("4551234567", TestDisplayName = "Area code starting with 4")]
    [InlineData("5551234567", TestDisplayName = "Area code starting with 5")]
    [InlineData("6551234567", TestDisplayName = "Area code starting with 6")]
    [InlineData("7551234567", TestDisplayName = "Area code starting with 7")]
    [InlineData("8551234567", TestDisplayName = "Area code starting with 8")]
    [InlineData("9551234567", TestDisplayName = "Area code starting with 9")]
    public void CreateNorthAmerican_WithValidAreaCodes_ReturnsPhoneNumber(string phoneNumber)
    {
        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Number.ShouldBe(phoneNumber);
    }

    #endregion

    #region North American Phone Number Tests - Error Cases

    [Theory(DisplayName = "Rejects null or empty phone number")]
    [InlineData(null, TestDisplayName = "Null phone number is rejected")]
    [InlineData("", TestDisplayName = "Empty phone number is rejected")]
    [InlineData("   ", TestDisplayName = "Whitespace-only phone number is rejected")]
    public void CreateNorthAmerican_WithNullOrEmpty_ReturnsError(string? phoneNumber)
    {
        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber!);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(PhoneNumberErrors.PhoneNumberIsRequired.Code);
        result.FirstError.Description.ShouldBe("Phone number is required.");
    }

    [Theory(DisplayName = "Rejects phone number with invalid length")]
    [InlineData("123", TestDisplayName = "Too short - 3 digits")]
    [InlineData("12345", TestDisplayName = "Too short - 5 digits")]
    [InlineData("123456789", TestDisplayName = "Too short - 9 digits")]
    [InlineData("22345678901", TestDisplayName = "Too long - 11 digits not starting with 1")]
    [InlineData("123456789012", TestDisplayName = "Too long - 12 digits")]
    public void CreateNorthAmerican_WithInvalidLength_ReturnsError(string phoneNumber)
    {
        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("PhoneNumber.InvalidNorthAmericanPhoneNumber");
        result.FirstError.Description.ShouldBe("The provided phone number is not a valid North American phone number.");
    }

    [Theory(DisplayName = "Rejects invalid area codes")]
    [InlineData("0551234567", "055", TestDisplayName = "Area code starting with 0")]
    [InlineData("1551234567", "155", TestDisplayName = "Area code starting with 1")]
    [InlineData("2111234567", "211", TestDisplayName = "211 service code")]
    [InlineData("3111234567", "311", TestDisplayName = "311 service code")]
    [InlineData("4111234567", "411", TestDisplayName = "411 service code")]
    [InlineData("5111234567", "511", TestDisplayName = "511 service code")]
    [InlineData("6111234567", "611", TestDisplayName = "611 service code")]
    [InlineData("7111234567", "711", TestDisplayName = "711 service code")]
    [InlineData("8111234567", "811", TestDisplayName = "811 service code")]
    [InlineData("9111234567", "911", TestDisplayName = "911 service code")]
    public void CreateNorthAmerican_WithInvalidAreaCode_ReturnsError(string phoneNumber, string expectedAreaCode)
    {
        // Act
        ErrorOr<PhoneNumber> result = PhoneNumber.CreateNorthAmerican(phoneNumber);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("PhoneNumber.InvalidNorthAmericanAreaCode");
        result.FirstError.Description.ShouldBe("The area code is not a valid North American area code.");
        result.FirstError.Metadata!["InvalidAreaCode"].ShouldBe(expectedAreaCode);
    }

    #endregion
}
