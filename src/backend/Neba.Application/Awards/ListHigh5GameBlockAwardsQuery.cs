using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards;

/// <summary>
/// Query to retrieve a read-only collection of <see cref="HighBlockAwardDto"/> objects.
/// </summary>
public sealed record ListHigh5GameBlockAwardsQuery
    : IQuery<IReadOnlyCollection<HighBlockAwardDto>>;
