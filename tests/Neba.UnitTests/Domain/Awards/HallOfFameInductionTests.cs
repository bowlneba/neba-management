using Neba.Domain;
using Neba.Domain.Awards;
using Neba.Domain.Identifiers;
using Neba.Tests;
using Neba.Website.Domain.Awards;

namespace Neba.UnitTests.Domain.Awards;
[Trait("Category", "Unit")]
[Trait("Component", "Domain.Awards")]

public sealed class HallOfFameInductionTests
{
    [Fact(DisplayName = "Constructor should generate valid HallOfFameId")]
    public void Constructor_ShouldGenerateValidHallOfFameId()
    {
        // Act
        HallOfFameInduction induction = new HallOfFameInduction();

        // Assert
        induction.Id.Value.ShouldNotBe(Ulid.Empty);
    }

    [Fact(DisplayName = "Constructor should generate unique IDs")]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Act
        HallOfFameInduction induction1 = new HallOfFameInduction();
        HallOfFameInduction induction2 = new HallOfFameInduction();

        // Assert
        induction1.Id.ShouldNotBe(induction2.Id);
    }

    [Fact(DisplayName = "Categories default value should be empty collection")]
    public void Categories_DefaultValue_ShouldBeEmptyCollection()
    {
        // Act
        HallOfFameInduction induction = new HallOfFameInduction();

        // Assert
        induction.Categories.ShouldNotBeNull();
        induction.Categories.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Categories should be read-only collection")]
    public void Categories_ShouldBeReadOnlyCollection()
    {
        // Act
        HallOfFameInduction induction = new HallOfFameInduction
        {
            Categories = [HallOfFameCategory.SuperiorPerformance]
        };

        // Assert
        induction.Categories.ShouldBeAssignableTo<IReadOnlyCollection<HallOfFameCategory>>();
    }

    [Fact(DisplayName = "Photo default value should be null")]
    public void Photo_DefaultValue_ShouldBeNull()
    {
        // Act
        HallOfFameInduction induction = new HallOfFameInduction();

        // Assert
        induction.Photo.ShouldBeNull();
    }

    [Fact(DisplayName = "Photo can be set to StoredFile")]
    public void Photo_CanBeSetToStoredFile()
    {
        // Arrange
        StoredFile photo = StoredFileFactory.Create(container: "photos", path: "inductee.jpg");

        // Act
        HallOfFameInduction induction = new HallOfFameInduction
        {
            Photo = photo
        };

        // Assert
        induction.Photo.ShouldNotBeNull();
        induction.Photo.ShouldBe(photo);
    }

    [Fact(DisplayName = "Year can be initialized")]
    public void Year_CanBeInitialized()
    {
        // Arrange
        const int year = 2024;

        // Act
        HallOfFameInduction induction = new HallOfFameInduction
        {
            Year = year
        };

        // Assert
        induction.Year.ShouldBe(year);
    }

    [Fact(DisplayName = "Categories can be initialized with multiple categories")]
    public void Categories_CanBeInitializedWithMultipleCategories()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.SuperiorPerformance, HallOfFameCategory.MeritoriousService];

        // Act
        HallOfFameInduction induction = new HallOfFameInduction
        {
            Categories = categories
        };

        // Assert
        induction.Categories.Count.ShouldBe(2);
        induction.Categories.ShouldContain(HallOfFameCategory.SuperiorPerformance);
        induction.Categories.ShouldContain(HallOfFameCategory.MeritoriousService);
    }

    [Fact(DisplayName = "All properties can be initialized together")]
    public void AllProperties_CanBeInitializedTogether()
    {
        // Arrange
        const int year = 2020;
        StoredFile photo = StoredFileFactory.Create();
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.FriendOfNeba];

        // Act
        HallOfFameInduction induction = new HallOfFameInduction
        {
            Year = year,
            Photo = photo,
            Categories = categories
        };

        // Assert
        induction.Id.Value.ShouldNotBe(Ulid.Empty);
        induction.Year.ShouldBe(year);
        induction.Photo.ShouldBe(photo);
        induction.Categories.Count.ShouldBe(1);
        induction.Categories.Single().ShouldBe(HallOfFameCategory.FriendOfNeba);
    }

    [Fact(DisplayName = "Two inductions with same ID should be equal")]
    public void Equality_TwoInductionsWithSameId_ShouldBeEqual()
    {
        // Arrange
        HallOfFameId id = HallOfFameId.New();
        HallOfFameInduction induction1 = new HallOfFameInduction
        {
            Id = id,
            Year = 2024
        };
        HallOfFameInduction induction2 = new HallOfFameInduction
        {
            Id = id,
            Year = 2025
        };

        // Act & Assert
        induction1.Equals(induction2).ShouldBeTrue();
        induction1.GetHashCode().ShouldBe(induction2.GetHashCode());
    }

    [Fact(DisplayName = "Two inductions with different IDs should not be equal")]
    public void Equality_TwoInductionsWithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        HallOfFameInduction induction1 = new HallOfFameInduction { Year = 2024 };
        HallOfFameInduction induction2 = new HallOfFameInduction { Year = 2024 };

        // Act & Assert
        induction1.Equals(induction2).ShouldBeFalse();
        induction1.GetHashCode().ShouldNotBe(induction2.GetHashCode());
    }
}
