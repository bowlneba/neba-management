using Neba.Application.Messaging;

namespace Neba.Application.Awards.HighAverage;

/// <summary>
/// Query requesting the list of all High Average awards.
/// </summary>
/// <remarks>
/// The query returns a read-only collection of <see cref="HighAverageAwardDto"/>.
/// </remarks>
public sealed record ListHighAverageAwardsQuery
    : IQuery<IReadOnlyCollection<HighAverageAwardDto>>;
