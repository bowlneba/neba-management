using System.Net.Mime;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Contracts;
using Neba.Infrastructure.Documents;
using Neba.Infrastructure.Http;
using Neba.Website.Application.Tournaments;
using Neba.Website.Application.Tournaments.ListTournaments;
using Neba.Website.Application.Tournaments.TournamentRules;
using Neba.Website.Contracts.Tournaments;
using Neba.Website.Endpoints.Documents;

namespace Neba.Website.Endpoints.Tournaments;

#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable S2325 // Extension methods should be static

internal static class TournamentEndpoints
{
    private const string TournamentsTag = "tournaments";

    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTournamentEndpoints()
        {
            RouteGroupBuilder tournamentGroup = app
                .MapGroup("/tournaments")
                .WithTags(TournamentsTag, WebsiteEndpoints.WebsiteTag)
                .AllowAnonymous();

            tournamentGroup
                .MapGetFutureTournamentsEndpoint()
                .MapGetTournamentsInAYearEndpoint();

            RouteGroupBuilder tournamentRulesGroup = tournamentGroup
                .MapGroup("/rules")
                .WithTags(TournamentsTag, WebsiteEndpoints.WebsiteTag, "documents")
                .AllowAnonymous();

            tournamentRulesGroup
                .MapGetTournamentRulesEndpoint()
                .MapRefreshTournamentRulesCacheEndpoint()
                .MapTournamentRulesRefreshStatusSseEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetFutureTournamentsEndpoint()
        {
            app.MapGet(
                "/future",
                async (
                    IQueryHandler<ListFutureTournamentsQuery, IReadOnlyCollection<TournamentSummaryDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new ListFutureTournamentsQuery();

                    IReadOnlyCollection<TournamentSummaryDto> result = await queryHandler.HandleAsync(query, cancellationToken);

                    IReadOnlyCollection<TournamentSummaryResponse> response = result.Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetFutureTournaments")
                .WithSummary("Get all future NEBA tournaments.")
                .WithDescription("Retrieves a list of all upcoming NEBA tournaments.")
                .Produces<CollectionResponse<TournamentSummaryResponse>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
                .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
                .WithTags(TournamentsTag, WebsiteEndpoints.WebsiteTag);

            return app;
        }

        private IEndpointRouteBuilder MapGetTournamentsInAYearEndpoint()
        {
            app.MapGet(
                "/{year:int}",
                async (
                    int year,
                    IQueryHandler<ListTournamentInAYearQuery, IReadOnlyCollection<TournamentSummaryDto>> queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var query = new ListTournamentInAYearQuery { Year = year };

                    IReadOnlyCollection<TournamentSummaryDto> result = await queryHandler.HandleAsync(query, cancellationToken);

                    IReadOnlyCollection<TournamentSummaryResponse> response = result.Select(dto => dto.ToResponseModel()).ToList();

                    return TypedResults.Ok(CollectionResponse.Create(response));
                })
                .WithName("GetTournamentsInAYear")
                .WithSummary("Get all NEBA tournaments in a specific year.")
                .WithDescription("Retrieves a list of all NEBA tournaments that took place in the specified year.")
                .Produces<CollectionResponse<TournamentSummaryResponse>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
                .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
                .WithTags(TournamentsTag, WebsiteEndpoints.WebsiteTag);

            return app;
        }

        private IEndpointRouteBuilder MapGetTournamentRulesEndpoint()
        {
            app.MapGet("/", async (
                IQueryHandler<GetTournamentRulesQuery, DocumentDto> queryHandler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetTournamentRulesQuery();

                DocumentDto result = await queryHandler.HandleAsync(query, cancellationToken);

                return TypedResults.Ok(result.ToStringResponse());
            })
            .WithName("GetTournamentRules")
            .WithSummary("Get the tournament rules document.")
            .WithDescription("Retrieves the tournament rules document")
            .Produces<DocumentResponse<string>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
            .WithTags(TournamentsTag, WebsiteEndpoints.WebsiteTag, "documents");

            return app;
        }

        private IEndpointRouteBuilder MapRefreshTournamentRulesCacheEndpoint()
        {
            app.MapPost(
                "/refresh",
                async (
                    ICommandHandler<RefreshTournamentRulesCacheCommand, string> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RefreshTournamentRulesCacheCommand();
                    ErrorOr<string> jobIdResult = await commandHandler.HandleAsync(command, cancellationToken);

                    if (jobIdResult.IsError)
                    {
                        return jobIdResult.Problem();
                    }

                    return TypedResults.Ok(ApiResponse.Create(jobIdResult.Value));
                })
                .WithName("RefreshTournamentRulesCache")
                .WithSummary("Refresh the cached tournament rules document.")
                .WithDescription("Refreshes the cached version of the tournament rules document.")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
                .WithTags(TournamentsTag, WebsiteEndpoints.WebsiteTag);

            return app;
        }

        private IEndpointRouteBuilder MapTournamentRulesRefreshStatusSseEndpoint()
        {
            app.MapGet(
                "/refresh/status",
                DocumentRefreshSseStreamHandler.CreateStreamHandler("tournament-rules"))
                .WithName("TournamentRulesRefreshStatus")
                .WithSummary("Stream tournament rules document refresh status updates via SSE")
                .WithDescription("Subscribes to real-time status updates for tournament rules document refresh operations using Server-Sent Events.")
                .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
                .WithTags(TournamentsTag, WebsiteEndpoints.WebsiteTag, "sse");

            return app;
        }
    }
}
