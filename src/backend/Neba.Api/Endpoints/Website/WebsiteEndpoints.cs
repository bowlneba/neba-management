using Neba.Api.Endpoints.Website.Bowlers;
using Neba.Api.Endpoints.Website.Titles;

namespace Neba.Api.Endpoints.Website;

internal static class WebsiteEndpoints
{
#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S1144 // Remove unused constructor of private type.

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapWebsiteEndpoints()
        {
            app
                .MapBowlersEndpoints()
                .MapTitlesEndpoints();

            return app;
        }
    }
}
