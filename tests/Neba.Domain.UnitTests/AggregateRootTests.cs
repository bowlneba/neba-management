namespace Neba.Domain.UnitTests;

public sealed class AggregateRootTests
{
    [Fact]
    public void AggregateRoot_IsAnEntity()
    {
        var aggregateRoot = new TestAggregateRoot(1);

        aggregateRoot.Should().BeAssignableTo<Entity<int>>();
    }

    [Fact]
    public void PopDomainEvents_WhenCalled_ReturnsAndClearsDomainEvents()
    {
        var aggregateRoot = new TestAggregateRoot(1);
        aggregateRoot.AddDomainEvent();

        var domainEvents = aggregateRoot.PopDomainEvents();

        domainEvents.Should().ContainSingle();
        aggregateRoot.PopDomainEvents().Should().BeEmpty();
    }
}

internal sealed class TestAggregateRoot : AggregateRoot<int>
{
    public TestAggregateRoot(int id)
        : base(id)
    {
    }

    internal void AddDomainEvent()
    {
        AddDomainEvent(Substitute.For<IDomainEvent>());
    }
}