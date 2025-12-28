using Neba.Domain;
using Neba.Domain.Awards;
using Neba.Domain.Identifiers;
using Neba.Tests;
using Neba.Website.Domain.Awards;

namespace Neba.UnitTests.Domain.Awards;

public sealed class HallOfFameInductionTests
{
    [Fact]
    public void Constructor_ShouldGenerateValidHallOfFameId()
    {
        // Act
        HallOfFameInduction induction = new HallOfFameInduction();

        // Assert
        induction.Id.Value.ShouldNotBe(Ulid.Empty);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Act
        HallOfFameInduction induction1 = new HallOfFameInduction();
        HallOfFameInduction induction2 = new HallOfFameInduction();

        // Assert
        induction1.Id.ShouldNotBe(induction2.Id);
    }

    [Fact]
    public void Categories_DefaultValue_ShouldBeEmptyCollection()
    {
        // Act
        HallOfFameInduction induction = new HallOfFameInduction();

        // Assert
        induction.Categories.ShouldNotBeNull();
        induction.Categories.ShouldBeEmpty();
    }

    [Fact]
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

    [Fact]
    public void Photo_DefaultValue_ShouldBeNull()
    {
        // Act
        HallOfFameInduction induction = new HallOfFameInduction();

        // Assert
        induction.Photo.ShouldBeNull();
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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
