
namespace Neba.Api.Endpoints.Website.Bowlers;

internal static class BowlersEndpoints
{
    #pragma warning disable S2325 // Extension methods should be static
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBowlersEndpoints()
        {
            RouteGroupBuilder bowlerGroup
                = app
                    .MapGroup("/bowlers")
                    .WithTags("bowlers","website")
                    .AllowAnonymous();

            bowlerGroup
                .MapBowlerTitlesEndpoints();

            return app;
        }
    }
}
