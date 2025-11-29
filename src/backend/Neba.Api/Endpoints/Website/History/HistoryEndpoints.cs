using Neba.Api.Endpoints.Website.History.Titles;

namespace Neba.Api.Endpoints.Website.History;

internal static class HistoryEndpoints
{
    #pragma warning disable S2325 // Extension methods should be static
    #pragma warning disable CA1034 // Nested types should not be visible
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapHistoryEndpoints()
        {
            RouteGroupBuilder historyApp = app.MapGroup("/history")
                .AllowAnonymous();

            historyApp
                .MapTitlesEndpoints();

            return app;
        }
    }
}
