using Microsoft.EntityFrameworkCore;
using Neba.Domain.Contact;
using Neba.Website.Application.BowlingCenters;

namespace Neba.Website.Infrastructure.Database.Repositories;

internal sealed class WebsiteBowlingCenterQueryRepository(
    WebsiteDbContext dbContext)
        : IWebsiteBowlingCenterQueryRepository
{
    private readonly WebsiteDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BowlingCenterDto>> ListBowlingCentersAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.BowlingCenters
            .Select(bc => new BowlingCenterDto
            {
                Name = bc.Name,
                Street = bc.Address.Street,
                Unit = bc.Address.Unit,
                City = bc.Address.City,
                State = UsState.FromValue(bc.Address.Region),
                ZipCode = bc.Address.PostalCode,
                PhoneNumber = $"{bc.PhoneNumber.CountryCode}{bc.PhoneNumber.Number}",
                Extension = bc.PhoneNumber.Extension,
                Latitude = bc.Address.Coordinates!.Latitude,
                Longitude = bc.Address.Coordinates!.Longitude,
                IsClosed = bc.IsClosed
            })
            .ToListAsync(cancellationToken);
    }
}
