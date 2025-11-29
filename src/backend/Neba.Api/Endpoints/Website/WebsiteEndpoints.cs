using Neba.Api.Endpoints.Website.Bowlers;

namespace Neba.Api.Endpoints.Website;

internal static class WebsiteEndpoints
{
    #pragma warning disable S2325 // Extension methods should be static
    #pragma warning disable CA1034 // Nested types should not be visible
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapWebsiteEndpoints()
        {
            app.MapBowlersEndpoints();

            return app;
        }
    }
}
