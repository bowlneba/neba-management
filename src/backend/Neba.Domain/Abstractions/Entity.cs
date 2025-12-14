
namespace Neba.Domain.Abstractions;

/// <summary>
/// Represents a domain entity with a unique identity, as defined by Domain-Driven Design (DDD) principles.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
/// </remarks>
/// <param name="id">The unique identifier for the entity.</param>
public abstract class Entity<TId>(TId id)
    where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public TId Id { get; internal init; } = id;

    /// <summary>
    /// Determines whether the specified object is equal to the current entity, based on identity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns><c>true</c> if the specified object is an entity with the same identity; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Returns a hash code for the entity based on its identity.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
        => Id.GetHashCode();
}
