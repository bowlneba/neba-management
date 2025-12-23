using System.Net.Mime;
using System.Text;
using System.Text.Json;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Neba.Application.Documents;
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
                "/bylaws/refresh-cache",
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
                DocumentRefreshSseHelper.CreateStreamHandler("bylaws"))
                .WithName("BylawsRefreshStatus")
                .WithSummary("Stream bylaws document refresh status updates via SSE")
                .WithDescription("Subscribes to real-time status updates for bylaws document refresh operations using Server-Sent Events.")
                .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
                .WithTags("documents", "website", "sse");

            return app;
        }
    }
}

/// <summary>
/// Helper class for creating SSE streaming handlers for document refresh status.
/// </summary>
internal static class DocumentRefreshSseHelper
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

#pragma warning disable CA1031 // Do not catch general exception types
    public static Delegate CreateStreamHandler(string documentType)
    {
        return async (
            DocumentRefreshChannelManager channelManager,
            HybridCache cache,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            return Results.Stream(
                async (Stream stream) =>
                {
                    var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                    try
                    {
                        logger.LogClientConnected(documentType);

                        // Send initial state if available from cache
                        await SendInitialStateAsync(documentType, writer, cache, cancellationToken);

                        // Get or create channel for this document type
                        var channelReader = channelManager.GetOrCreateChannel(documentType);

                        // Stream updates from channel
                        await foreach (var statusEvent in channelReader.ReadAllAsync(cancellationToken))
                        {
                            await WriteSseEventAsync(writer, statusEvent, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        logger.LogClientDisconnected(ex, documentType);
                    }
                    catch (Exception ex)
                    {
                        logger.LogStreamError(ex, documentType);
                    }
                    finally
                    {
                        channelManager.ReleaseListener(documentType);
                        await writer.DisposeAsync();
                    }
                },
                contentType: "text/event-stream");
        };
    }
#pragma warning restore CA1031

    private static async Task SendInitialStateAsync(
        string documentType,
        StreamWriter writer,
        HybridCache cache,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"{documentType}:refresh:current";

        DocumentRefreshJobState? state = await cache.GetOrCreateAsync(
            cacheKey,
            _ => ValueTask.FromResult<DocumentRefreshJobState?>(null),
            tags: [documentType, "document-refresh-state"],
            cancellationToken: cancellationToken);

        if (state is not null)
        {
            var initialEvent = DocumentRefreshStatusEvent.FromStatus(state.Status, state.ErrorMessage);
            await WriteSseEventAsync(writer, initialEvent, cancellationToken);
        }
    }

    private static async Task WriteSseEventAsync(
        StreamWriter writer,
        DocumentRefreshStatusEvent statusEvent,
        CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(statusEvent, s_jsonOptions);
        await writer.WriteLineAsync($"data: {json}".AsMemory(), cancellationToken);
        await writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, cancellationToken);
    }
}
