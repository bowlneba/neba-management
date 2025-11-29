
using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitleCounts;
using Neba.Contracts;
using Neba.Contracts.History.Titles;

namespace Neba.Api.Endpoints.Website.History.Titles;

internal static class TitlesEndpoints
{
    #pragma warning disable S2325 // Extension methods should be static
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTitlesEndpoints()
        {
            RouteGroupBuilder titleGroup
                = app
                    .MapGroup("/titles")
                    .WithTags("website", "history", "titles");

            titleGroup
                .MapGetTitlesEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetTitlesEndpoint()
        {
            app.MapGet(
                "/",
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
                .ProducesProblem(StatusCodes.Status500InternalServerError, "application/problem+json");

            return app;
        }
    }
}
