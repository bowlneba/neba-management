
using Neba.Domain.Awards;

namespace Neba.UnitTests.Domain.Awards;

[Trait("Category", "Unit")]
[Trait("Component", "Domain.Awards")]

public sealed class BowlerOfTheYearCategoryTests
{
    [Fact(DisplayName = "BowlerOfTheYearCategory should have six categories")]
    public void BowlerOfTheYearCategory_ShouldHaveSixCategories()
    {
        // Arrange & Act
        IReadOnlyCollection<BowlerOfTheYearCategory> categories = BowlerOfTheYearCategory.List;

        // Assert
        categories.Count.ShouldBe(6);
    }

    [Theory(DisplayName = "Has correct name and value for all categories")]
    [InlineData("Bowler of the Year", 1, TestDisplayName = "Bowler of the Year category is correct")]
    [InlineData("Woman Bowler of the Year", 2, TestDisplayName = "Woman Bowler of the Year category is correct")]
    [InlineData("Senior Bowler of the Year", 50, TestDisplayName = "Senior Bowler of the Year category is correct")]
    [InlineData("Super Senior Bowler of the Year", 60, TestDisplayName = "Super Senior Bowler of the Year category is correct")]
    [InlineData("Rookie of the Year", 10, TestDisplayName = "Rookie of the Year category is correct")]
    [InlineData("Youth Bowler of the Year", 20, TestDisplayName = "Youth Bowler of the Year category is correct")]
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
