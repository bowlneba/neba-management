using Neba.Application.Messaging;

namespace Neba.Application.Awards.HighBlock;

/// <summary>
/// Query to retrieve a read-only collection of <see cref="HighBlockAwardDto"/> objects.
/// </summary>
public sealed record ListHigh5GameBlockAwardsQuery
    : IQuery<IReadOnlyCollection<HighBlockAwardDto>>;
