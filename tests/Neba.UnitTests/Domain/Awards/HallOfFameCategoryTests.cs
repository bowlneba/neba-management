using Neba.Domain;
using Neba.Domain.Awards;

namespace Neba.UnitTests.Domain.Awards;

public sealed class HallOfFameCategoryTests
{
    [Theory(DisplayName = "Hall of Fame Categories Have Correct Names and Values")]
    [InlineData("None", 0)]
    [InlineData("Superior Performance", 1)]
    [InlineData("Meritorious Service", 2)]
    [InlineData("Friend of NEBA", 4)]
    public void HallOfFameCategory_HasCorrectNameAndValue(string expectedName, int expectedValue)
    {
        // Arrange & Act
        var category = HallOfFameCategory.FromValue(expectedValue).ToList();

        // Assert
        category.Count.ShouldBe(1);
        category[0].Name.ShouldBe(expectedName);
        category[0].Value.ShouldBe(expectedValue);

    }

    [Fact(DisplayName = "Hall of Fame Category List Contains All Categories")]
    public void HallOfFameCategory_ListContainsAllCategories()
    {
        // Arrange & Act
        IReadOnlyCollection<HallOfFameCategory> categories = HallOfFameCategory.List;

        // Assert
        categories.Count.ShouldBe(4);
        categories.ShouldContain(HallOfFameCategory.None);
        categories.ShouldContain(HallOfFameCategory.SuperiorPerformance);
        categories.ShouldContain(HallOfFameCategory.MeritoriousService);
        categories.ShouldContain(HallOfFameCategory.FriendOfNeba);
    }

    [Fact(DisplayName = "Hall of Fame Category Combination Works Correctly")]
    public void HallOfFameCategory_CombinationWorksCorrectly()
    {
        // Arrange
        int combinedCategory = HallOfFameCategory.SuperiorPerformance | HallOfFameCategory.FriendOfNeba;

        // Act
        List<HallOfFameCategory> categories = [.. HallOfFameCategory.FromValue(combinedCategory)];

        // Assert
        combinedCategory.ShouldBe(5);
        categories.Count.ShouldBe(2);
        categories.ShouldContain(HallOfFameCategory.SuperiorPerformance);
        categories.ShouldContain(HallOfFameCategory.FriendOfNeba);
    }
}
