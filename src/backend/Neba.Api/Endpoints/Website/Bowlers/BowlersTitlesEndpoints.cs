using ErrorOr;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitles;
using Neba.Contracts;
using Neba.Contracts.Website.Bowlers;
using Neba.Domain.Bowlers;

namespace Neba.Api.Endpoints.Website.Bowlers;

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
                "/{bowlerId:guid}/titles",
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

                    BowlerTitlesResponse response = result.Value!.ToResponseModel();

                    return TypedResults.Ok(ApiResponse.Create(response));
                })
                .WithName("GetBowlerTitles")
                .WithSummary("Get all NEBA titles for a specific bowler.")
                .WithDescription("Retrieves all NEBA titles won by a specific bowler, including month, year, and tournament type for each title. Results are returned as a collection of title records for the specified bowler.")
                .Produces<ApiResponse<BowlerTitlesResponse>>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
                .ProducesProblem(StatusCodes.Status400BadRequest, ContentTypes.ApplicationProblemJson)
                .ProducesProblem(StatusCodes.Status404NotFound, ContentTypes.ApplicationProblemJson)
                .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypes.ApplicationProblemJson)
                .WithTags(s_tags);

            return app;
        }
    }
}
