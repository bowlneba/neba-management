using Neba.Domain.Awards;
using Neba.Website.Infrastructure.Database.Converters;

namespace Neba.UnitTests.Website.Infrastructure.Database.Converters;

public sealed class HallOfFameCategoryValueConverterTests
{
    private readonly HallOfFameCategoryValueConverter _converter = new();

    [Fact]
    public void ConvertToProvider_SingleCategory_SuperiorPerformance_ReturnsCorrectValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.SuperiorPerformance];

        // Act
        int result = (int)_converter.ConvertToProvider(categories)!;

        // Assert
        result.ShouldBe(1); // SuperiorPerformance has value 1
    }

    [Fact]
    public void ConvertToProvider_SingleCategory_MeritoriousService_ReturnsCorrectValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.MeritoriousService];

        // Act
        int result = (int)_converter.ConvertToProvider(categories)!;

        // Assert
        result.ShouldBe(2); // MeritoriousService has value 2
    }

    [Fact]
    public void ConvertToProvider_SingleCategory_FriendOfNeba_ReturnsCorrectValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.FriendOfNeba];

        // Act
        int result = (int)_converter.ConvertToProvider(categories)!;

        // Assert
        result.ShouldBe(4); // FriendOfNeba has value 4
    }

    [Fact]
    public void ConvertToProvider_MultipleCategories_SuperiorAndMeritorious_ReturnsCorrectValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.SuperiorPerformance, HallOfFameCategory.MeritoriousService];

        // Act
        int result = (int)_converter.ConvertToProvider(categories)!;

        // Assert
        result.ShouldBe(3); // 1 | 2 = 3
    }

    [Fact]
    public void ConvertToProvider_MultipleCategories_SuperiorAndFriend_ReturnsCorrectValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.SuperiorPerformance, HallOfFameCategory.FriendOfNeba];

        // Act
        int result = (int)_converter.ConvertToProvider(categories)!;

        // Assert
        result.ShouldBe(5); // 1 | 4 = 5
    }

    [Fact]
    public void ConvertToProvider_AllThreeCategories_ReturnsCorrectValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [HallOfFameCategory.SuperiorPerformance, HallOfFameCategory.MeritoriousService, HallOfFameCategory.FriendOfNeba];

        // Act
        int result = (int)_converter.ConvertToProvider(categories)!;

        // Assert
        result.ShouldBe(7); // 1 | 2 | 4 = 7
    }

    [Fact]
    public void ConvertToProvider_EmptyCollection_ReturnsZero()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> categories = [];

        // Act
        int result = (int)_converter.ConvertToProvider(categories)!;

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void ConvertFromProvider_Value1_ReturnsSuperiorPerformance()
    {
        // Arrange
        const int value = 1;

        // Act
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(value)!;

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(HallOfFameCategory.SuperiorPerformance);
    }

    [Fact]
    public void ConvertFromProvider_Value2_ReturnsMeritoriousService()
    {
        // Arrange
        const int value = 2;

        // Act
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(value)!;

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(HallOfFameCategory.MeritoriousService);
    }

    [Fact]
    public void ConvertFromProvider_Value4_ReturnsFriendOfNeba()
    {
        // Arrange
        const int value = 4;

        // Act
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(value)!;

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(HallOfFameCategory.FriendOfNeba);
    }

    [Fact]
    public void ConvertFromProvider_Value3_ReturnsSuperiorAndMeritorious()
    {
        // Arrange
        const int value = 3; // 1 | 2

        // Act
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(value)!;

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(HallOfFameCategory.SuperiorPerformance);
        result.ShouldContain(HallOfFameCategory.MeritoriousService);
    }

    [Fact]
    public void ConvertFromProvider_Value5_ReturnsSuperiorAndFriend()
    {
        // Arrange
        const int value = 5; // 1 | 4

        // Act
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(value)!;

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(HallOfFameCategory.SuperiorPerformance);
        result.ShouldContain(HallOfFameCategory.FriendOfNeba);
    }

    [Fact]
    public void ConvertFromProvider_Value7_ReturnsAllThreeCategories()
    {
        // Arrange
        const int value = 7; // 1 | 2 | 4

        // Act
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(value)!;

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldContain(HallOfFameCategory.SuperiorPerformance);
        result.ShouldContain(HallOfFameCategory.MeritoriousService);
        result.ShouldContain(HallOfFameCategory.FriendOfNeba);
    }

    [Fact]
    public void ConvertFromProvider_Value0_ReturnsNoneCategory()
    {
        // Arrange
        const int value = 0;

        // Act
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(value)!;

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(HallOfFameCategory.None);
    }

    [Fact]
    public void RoundTrip_SingleCategory_MaintainsValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> original = [HallOfFameCategory.MeritoriousService];

        // Act
        int intermediate = (int)_converter.ConvertToProvider(original)!;
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(intermediate)!;

        // Assert
        result.Count.ShouldBe(1);
        result.Single().ShouldBe(HallOfFameCategory.MeritoriousService);
    }

    [Fact]
    public void RoundTrip_MultipleCategories_MaintainsValue()
    {
        // Arrange
        IReadOnlyCollection<HallOfFameCategory> original = [HallOfFameCategory.SuperiorPerformance, HallOfFameCategory.FriendOfNeba];

        // Act
        int intermediate = (int)_converter.ConvertToProvider(original)!;
        IReadOnlyCollection<HallOfFameCategory> result = (IReadOnlyCollection<HallOfFameCategory>)_converter.ConvertFromProvider(intermediate)!;

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(HallOfFameCategory.SuperiorPerformance);
        result.ShouldContain(HallOfFameCategory.FriendOfNeba);
    }
}
