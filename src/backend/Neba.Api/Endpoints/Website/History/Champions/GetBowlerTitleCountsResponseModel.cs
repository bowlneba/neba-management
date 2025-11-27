using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Domain.Bowlers;

namespace Neba.Api.Endpoints.Website.History.Champions;

/// <summary>
/// Response model representing a bowler and their total number of titles for the champions endpoint.
/// </summary>
public sealed record GetBowlerTitleCountsResponseModel
{
    /// <summary>
    /// Gets the unique identifier of the bowler.
    /// </summary>
    public required BowlerId BowlerId { get; init; }

    /// <summary>
    /// Gets the full name of the bowler.
    /// </summary>
    public required string BowlerName { get; init; }

    /// <summary>
    /// Gets the total number of titles won by the bowler.
    /// </summary>
    public int TitleCount { get; init; }
}

internal static class GetBowlerTitleCountsResponseModelExtensions
{
    extension(BowlerTitleCountDto dto)
    {
        public GetBowlerTitleCountsResponseModel ToResponseModel()
        {
            return new GetBowlerTitleCountsResponseModel
            {
                BowlerId = dto.BowlerId,
                BowlerName = dto.BowlerName,
                TitleCount = dto.TitleCount
            };
        }
    }
}
