namespace Neba.IntegrationTests;

public sealed class PlaceholderIntegrationTest
{
    [Fact]
    public void TemporaryTest()
    {
        const int x = 2 + 3;

        x.ShouldBe(5);
    }
}
