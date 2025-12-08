
namespace Neba.Api.Endpoints.Website.Bowlers;

#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable S2325 // Extension methods should be static

internal static class BowlersEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBowlersEndpoints()
        {
            RouteGroupBuilder bowlerGroup
                = app
                    .MapGroup("/bowlers")
                    .WithTags("bowlers", "website")
                    .AllowAnonymous();

            bowlerGroup
                .MapBowlerTitlesEndpoints();

            return app;
        }
    }
}
