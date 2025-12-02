using Neba.Domain.Abstractions;

namespace Neba.UnitTests.Abstractions;

public sealed class AggregateTests
{
    [Fact]
    public void Aggregate_ShouldBeAnEntity()
    {
        // Arrange
        var aggregate = new TestAggregate(1);

        // Act & Assert
        aggregate.ShouldBeAssignableTo<Entity<int>>();
    }
}

internal sealed class TestAggregate(int id)
        : Aggregate<int>(id);
