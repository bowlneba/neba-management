namespace Neba.Domain.UnitTests;

public sealed class AggregateRootTests
{
    [Fact]
    public void Constructor_WhenConstructed_ShouldBeAnEntity()
    {
        // Arrange
        var id = 1;

        // Act
        var aggregateRoot = new TestAggregateRoot(id);

        // Assert
        aggregateRoot.Should().BeAssignableTo<Entity<int>>();
    }
}

public sealed class TestAggregateRoot
    : AggregateRoot<int>
{
    public TestAggregateRoot(int id)
        : base(id)
    { }
}