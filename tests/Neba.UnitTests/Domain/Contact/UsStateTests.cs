using Neba.Domain.Contact;

namespace Neba.UnitTests.Domain.Contact;

[Trait("Category", "Unit")]
[Trait("Component", "Domain.Contact")]

public sealed class UsStateTests
{
    [Fact(DisplayName = "All states should contain 50 states plus DC")]
    public void AllStates_ShouldContain50StatesPlusDC()
    {
        // Arrange & Act
        IReadOnlyCollection<UsState> allStates = UsState.List;

        // Assert
        allStates.Count.ShouldBe(51); // 50 states + DC
    }

    [Theory(DisplayName = "State abbreviations should be correct")]
    [InlineData("Alabama", "AL", TestDisplayName = "Alabama abbreviation should be AL")]
    [InlineData("Alaska", "AK", TestDisplayName = "Alaska abbreviation should be AK")]
    [InlineData("Arizona", "AZ", TestDisplayName = "Arizona abbreviation should be AZ")]
    [InlineData("Arkansas", "AR", TestDisplayName = "Arkansas abbreviation should be AR")]
    [InlineData("California", "CA", TestDisplayName = "California abbreviation should be CA")]
    [InlineData("Colorado", "CO", TestDisplayName = "Colorado abbreviation should be CO")]
    [InlineData("Connecticut", "CT", TestDisplayName = "Connecticut abbreviation should be CT")]
    [InlineData("Delaware", "DE", TestDisplayName = "Delaware abbreviation should be DE")]
    [InlineData("Florida", "FL", TestDisplayName = "Florida abbreviation should be FL")]
    [InlineData("Georgia", "GA", TestDisplayName = "Georgia abbreviation should be GA")]
    [InlineData("Hawaii", "HI", TestDisplayName = "Hawaii abbreviation should be HI")]
    [InlineData("Idaho", "ID", TestDisplayName = "Idaho abbreviation should be ID")]
    [InlineData("Illinois", "IL", TestDisplayName = "Illinois abbreviation should be IL")]
    [InlineData("Indiana", "IN", TestDisplayName = "Indiana abbreviation should be IN")]
    [InlineData("Iowa", "IA", TestDisplayName = "Iowa abbreviation should be IA")]
    [InlineData("Kansas", "KS", TestDisplayName = "Kansas abbreviation should be KS")]
    [InlineData("Kentucky", "KY", TestDisplayName = "Kentucky abbreviation should be KY")]
    [InlineData("Louisiana", "LA", TestDisplayName = "Louisiana abbreviation should be LA")]
    [InlineData("Maine", "ME", TestDisplayName = "Maine abbreviation should be ME")]
    [InlineData("Maryland", "MD", TestDisplayName = "Maryland abbreviation should be MD")]
    [InlineData("Massachusetts", "MA", TestDisplayName = "Massachusetts abbreviation should be MA")]
    [InlineData("Michigan", "MI", TestDisplayName = "Michigan abbreviation should be MI")]
    [InlineData("Minnesota", "MN", TestDisplayName = "Minnesota abbreviation should be MN")]
    [InlineData("Mississippi", "MS", TestDisplayName = "Mississippi abbreviation should be MS")]
    [InlineData("Missouri", "MO", TestDisplayName = "Missouri abbreviation should be MO")]
    [InlineData("Montana", "MT", TestDisplayName = "Montana abbreviation should be MT")]
    [InlineData("Nebraska", "NE", TestDisplayName = "Nebraska abbreviation should be NE")]
    [InlineData("Nevada", "NV", TestDisplayName = "Nevada abbreviation should be NV")]
    [InlineData("New Hampshire", "NH", TestDisplayName = "New Hampshire abbreviation should be NH")]
    [InlineData("New Jersey", "NJ", TestDisplayName = "New Jersey abbreviation should be NJ")]
    [InlineData("New Mexico", "NM", TestDisplayName = "New Mexico abbreviation should be NM")]
    [InlineData("New York", "NY", TestDisplayName = "New York abbreviation should be NY")]
    [InlineData("North Carolina", "NC", TestDisplayName = "North Carolina abbreviation should be NC")]
    [InlineData("North Dakota", "ND", TestDisplayName = "North Dakota abbreviation should be ND")]
    [InlineData("Ohio", "OH", TestDisplayName = "Ohio abbreviation should be OH")]
    [InlineData("Oklahoma", "OK", TestDisplayName = "Oklahoma abbreviation should be OK")]
    [InlineData("Oregon", "OR", TestDisplayName = "Oregon abbreviation should be OR")]
    [InlineData("Pennsylvania", "PA", TestDisplayName = "Pennsylvania abbreviation should be PA")]
    [InlineData("Rhode Island", "RI", TestDisplayName = "Rhode Island abbreviation should be RI")]
    [InlineData("South Carolina", "SC", TestDisplayName = "South Carolina abbreviation should be SC")]
    [InlineData("South Dakota", "SD", TestDisplayName = "South Dakota abbreviation should be SD")]
    [InlineData("Tennessee", "TN", TestDisplayName = "Tennessee abbreviation should be TN")]
    [InlineData("Texas", "TX", TestDisplayName = "Texas abbreviation should be TX")]
    [InlineData("Utah", "UT", TestDisplayName = "Utah abbreviation should be UT")]
    [InlineData("Vermont", "VT", TestDisplayName = "Vermont abbreviation should be VT")]
    [InlineData("Virginia", "VA", TestDisplayName = "Virginia abbreviation should be VA")]
    [InlineData("Washington", "WA", TestDisplayName = "Washington abbreviation should be WA")]
    [InlineData("West Virginia", "WV", TestDisplayName = "West Virginia abbreviation should be WV")]
    [InlineData("Wisconsin", "WI", TestDisplayName = "Wisconsin abbreviation should be WI")]
    [InlineData("Wyoming", "WY", TestDisplayName = "Wyoming abbreviation should be WY")]
    [InlineData("District of Columbia", "DC", TestDisplayName = "District of Columbia abbreviation should be DC")]
    public void StateAbbreviations_ShouldBeCorrect(string stateName, string expectedAbbreviation)
    {
        // Act
        UsState state = UsState.FromName(stateName);

        // Assert
        state.Name.ShouldBe(stateName);
        state.Value.ShouldBe(expectedAbbreviation);
    }
}
