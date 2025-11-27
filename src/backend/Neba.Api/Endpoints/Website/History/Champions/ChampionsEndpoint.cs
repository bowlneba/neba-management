using ErrorOr;
using Neba.Application.Abstractions.Messaging;
using Neba.Application.Bowlers.BowlerTitleCounts;

namespace Neba.Api.Endpoints.Website.History.Champions;

internal static class ChampionsEndpoint
{
    #pragma warning disable S2325 // Extension methods should be static
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapChampionsEndpoints()
        {
            app.MapGetBowlerTitleCountsEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBowlerTitleCountsEndpoint()
        {
            app.MapGet(
                "/history/champions", async (
                    IQueryHandler<GetBowlerTitleCountsQuery, IReadOnlyCollection<BowlerTitleCountDto>> queryHandler,
                    CancellationToken cancellationToken) =>
            {
                var query = new GetBowlerTitleCountsQuery();

                ErrorOr<IReadOnlyCollection<BowlerTitleCountDto>> result = await queryHandler.HandleAsync(query, cancellationToken);

                if (result.IsError)
                {
                    return result.Problem();
                }

                return Results.Ok(result.Value.Select(dto => dto.ToResponseModel()));
            });

            return app;
        }
    }
}
