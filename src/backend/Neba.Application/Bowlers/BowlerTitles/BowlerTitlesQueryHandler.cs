using ErrorOr;
using Neba.Application.Abstractions.Messaging;
using Neba.Domain.Bowlers;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class BowlerTitlesQueryHandler(
    IWebsiteBowlerQueryRepository websiteBowler)
        : IQueryHandler<BowlerTitlesQuery, ErrorOr<BowlerTitlesDto>>
{
    private readonly IWebsiteBowlerQueryRepository _websiteBowler = websiteBowler;

    public async Task<ErrorOr<BowlerTitlesDto>> HandleAsync(
        BowlerTitlesQuery request,
        CancellationToken cancellationToken)
    {
        BowlerTitlesDto? bowler = await _websiteBowler
            .GetBowlerTitlesAsync(request.BowlerId, cancellationToken);

        if (bowler is null)
        {
            return BowlerErrors.BowlerNotFound(request.BowlerId);
        }

        return bowler;
    }
}
