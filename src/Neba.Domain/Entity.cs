using Ardalis.GuardClauses;

namespace Neba.Domain;

/// <summary>
/// Represents a base entity with an identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public abstract class Entity<TId>
    where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Gets the identifier of the entity.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected Entity(TId id)
    {
        Id = Guard.Against.Default(id);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>
    /// A hash code for the current object.
    /// </returns>
    public override int GetHashCode()
        => Id.GetHashCode();

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// true if the specified object is equal to the current object; otherwise, false.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Entity<TId> other && other.Id.Equals(Id);
}