using Neba.Domain.Awards;

namespace Neba.UnitTests.Awards;

public sealed class BowlerOfTheYearCategoryTests
{
    [Fact]
    public void BowlerOfTheYearCategory_ShouldHaveSixCategories()
    {
        // Arrange & Act
        IReadOnlyCollection<BowlerOfTheYearCategory> categories = BowlerOfTheYearCategory.List;

        // Assert
        categories.Count.ShouldBe(6);
    }

    [Theory]
    [InlineData("Bowler of the Year", 1)]
    [InlineData("Woman Bowler of the Year", 2)]
    [InlineData("Senior Bowler of the Year", 50)]
    [InlineData("Super Senior Bowler of the Year", 60)]
    [InlineData("Rookie of the Year", 10)]
    [InlineData("Youth Bowler of the Year", 20)]
    public void BowlerOfTheYearCategory_ShouldHaveCorrectNameAndValue(string expectedName, int expectedValue)
    {
        // Arrange & Act
        BowlerOfTheYearCategory? category = BowlerOfTheYearCategory.List
            .SingleOrDefault(c => c.Name == expectedName && c.Value == expectedValue);

        // Assert
        category.ShouldNotBeNull();
        category.Name.ShouldBe(expectedName);
        category.Value.ShouldBe(expectedValue);
    }
}
