using ErrorOr;
using Neba.Domain;
using Neba.Tests;

namespace Neba.UnitTests.Domain;

public sealed class NameTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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
