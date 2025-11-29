using ErrorOr;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Contracts;
using Neba.Contracts.History.Champions;
using Neba.Domain.Bowlers;

namespace Neba.Api.Endpoints.Website.History.Champions;

internal static class ChampionsEndpoints
{
    #pragma warning disable S2325 // Extension methods should be static
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapChampionsEndpoints()
        {
            RouteGroupBuilder championGroup
                = app
                    .MapGroup("/champions")
                    .WithTags("website", "history", "champions");

            championGroup
                .MapGetBowlerTitleCountsEndpoint()
                .MapGetBowlerTitlesEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlerTitleCountsEndpoint()
        {
            app.MapGet(
                "/",
                async (
                    IQueryHandler<GetBowlerTitleCountsQuery, IReadOnlyCollection<BowlerTitleCountDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetBowlerTitleCountsQuery();

                    ErrorOr<IReadOnlyCollection<BowlerTitleCountDto>> result = await queryHandler.HandleAsync(query, cancellationToken);

                    if (result.IsError)
                    {
                        return result.Problem();
                    }

                    IReadOnlyCollection<GetBowlerTitleCountsResponse> response = result.Value.Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetBowlerTitleCounts")
                .WithSummary("Get all bowlers and their total number of NEBA titles.")
                .WithDescription("Retrieves a list of all bowlers and the total number of NEBA titles each has won. Results are returned as a collection of bowler title count records.")
                .Produces<CollectionResponse<GetBowlerTitleCountsResponse>>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest, "application/problem+json")
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json");

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlerTitlesEndpoint()
        {
            app.MapGet(
                "/{bowlerId:guid}",
                async (
                    IQueryHandler<GetBowlerTitlesQuery, BowlerTitlesDto?> queryHandler,
                    BowlerId bowlerId,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetBowlerTitlesQuery() { BowlerId = bowlerId };

                    ErrorOr<BowlerTitlesDto?> result = await queryHandler.HandleAsync(query, cancellationToken);

                    if (result.IsError)
                    {
                        return result.Problem();
                    }

                    GetBowlerTitlesResponse response = result.Value!.ToResponseModel();

                    return TypedResults.Ok(ApiResponse.Create(response));
                })
                .WithName("GetBowlerTitles")
                .WithSummary("Get all NEBA titles for a specific bowler.")
                .WithDescription("Retrieves all NEBA titles won by a specific bowler, including month, year, and tournament type for each title. Results are returned as a collection of title records for the specified bowler.")
                .Produces<ApiResponse<GetBowlerTitlesResponse>>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest, "application/problem+json")
                .ProducesProblem(StatusCodes.Status404NotFound, "application/problem+json")
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json");

            return app;
        }
    }
}
