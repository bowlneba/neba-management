using Neba.Domain.Abstractions;

namespace Neba.UnitTests.Abstractions;

public sealed class EntityTests
{
    [Fact]
    public void EntitiesWithSameId_ShouldBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act
        bool areEqual = entity1.Equals(entity2);

        // Assert
        areEqual.ShouldBeTrue();
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());
    }

    [Fact]
    public void EntitiesWithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act
        bool areEqual = entity1.Equals(entity2);

        // Assert
        areEqual.ShouldBeFalse();
        entity1.GetHashCode().ShouldNotBe(entity2.GetHashCode());
    }
}

internal sealed class TestEntity(int id)
        : Entity<int>(id);
