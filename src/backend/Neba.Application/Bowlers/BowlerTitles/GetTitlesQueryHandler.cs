using ErrorOr;
using Neba.Application.Abstractions.Messaging;

namespace Neba.Application.Bowlers.BowlerTitles;

internal sealed class GetTitlesQueryHandler(IWebsiteBowlerQueryRepository websiteBowlerQueryRepository)
        : IQueryHandler<GetTitlesQuery, IReadOnlyCollection<BowlerTitleDto>>
{
    private readonly IWebsiteBowlerQueryRepository _websiteBowlerQueryRepository = websiteBowlerQueryRepository;

    public async Task<ErrorOr<IReadOnlyCollection<BowlerTitleDto>>> HandleAsync(GetTitlesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BowlerTitleDto> titles = await _websiteBowlerQueryRepository.GetTitlesAsync(cancellationToken);

        return titles.ToErrorOr();
    }
}
