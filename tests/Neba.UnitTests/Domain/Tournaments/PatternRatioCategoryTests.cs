using Neba.Domain.Tournaments;

namespace Neba.UnitTests.Domain.Tournaments;

public sealed class PatternRatioCategoryTests
{
    [Fact(DisplayName = "There are three defined PatternRatio instances")]
    public void PatternRatio_ShouldHaveThreeInstances()
    {
        // Act
        IReadOnlyCollection<PatternRatioCategory> allPatternRatios = PatternRatioCategory.List;

        // Assert
        allPatternRatios.Count.ShouldBe(3);
    }

    [Theory(DisplayName = "Pattern Ratio has the correct name and value")]
    [InlineData("Sport", 1, null, 4)]
    [InlineData("Challenge", 2, 4, 8)]
    [InlineData("Recreation", 3, 8, null)]
    public void PatternRatio_ShouldHaveCorrectProperties(string name, int expectedValue, int? expectedMinimumRatio, int? expectedMaximumRatio)
    {
        // Act
        PatternRatioCategory patternRatio = PatternRatioCategory.FromName(name);

        // Assert
        patternRatio.Name.ShouldBe(name);
        patternRatio.Value.ShouldBe(expectedValue);
        patternRatio.MinimumRatio.ShouldBe(expectedMinimumRatio);
        patternRatio.MaximumRatio.ShouldBe(expectedMaximumRatio);
    }
}
