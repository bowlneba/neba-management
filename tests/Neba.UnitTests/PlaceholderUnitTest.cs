namespace Neba.Tests;

public sealed class PlaceholderUnitTest
{
    [Fact]
    public void TemporaryTest()
    {
        const int x = 2 * 3;

        x.ShouldBe(6);
    }
}
