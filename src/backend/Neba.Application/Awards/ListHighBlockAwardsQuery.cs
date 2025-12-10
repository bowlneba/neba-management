using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards;

/// <summary>
/// Query to retrieve a read-only collection of <see cref="HighBlockAwardDto"/> objects.
/// </summary>
public sealed record ListHighBlockAwardsQuery
    : IQuery<IReadOnlyCollection<HighBlockAwardDto>>;
