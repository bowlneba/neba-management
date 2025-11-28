
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitleCounts;

internal sealed record GetTitlesQuery
    : IQuery<IReadOnlyCollection<BowlerTitleDto>>;
