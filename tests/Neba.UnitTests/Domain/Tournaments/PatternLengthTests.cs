using Neba.Domain.Tournaments;

namespace Neba.UnitTests.Domain.Tournaments;

public sealed class PatternLengthTests
{
    [Fact(DisplayName = "There are three defined PatternLength instances")]
    public void PatternLength_ShouldHaveThreeInstances()
    {
        // Act
        IReadOnlyCollection<PatternLength> allPatternLengths = PatternLength.List;

        // Assert
        allPatternLengths.Count.ShouldBe(3);
    }

    [Theory(DisplayName = "Pattern Length has the correct name, value, minimum, and maximum lengths")]
    [InlineData("ShortPattern", 1, null, 37)]
    [InlineData("MediumPattern", 2, 38, 42)]
    [InlineData("LongPattern", 3, 43, null)]
    public void PatternLength_ShouldHaveCorrectProperties(string name, int expectedValue, int? expectedMinLength, int? expectedMaxLength)
    {
        // Act
        PatternLength patternLength = PatternLength.FromName(name);

        // Assert
        patternLength.Name.ShouldBe(name);
        patternLength.Value.ShouldBe(expectedValue);
        patternLength.MinimumLength.ShouldBe(expectedMinLength);
        patternLength.MaximumLength.ShouldBe(expectedMaxLength);
    }
}
