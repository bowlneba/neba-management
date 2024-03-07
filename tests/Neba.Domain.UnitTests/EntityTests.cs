namespace Neba.Domain.UnitTests;

public class EntityTests
{
    [Fact]
    public void WhenConstructedWithDefaultValue_ExceptionIsThrown()
    {
        var exception = Record.Exception(() => new TestEntity(default));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void Equals_WhenOtherIsNull_ReturnsFalse()
    {
        var entity = new TestEntity(1);

        var result = entity.Equals(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenOtherIsNotEntity_ReturnsFalse()
    {
        var entity = new TestEntity(1);

        var result = entity.Equals(new object());

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenOtherIsEntityAndIdsAreNotEqual_ReturnsFalse()
    {
        var entity = new TestEntity(1);
        var other = new TestEntity(2);

        var result = entity.Equals(other);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenOtherIsEntityAndIdsAreEqual_ReturnsTrue()
    {
        var entity = new TestEntity(1);
        var other = new TestEntity(1);

        var result = entity.Equals(other);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenOtherIsEntityAndIdsAreEqualAndOtherValueIsNot_ReturnsTrue()
    {
        var entity = new TestEntity(1) { OtherValue = 1 };
        var other = new TestEntity(1) { OtherValue = 2 };

        var result = entity.Equals(other);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_ReturnsIdValue()
    {
        var entity = new TestEntity(1);

        var result = entity.GetHashCode();

        result.Should().Be(1.GetHashCode());
    
    }
}

internal sealed class TestEntity : Entity<int>
{
    public TestEntity(int id) : base(id)
    {
    }

    public int OtherValue { get; init; }
}