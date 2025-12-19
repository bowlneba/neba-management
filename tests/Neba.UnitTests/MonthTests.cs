using Neba.Domain;

namespace Neba.UnitTests;

public sealed class MonthTests
{
    [Fact]
    public void Month_ShouldHave12DifferentMonths()
    {
        // Arrange & Act
        IReadOnlyCollection<Month> months = Month.List;

        // Assert
        months.Count.ShouldBe(12);
    }

    [Theory]
    [InlineData("January", 1, "Jan")]
    [InlineData("February", 2, "Feb")]
    [InlineData("March", 3, "Mar")]
    [InlineData("April", 4, "Apr")]
    [InlineData("May", 5, "May")]
    [InlineData("June", 6, "Jun")]
    [InlineData("July", 7, "Jul")]
    [InlineData("August", 8, "Aug")]
    [InlineData("September", 9, "Sep")]
    [InlineData("October", 10, "Oct")]
    [InlineData("November", 11, "Nov")]
    [InlineData("December", 12, "Dec")]
    public void Month_ShouldHaveCorrectProperties(string expectedName, int expectedValue, string expectedShort)
    {
        // Arrange
        Month month = Month.List.Single(m => m.Name == expectedName);

        // Act & Assert
        month.ShouldNotBeNull();
        month.Value.ShouldBe(expectedValue);
        month.ToShortString().ShouldBe(expectedShort);
    }
}
