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
    private static readonly string[] s_tags = ["titles", "bowlers", "website", "champions"];

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBowlerTitlesEndpoints()
        {
            app
                .MapGetBowlerTitlesEndpoint()
                .MapGetBowlersTitlesEndpoint()
                .MapGetBowlerTitlesSummaryEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlersTitlesEndpoint()
        {
            app.MapGet(
                "/titles",
                async (
                    IQueryHandler<ListBowlerTitlesQuery, IReadOnlyCollection<BowlerTitleDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new ListBowlerTitlesQuery();

                    ErrorOr<IReadOnlyCollection<BowlerTitleDto>> result = await queryHandler.HandleAsync(query, cancellationToken);

                    if (result.IsError)
                    {
                        return result.Problem();
                    }

                    IReadOnlyCollection<BowlerTitleResponse> response = result.Value.Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetTitles")
                .WithSummary("Get all NEBA titles won by bowlers.")
                .WithDescription("Retrieves a list of all titles won by bowlers, including bowler and tournament details. Results are returned as a collection of title records.")
                .Produces<CollectionResponse<BowlerTitleResponse>>(StatusCodes.Status200OK, "application/json")
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
                    IQueryHandler<BowlerTitlesQuery, BowlerTitlesDto?> queryHandler,
                    BowlerId bowlerId,
                    CancellationToken cancellationToken) =>
                {
                    var query = new BowlerTitlesQuery() { BowlerId = bowlerId };

                    ErrorOr<BowlerTitlesDto?> result = await queryHandler.HandleAsync(query, cancellationToken);

                    if (result.IsError)
                    {
                        return result.Problem();
                    }

                    BowlerTitlesResponse response = result.Value!.ToResponseModel();

                    return TypedResults.Ok(ApiResponse.Create(response));
                })
                .WithName("GetBowlerTitles")
                .WithSummary("Get all NEBA titles for a specific bowler.")
                .WithDescription("Retrieves all NEBA titles won by a specific bowler, including month, year, and tournament type for each title. Results are returned as a collection of title records for the specified bowler.")
                .Produces<ApiResponse<BowlerTitlesResponse>>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest, "application/problem+json")
                .ProducesProblem(StatusCodes.Status404NotFound, "application/problem+json")
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json")
                .WithTags(s_tags);

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlerTitlesSummaryEndpoint()
        {
            app.MapGet(
                "/titles/summary",
                async (
                    IQueryHandler<ListBowlerTitleSummariesQuery, IReadOnlyCollection<BowlerTitleSummaryDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new ListBowlerTitleSummariesQuery();

                    ErrorOr<IReadOnlyCollection<BowlerTitleSummaryDto>> result = await queryHandler.HandleAsync(query, cancellationToken);

                    if (result.IsError)
                    {
                        return result.Problem();
                    }

                    IReadOnlyCollection<BowlerTitleSummaryResponse> response = result.Value
                        .Select(dto => dto.ToResponseModel())
                        .ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetBowlersTitlesSummary")
                .WithSummary("Get a summary of NEBA titles for all bowlers.")
                .WithDescription("Retrieves a summary of titles won by all bowlers, including each bowler's unique identifier, name, and total title count. Results are returned as a collection of bowler title summaries.")
                .Produces<CollectionResponse<BowlerTitleSummaryResponse>>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest, "application/problem+json")
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json")
                .WithTags(s_tags);

            return app;
        }
    }
}
