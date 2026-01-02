using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Neba.Contracts;
using Neba.Website.Application.BowlingCenters;
using Neba.Website.Contracts.BowlingCenters;

namespace Neba.Website.Endpoints.BowlingCenters;

#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable S1144 // Remove unused constructor of private type.

internal static class BowlingCenterEndpoints
{
    private static readonly string[] s_tags = ["bowling-centers", "website"];

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBowlingCenterEndpoints()
        {
            RouteGroupBuilder bowlingCenterGroup = app.MapGroup("/bowling-centers");

            bowlingCenterGroup.MapGetBowlingCentersEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlingCentersEndpoint()
        {
            app.MapGet(
                "/",
                async (
                    IWebsiteBowlingCenterQueryRepository repository,
                    CancellationToken cancellationToken) =>
                {
                    IReadOnlyCollection<BowlingCenterDto> result = await repository.ListBowlingCentersAsync(cancellationToken);

                    IReadOnlyCollection<BowlingCenterResponse> response = result
                        .Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetBowlingCenters")
                .WithSummary("Get all New England bowling centers.")
                .WithDescription("Retrieves a list of all bowling centers located in New England, including contact and location details. Results are returned as a collection of bowling center records.")
                .Produces<CollectionResponse<BowlingCenterResponse>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
                .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
                .WithTags(s_tags);

            return app;
        }
    }
}
