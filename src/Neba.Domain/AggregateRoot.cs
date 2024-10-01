namespace Neba.Domain;

/// <summary>
/// Represents an aggregate root with an identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public abstract class AggregateRoot<TId>
    : Entity<TId>, IAggregateRoot
    where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected AggregateRoot(TId id)
        : base(id)
    { }
}

/// <summary>
/// Marker interface for aggregate roots.
/// </summary>
public interface IAggregateRoot;