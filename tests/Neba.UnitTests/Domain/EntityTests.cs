using Neba.Domain;

namespace Neba.UnitTests.Domain;

public sealed class EntityTests
{
    [Fact(DisplayName = "Entities with same ID should be equal")]
    public void EntitiesWithSameId_ShouldBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(1, "A");
        var entity2 = new TestEntity(1, "B");

        // Act
        bool areEqual = entity1.Equals(entity2);

        // Assert
        areEqual.ShouldBeTrue();
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());
    }

    [Fact(DisplayName = "Entities with different IDs should not be equal")]
    public void EntitiesWithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(1, "A");
        var entity2 = new TestEntity(2, "A");

        // Act
        bool areEqual = entity1.Equals(entity2);

        // Assert
        areEqual.ShouldBeFalse();
        entity1.GetHashCode().ShouldNotBe(entity2.GetHashCode());
    }
}

internal sealed class TestEntity(int id, string testProperty)
                : Entity<int>(id)
{
    public string TestProperty { get; }
        = testProperty;
}
