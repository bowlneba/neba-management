using Neba.Website.Contracts.BowlingCenters;

namespace Neba.Web.Server.BowlingCenters;

#pragma warning disable S1144 // Extensions should be in static classes
#pragma warning disable S2325 // Methods and properties that don't use their 'this' parameter should be static

internal static class BowlingCenterMappingExtensions
{
    extension(BowlingCenterResponse response)
    {
        public BowlingCenterViewModel ToViewModel()
        {
            return new BowlingCenterViewModel(
                response.Name,
                response.Street,
                response.Unit,
                response.City,
                response.State.Value,
                response.ZipCode,
                response.PhoneNumber,
                response.PhoneExtension,
                response.Latitude,
                response.Longitude,
                response.IsClosed);
        }
    }
}
