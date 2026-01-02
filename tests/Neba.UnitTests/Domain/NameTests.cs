using ErrorOr;
using Neba.Domain;
using Neba.Tests;

namespace Neba.UnitTests.Domain;

[Trait("Category", "Unit")]
[Trait("Component", "Domain")]

public sealed class NameTests
{
    [Theory(DisplayName = "Returns error when first name is null or whitespace")]
    [InlineData(null, TestDisplayName = "Null first name is rejected")]
    [InlineData("", TestDisplayName = "Empty first name is rejected")]
    [InlineData(" ", TestDisplayName = "Whitespace-only first name is rejected")]
    public void Create_ShouldReturnAnError_WhenFirstNameIsNullOrWhitespace(string? firstName)
    {
#nullable disable
        // Act
        ErrorOr<Name> result = Name.Create(firstName, NameFactory.LastName);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(NameErrors.FirstNameRequired);
#nullable enable
    }

    [Theory(DisplayName = "Returns error when last name is null or whitespace")]
    [InlineData(null, TestDisplayName = "Null last name is rejected")]
    [InlineData("", TestDisplayName = "Empty last name is rejected")]
    [InlineData(" ", TestDisplayName = "Whitespace-only last name is rejected")]
    public void Create_ShouldReturnAnError_WhenLastNameIsNullOrWhitespace(string? lastName)
    {
#nullable disable

        // Act
        ErrorOr<Name> result = Name.Create(NameFactory.FirstName, lastName);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(NameErrors.LastNameRequired);
#nullable enable
    }

    [Fact(DisplayName = "Create should return multiple errors when first and last name are invalid")]
    public void Create_ShouldReturnMultipleErrors_WhenFirstAndLastNameAreInvalid()
    {
        // Arrange
        const string firstName = " ";
        const string lastName = "";

        // Act
        ErrorOr<Name> result = Name.Create(firstName, lastName);

        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(NameErrors.FirstNameRequired);
        result.Errors.ShouldContain(NameErrors.LastNameRequired);
    }

    [Fact(DisplayName = "Create should return Name when valid first and last name provided")]
    public void Create_ShouldReturnName_WhenValidFirstAndLastNameProvided()
    {
        // Act
        ErrorOr<Name> result = Name.Create(NameFactory.FirstName, NameFactory.LastName);

        // Assert
        result.IsError.ShouldBeFalse();

        Name name = result.Value;
        name.FirstName.ShouldBe(NameFactory.FirstName);
        name.LastName.ShouldBe(NameFactory.LastName);
        name.MiddleName.ShouldBeNull();
        name.Suffix.ShouldBeNull();
        name.Nickname.ShouldBeNull();
    }

    [Fact(DisplayName = "Create should return Name when all values are provided")]
    public void Create_ShouldReturnName_WhenAllValuesAreProvided()
    {
        // Act
        ErrorOr<Name> result = Name.Create(
            NameFactory.FirstName,
            NameFactory.LastName,
            NameFactory.MiddleName,
            NameFactory.Suffix,
            NameFactory.Nickname
        );

        // Assert
        result.IsError.ShouldBeFalse();

        Name name = result.Value;
        name.FirstName.ShouldBe(NameFactory.FirstName);
        name.LastName.ShouldBe(NameFactory.LastName);
        name.MiddleName.ShouldBe(NameFactory.MiddleName);
        name.Suffix.ShouldBe(NameFactory.Suffix);
        name.Nickname.ShouldBe(NameFactory.Nickname);
    }

    [Fact(DisplayName = "ToLegalName should return correct format when only first and last name provided")]
    public void ToLegalName_ShouldReturnCorrectFormat_WhenOnlyFirstAndLastNameProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName);

        // Act
        string legalName = name.ToLegalName();

        // Assert
        legalName.ShouldBe($"{NameFactory.FirstName} {NameFactory.LastName}");
    }

    [Fact(DisplayName = "ToLegalName should return correct format when middle initial is provided")]
    public void ToLegalName_ShouldReturnCorrectFormat_WhenMiddleInitialIsProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            middleName: NameFactory.MiddleName);

        // Act
        string legalName = name.ToLegalName();

        // Assert
        legalName.ShouldBe($"{NameFactory.FirstName} {NameFactory.MiddleName} {NameFactory.LastName}");
    }

    [Fact(DisplayName = "ToLegalName should return correct format when suffix is provided")]
    public void ToLegalName_ShouldReturnCorrectFormat_WhenSuffixIsProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            suffix: NameFactory.Suffix);

        // Act
        string legalName = name.ToLegalName();

        // Assert
        legalName.ShouldBe($"{NameFactory.FirstName} {NameFactory.LastName}, {NameFactory.Suffix}");
    }

    [Fact(DisplayName = "ToLegalName should return correct format when middle name and suffix are provided")]
    public void ToLegalName_ShouldReturnCorrectFormat_WhenMiddleNameAndSuffixAreProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            middleName: NameFactory.MiddleName,
            suffix: NameFactory.Suffix);

        // Act
        string legalName = name.ToLegalName();

        // Assert
        legalName.ShouldBe($"{NameFactory.FirstName} {NameFactory.MiddleName} {NameFactory.LastName}, {NameFactory.Suffix}");
    }

    [Fact(DisplayName = "ToLegalName should return correct format when nickname is provided")]
    public void ToLegalName_ShouldReturnCorrectFormat_WhenNicknameIsProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            nickname: NameFactory.Nickname);

        // Act
        string legalName = name.ToLegalName();

        // Assert
        legalName.ShouldBe($"{NameFactory.FirstName} {NameFactory.LastName}");
    }

    [Fact(DisplayName = "ToDisplayName should return correct format when nickname is provided")]
    public void ToDisplayName_ShouldReturnCorrectFormat_WhenNicknameIsProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            nickname: NameFactory.Nickname);

        // Act
        string displayName = name.ToDisplayName();

        // Assert
        displayName.ShouldBe($"{NameFactory.Nickname} {NameFactory.LastName}");
    }

    [Fact(DisplayName = "ToDisplayName should return correct format when nickname is not provided")]
    public void ToDisplayName_ShouldReturnCorrectFormat_WhenNicknameIsNotProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName);

        // Act
        string displayName = name.ToDisplayName();

        // Assert
        displayName.ShouldBe($"{NameFactory.FirstName} {NameFactory.LastName}");
    }

    [Fact(DisplayName = "ToDisplayName should return correct format when all values are provided")]
    public void ToDisplayName_ShouldReturnCorrectFormat_WhenAllValuesAreProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            middleName: NameFactory.MiddleName,
            suffix: NameFactory.Suffix,
            nickname: NameFactory.Nickname);

        // Act
        string displayName = name.ToDisplayName();

        // Assert
        displayName.ShouldBe($"{NameFactory.Nickname} {NameFactory.LastName}");
    }

    [Fact(DisplayName = "ToFormalName should return correct format when all values are provided")]
    public void ToFormalName_ShouldReturnCorrectFormat_WhenAllValuesAreProvided()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            middleName: NameFactory.MiddleName,
            suffix: NameFactory.Suffix,
            nickname: NameFactory.Nickname);

        // Act
        string formalName = name.ToFormalName();

        // Assert
        formalName.ShouldBe($"{NameFactory.FirstName} {NameFactory.LastName}");
    }

    [Fact(DisplayName = "ToString should return legal name")]
    public void ToString_ShouldReturnLegalName()
    {
        // Arrange
        Name name = NameFactory.Create(
            firstName: NameFactory.FirstName,
            lastName: NameFactory.LastName,
            nickname: NameFactory.Nickname);

        // Act
        string nameString = name.ToString();

        // Assert
        nameString.ShouldBe(name.ToLegalName());
    }
}
