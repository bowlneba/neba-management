using Neba.Domain;

namespace Neba.UnitTests.Domain;

[Trait("Category", "Unit")]
[Trait("Component", "Domain")]

public sealed class MonthTests
{
    [Fact(DisplayName = "Month should have 12 different months")]
    public void Month_ShouldHave12DifferentMonths()
    {
        // Arrange & Act
        IReadOnlyCollection<Month> months = Month.List;

        // Assert
        months.Count.ShouldBe(12);
    }

    [Theory(DisplayName = "Has correct properties for each month")]
    [InlineData("January", 1, "Jan", TestDisplayName = "January properties are correct")]
    [InlineData("February", 2, "Feb", TestDisplayName = "February properties are correct")]
    [InlineData("March", 3, "Mar", TestDisplayName = "March properties are correct")]
    [InlineData("April", 4, "Apr", TestDisplayName = "April properties are correct")]
    [InlineData("May", 5, "May", TestDisplayName = "May properties are correct")]
    [InlineData("June", 6, "Jun", TestDisplayName = "June properties are correct")]
    [InlineData("July", 7, "Jul", TestDisplayName = "July properties are correct")]
    [InlineData("August", 8, "Aug", TestDisplayName = "August properties are correct")]
    [InlineData("September", 9, "Sep", TestDisplayName = "September properties are correct")]
    [InlineData("October", 10, "Oct", TestDisplayName = "October properties are correct")]
    [InlineData("November", 11, "Nov", TestDisplayName = "November properties are correct")]
    [InlineData("December", 12, "Dec", TestDisplayName = "December properties are correct")]
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
