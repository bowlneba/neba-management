using Neba.Application.Messaging;
using Neba.Application.Storage;

namespace Neba.Website.Application.Awards.HallOfFame;

internal sealed class ListHallOfFameInductionsQueryHandler(
    IWebsiteAwardQueryRepository awardQueryRepository,
    IStorageService storageService)
        : IQueryHandler<ListHallOfFameInductionsQuery, IReadOnlyCollection<HallOfFameInductionDto>>
{
    private readonly IWebsiteAwardQueryRepository _awardQueryRepository = awardQueryRepository;
    private readonly IStorageService _storageService = storageService;

    public async Task<IReadOnlyCollection<HallOfFameInductionDto>> HandleAsync(
        ListHallOfFameInductionsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<HallOfFameInductionDto> inductions
            = await _awardQueryRepository.ListHallOfFameInductionsAsync(cancellationToken);

        foreach (HallOfFameInductionDto induction in inductions)
        {
            if (induction.Photo != null)
            {
                induction.PhotoUri = _storageService.GetBlobUri(induction.Photo.Container, induction.Photo.Path);
            }
        }

        return inductions;
    }
}
