namespace Neba.Domain.Abstractions;

/// <summary>
/// Represents a domain aggregate root, which is an entity that serves as the entry point for a cluster of related objects in Domain-Driven Design (DDD).
/// </summary>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
public abstract class Aggregate<TId>(TId id)
    : Entity<TId>(id)
    where TId : struct, IEquatable<TId>;
