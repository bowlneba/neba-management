using ErrorOr;
using Neba.Application.Messaging;
using Neba.Website.Domain.Bowlers;

namespace Neba.Website.Application.Bowlers.BowlerTitles;

internal sealed class BowlerTitlesQueryHandler(
    IWebsiteBowlerQueryRepository websiteBowler)
        : IQueryHandler<BowlerTitlesQuery, ErrorOr<BowlerTitlesDto>>
{
    public async Task<ErrorOr<BowlerTitlesDto>> HandleAsync(
        BowlerTitlesQuery request,
        CancellationToken cancellationToken)
    {
        BowlerTitlesDto? bowler = await websiteBowler
            .GetBowlerTitlesAsync(request.BowlerId, cancellationToken);

        if (bowler is null)
        {
            return BowlerErrors.BowlerNotFound(request.BowlerId);
        }

        return bowler;
    }
}
