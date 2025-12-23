using System.Net.Mime;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Neba.Application.Messaging;
using Neba.Contracts;
using Neba.Infrastructure.Documents.Sse;
using Neba.Infrastructure.Http;
using Neba.Website.Application.Documents.Bylaws;

namespace Neba.Website.Endpoints.Documents;

#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable S2325 // Extension methods should be static
internal static class DocumentEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapDocumentEndpoints()
        {
            app
                .MapGetBylawsEndpoint()
                .MapRefreshBylawsCacheEndpoint()
                .MapBylawsRefreshStatusSseEndpoint();

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

                    return TypedResults.Content(result, MediaTypeNames.Text.Html);
                })
                .WithName("GetBylaws")
                .WithSummary("Get the NEBA Bylaws document.")
                .WithDescription("Retrieves the NEBA Bylaws document as an HTML string.")
                .Produces<string>(StatusCodes.Status200OK, MediaTypeNames.Text.Html)
                .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
                .WithTags("documents", "website");

            return app;
        }

        private IEndpointRouteBuilder MapRefreshBylawsCacheEndpoint()
        {
            app.MapPost(
                "/bylaws/refresh",
                async (
                    ICommandHandler<RefreshBylawsCacheCommand, string> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RefreshBylawsCacheCommand();
                    ErrorOr<string> jobIdResult = await commandHandler.HandleAsync(command, cancellationToken);

                    if(jobIdResult.IsError)
                    {
                        return jobIdResult.Problem();
                    }

                    return TypedResults.Ok(ApiResponse.Create(jobIdResult.Value));
                })
                .WithName("RefreshBylawsCache")
                .WithSummary("Refresh the cached NEBA Bylaws document.")
                .WithDescription("Refreshes the cached version of the NEBA Bylaws document.")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
                .WithTags("documents", "website");

            return app;
        }

        private IEndpointRouteBuilder MapBylawsRefreshStatusSseEndpoint()
        {
            app.MapGet(
                "/bylaws/refresh/status",
                DocumentRefreshSseStreamHandler.CreateStreamHandler("bylaws"))
                .WithName("BylawsRefreshStatus")
                .WithSummary("Stream bylaws document refresh status updates via SSE")
                .WithDescription("Subscribes to real-time status updates for bylaws document refresh operations using Server-Sent Events.")
                .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
                .WithTags("documents", "website", "sse");

            return app;
        }
    }
}
