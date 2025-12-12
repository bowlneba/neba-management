using Neba.Application.Abstractions.Messaging;
using Neba.Application.Awards.BowlerOfTheYear;
using Neba.Application.Awards.HighAverage;
using Neba.Application.Awards.HighBlock;
using Neba.Contracts;
using Neba.Contracts.Website.Awards;

namespace Neba.Api.Endpoints.Website.Awards;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class AwardsEndpoints
{
    private static readonly string[] s_tags = ["awards", "website"];

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapAwardsEndpoints()
        {
            RouteGroupBuilder awardGroup = app.MapGroup("/awards");

            awardGroup
                .MapGetBowlerOfTheYearWinnersEndpoint()
                .MapHighBlockAwardWinnersEndpoint()
                .MapHighAverageAwardWinnersEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlerOfTheYearWinnersEndpoint()
        {
            app.MapGet(
                "/bowler-of-the-year",
                async (
                    IQueryHandler<ListBowlerOfTheYearAwardsQuery, IReadOnlyCollection<BowlerOfTheYearAwardDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new ListBowlerOfTheYearAwardsQuery();

                    IReadOnlyCollection<BowlerOfTheYearAwardDto> result = await queryHandler.HandleAsync(query, cancellationToken);

                    IReadOnlyCollection<BowlerOfTheYearResponse> response = result
                        .OrderBy(boy => boy.Season)
                        .ThenBy(boy => boy.Category.Value)
                        .Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetBowlerOfTheYearAwards")
                .WithSummary("Get all NEBA Bowler of the Year winners.")
                .WithDescription("Retrieves a list of all Bowler of the Year awards, including bowler and award details. Results are returned as a collection of award records.")
                .Produces<CollectionResponse<BowlerOfTheYearResponse>>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
                .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypes.ApplicationProblemJson)
                .WithTags(s_tags);

            return app;
        }

        private IEndpointRouteBuilder MapHighBlockAwardWinnersEndpoint()
        {
            app.MapGet(
                "/high-block",
                async (
                    IQueryHandler<ListHigh5GameBlockAwardsQuery, IReadOnlyCollection<HighBlockAwardDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new ListHigh5GameBlockAwardsQuery();

                    IReadOnlyCollection<HighBlockAwardDto> result = await queryHandler.HandleAsync(query, cancellationToken);

                    IReadOnlyCollection<HighBlockAwardResponse> response = result
                        .OrderBy(hb => hb.Season)
                        .Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetHighBlockAwards")
                .WithSummary("Get all NEBA High Block winners.")
                .WithDescription("Retrieves a list of all High Block awards, including bowler and award details. Results are returned as a collection of award records.")
                .Produces<CollectionResponse<HighBlockAwardResponse>>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
                .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypes.ApplicationProblemJson)
                .WithTags(s_tags);

            return app;
        }

        private IEndpointRouteBuilder MapHighAverageAwardWinnersEndpoint()
        {
            app.MapGet(
                "/high-average",
                async (
                    IQueryHandler<ListHighAverageAwardsQuery, IReadOnlyCollection<HighAverageAwardDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new ListHighAverageAwardsQuery();

                    IReadOnlyCollection<HighAverageAwardDto> result = await queryHandler.HandleAsync(query, cancellationToken);

                    IReadOnlyCollection<HighAverageAwardResponse> response = result
                        .OrderBy(ha => ha.Season)
                        .Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetHighAverageAwards")
                .WithSummary("Get all NEBA High Average winners.")
                .WithDescription("Retrieves a list of all High Average awards, including bowler and award details. Results are returned as a collection of award records.")
                .Produces<CollectionResponse<HighAverageAwardResponse>>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
                .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypes.ApplicationProblemJson)
                .WithTags(s_tags);

            return app;
        }
    }
}
