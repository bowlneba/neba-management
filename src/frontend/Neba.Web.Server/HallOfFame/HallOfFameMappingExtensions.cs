using Neba.Website.Contracts.Awards;

namespace Neba.Web.Server.HallOfFame;

#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class HallOfFameMappingExtensions
{
    extension(HallOfFameInductionResponse dto)
    {
        public HallOfFameInductionViewModel ToViewModel()
        {
            return new HallOfFameInductionViewModel
            {
                BowlerName = dto.BowlerName,
                InductionYear = dto.Year,
                Categories = dto.Categories,
                PhotoUrl = dto.PhotoUrl
            };
        }
    }
}
