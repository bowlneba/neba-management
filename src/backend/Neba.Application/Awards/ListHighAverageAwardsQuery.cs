using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Awards;

/// <summary>
/// Query requesting the list of all High Average awards.
/// </summary>
/// <remarks>
/// The query returns a read-only collection of <see cref="HighAverageAwardDto"/>.
/// </remarks>
public sealed record ListHighAverageAwardsQuery
    : IQuery<IReadOnlyCollection<HighAverageAwardDto>>;
