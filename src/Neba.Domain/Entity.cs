using Ardalis.GuardClauses;

namespace Neba.Domain;

public abstract class Entity<TId>
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; }

    protected Entity(TId id)
    {
        Id = Guard.Against.Default(id);
    }

    public override bool Equals(object? obj)
        => obj is not null && obj.GetType() == GetType() && obj is Entity<TId> other && other.Id.Equals(Id);

    public override int GetHashCode()
        => Id.GetHashCode();
}