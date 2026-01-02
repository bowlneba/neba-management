using Neba.Domain;

namespace Neba.UnitTests.Domain;
[Trait("Category", "Unit")]
[Trait("Component", "Domain")]

public sealed class AggregateTests
{
    [Fact(DisplayName = "Aggregate should be an Entity")]
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
