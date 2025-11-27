using Neba.Api.Endpoints.Website.History.Champions;

namespace Neba.Api.Endpoints.Website.History;

internal static class HistoryEndpoints
{
    #pragma warning disable S2325 // Extension methods should be static
    #pragma warning disable CA1034 // Nested types should not be visible
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapHistoryEndpoints()
        {
            app.MapChampionsEndpoints();
            return app;
        }
    }
}
