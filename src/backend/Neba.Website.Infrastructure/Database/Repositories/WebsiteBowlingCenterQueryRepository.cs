using Microsoft.EntityFrameworkCore;
using Neba.Website.Application.BowlingCenters;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteBowlingCenterQueryRepository
    : IWebsiteBowlingCenterQueryRepository
{
    private readonly WebsiteDbContext _dbContext;

    public WebsiteBowlingCenterQueryRepository(
        WebsiteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<BowlingCenterDto>> ListBowlingCentersAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.BowlingCenters
            .AsNoTracking()
            .Select(bc => new BowlingCenterDto
            {
                Name = bc.Name,
                Street = bc.Address.Street,
                Unit = bc.Address.Unit,
                City = bc.Address.City,
                State = bc.Address.Region,
                ZipCode = bc.Address.PostalCode,
                PhoneNumber = $"{bc.PhoneNumber.CountryCode}{bc.PhoneNumber.Number}",
                Extension = bc.PhoneNumber.Extension,
                Latitude = bc.Address.Coordinates!.Latitude,
                Longitude = bc.Address.Coordinates!.Longitude,
            })
            .ToListAsync(cancellationToken);
    }
}
