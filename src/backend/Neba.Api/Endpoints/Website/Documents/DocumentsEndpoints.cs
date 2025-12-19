using Neba.Application.Messaging;
using Neba.Application.Documents.Bylaws;

namespace Neba.Api.Endpoints.Website.Documents;

#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable S2325 // Extension methods should be static
internal static class DocumentEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapDocumentEndpoints()
        {
            app
                .MapGetBylawsEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetBylawsEndpoint()
        {
            app.MapGet(
                "/bylaws",
                async (
                    IQueryHandler<GetBylawsQuery, string> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetBylawsQuery();

                    string result = await queryHandler.HandleAsync(query, cancellationToken);

                    return TypedResults.Content(result, ContentTypes.TextHtml);
                })
                .WithName("GetBylaws")
                .WithSummary("Get the NEBA Bylaws document.")
                .WithDescription("Retrieves the NEBA Bylaws document as an HTML string.")
                .Produces<string>(StatusCodes.Status200OK, ContentTypes.TextHtml)
                .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypes.ApplicationProblemJson)
                .WithTags("documents", "website");

            return app;
        }
    }
}
