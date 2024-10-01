namespace Neba.Domain.UnitTests;

public sealed class EntityTests
{
    [Fact]
    public void Constructor_WhenConstructedWithDefaultValue_ShouldThrowAnException()
    {
        // Arrange
        var id = default(int);

        // Act
        var exception = Record.Exception(() => new TestEntity(id));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenConstructedWithAValidValue_ShouldInstanciateTheClass()
    {
        // Arrange
        var id = 1;

        // Act
        var entity = new TestEntity(id);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Equals_WhenOtherIsNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(1);

        // Act
        var result = entity.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenOtherIsNotEntity_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(1);

        // Act
        var result = entity.Equals(new object());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenOtherIsEntityAndIdsAreEqual_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(1);
        var other = new TestEntity(1);

        // Act
        var result = entity.Equals(other);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenOtherIsEntityAndIdsAreEqualAndOtherValueIsNot_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(1) { OtherValue = 1 };
        var other = new TestEntity(1) { OtherValue = 2 };

        // Act
        var result = entity.Equals(other);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WhenCalled_ShouldReturnTheHashCodeOfTheId()
    {
        // Arrange
        var id = 1;
        var entity = new TestEntity(id);

        // Act
        var hashCode = entity.GetHashCode();

        // Assert
        hashCode.Should().Be(id.GetHashCode());
    }
}

public sealed class TestEntity
    : Entity<int>
{
    public TestEntity(int id)
        : base(id)
    { }

    public int OtherValue { get; init; }
}