using Neba.Website.Application.BowlingCenters;
using Neba.Website.Contracts.BowlingCenters;

namespace Neba.Website.Endpoints.BowlingCenters;

#pragma warning disable S1144 // Unused private types or members should be removed --- IGNORE ---

internal static class BowlingCentersMappingExtensions
{
    extension(BowlingCenterDto dto)
    {
        public BowlingCenterResponse ToResponse()
        {
            return new BowlingCenterResponse
            {
                Name = dto.Name,
                Street = dto.Street,
                Unit = dto.Unit,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                PhoneNumber = dto.PhoneNumber,
                PhoneExtension = dto.Extension,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsClosed = dto.IsClosed
            };
        }
    }
}
