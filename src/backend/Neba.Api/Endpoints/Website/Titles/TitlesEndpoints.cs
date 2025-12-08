using ErrorOr;
using Neba.Api.Endpoints.Website.Bowlers;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;

namespace Neba.Api.Endpoints.Website.Titles;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class TitlesEndpoints
{
    private static readonly string[] s_tags = ["titles", "website", "champions"];

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTitlesEndpoints()
        {
            app
                .MapGetTitlesEndpoint()
                .MapGetTitlesSummaryEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetTitlesEndpoint()
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
                .Produces<CollectionResponse<BowlerTitleResponse>>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
                .ProducesProblem(StatusCodes.Status400BadRequest, ContentTypes.ApplicationProblemJson)
                .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypes.ApplicationProblemJson)
                .WithTags(s_tags);

            return app;
        }

        private IEndpointRouteBuilder MapGetTitlesSummaryEndpoint()
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
                .WithName("GetTitlesSummary")
                .WithSummary("Get a summary of NEBA titles won by bowlers.")
                .WithDescription("Retrieves a summary of titles won by all bowlers, including each bowler's unique identifier, name, and total title count. Results are returned as a collection of bowler title summaries.")
                .Produces<CollectionResponse<BowlerTitleSummaryResponse>>(StatusCodes.Status200OK, "application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest, "application/problem+json")
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json")
                .WithTags(s_tags);

            return app;
        }
    }
}
