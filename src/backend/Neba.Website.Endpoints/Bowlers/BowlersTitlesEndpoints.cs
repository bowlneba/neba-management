using System.Net.Mime;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Neba.Application.Messaging;
using Neba.Contracts;
using Neba.Domain.Identifiers;
using Neba.Infrastructure.Http;
using Neba.Website.Application.Bowlers.BowlerTitles;
using Neba.Website.Contracts.Bowlers;

namespace Neba.Website.Endpoints.Bowlers;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class BowlersTitlesEndpoints
{
    private static readonly string[] s_tags = ["bowlers", "website"];

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBowlerTitlesEndpoints()
        {
            app
                .MapGetBowlerTitlesEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlerTitlesEndpoint()
        {
            app.MapGet(
                "/{bowlerId}/titles",
                async (
                    IQueryHandler<BowlerTitlesQuery, ErrorOr<BowlerTitlesDto>> queryHandler,
                    BowlerId bowlerId,
                    CancellationToken cancellationToken) =>
                {
                    var query = new BowlerTitlesQuery() { BowlerId = bowlerId };

                    ErrorOr<BowlerTitlesDto> result = await queryHandler.HandleAsync(query, cancellationToken);

                    if (result.IsError)
                    {
                        return result.Problem();
                    }

                    BowlerTitlesResponse response = result.Value.ToResponseModel();

                    return TypedResults.Ok(ApiResponse.Create(response));
                })
                .WithName("GetBowlerTitles")
                .WithSummary("Get all NEBA titles for a specific bowler.")
                .WithDescription("Retrieves all NEBA titles won by a specific bowler, including month, year, and tournament type for each title. Results are returned as a collection of title records for the specified bowler.")
                .Produces<ApiResponse<BowlerTitlesResponse>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
                .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .ProducesProblem(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
                .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
                .WithTags(s_tags);

            return app;
        }
    }
}
