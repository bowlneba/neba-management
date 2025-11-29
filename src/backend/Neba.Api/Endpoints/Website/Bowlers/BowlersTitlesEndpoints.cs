using ErrorOr;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Neba.Domain.Bowlers;

namespace Neba.Api.Endpoints.Website.Bowlers;

#pragma warning disable S2325 // Extension methods should be static
internal static class BowlersTitlesEndpoints
{
    private static readonly string[] s_tags = ["titles","bowlers","website","champions"];

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBowlerTitlesEndpoints()
        {
            app
                .MapGetBowlerTitlesEndpoint()
                .MapGetBowlersTitlesEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlersTitlesEndpoint()
        {
            app.MapGet(
                "/titles",
                async (
                    IQueryHandler<GetTitlesQuery, IReadOnlyCollection<BowlerTitleDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetTitlesQuery();

                    ErrorOr<IReadOnlyCollection<BowlerTitleDto>> result = await queryHandler.HandleAsync(query, cancellationToken);

                    if (result.IsError)
                    {
                        return result.Problem();
                    }

                    IReadOnlyCollection<GetTitlesResponse> response = result.Value.Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetTitles")
                .WithSummary("Get all NEBA titles won by bowlers.")
                .WithDescription("Retrieves a list of all titles won by bowlers, including bowler and tournament details. Results are returned as a collection of title records.")
                .Produces<CollectionResponse<GetTitlesResponse>>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest, "application/problem+json")
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json")
                .WithTags(s_tags);

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlerTitlesEndpoint()
        {
            app.MapGet(
                "/{bowlerId:guid}/titles",
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
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json")
                .WithTags(s_tags);

            return app;
        }
    }
}
